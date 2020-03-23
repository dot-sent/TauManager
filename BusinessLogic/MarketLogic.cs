using System.Linq;
using Microsoft.EntityFrameworkCore;
using TauManager.Models;
using TauManager.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using TauManager.Utils;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace TauManager.BusinessLogic
{
    public class MarketLogic : IMarketLogic
    {
        public TauDbContext _dbContext { get; set; }
        private ITauHeadClient _tauHead { get; set; }
        public MarketLogic(TauDbContext dbContext, ITauHeadClient tauHead)
        {
            _dbContext = dbContext;
            _tauHead = tauHead;
        }
        public MarketIndexViewModel GetIndexViewModel(int? playerId, MarketIndexParamsViewModel filters)
        {
            var ownAds = _dbContext.MarketAd.Where(ad => ad.AuthorId == playerId);
            var model = new MarketIndexViewModel
            {
                OwnActiveAds = ownAds.Count(ad => ad.Active),
                OwnInactiveAds = ownAds.Count(ad => !ad.Active),
                OwnAdReactions = ownAds.Sum(ad => ad.Reactions.Count()),
                ItemTypes = EnumExtensions.ToDictionary<int>(typeof(Item.ItemType)),
                WeaponTypes = EnumExtensions.ToDictionary<int>(typeof(Item.ItemWeaponType)),
                Filters = filters,
            };
            
            if (filters.HasItemLevelFilters)
            {
                var items = _dbContext.MarketAdBundleItem
                    .Include(bi => bi.Item)
                    .Include(bi => bi.Bundle)
                    .ThenInclude(b => b.Ad)
                    .ThenInclude(a => a.Author)
                    .Where(i => 
                        // Item-level filters
                        (string.IsNullOrWhiteSpace(filters.NamePart) || i.Item.Name.Contains(filters.NamePart)) &&
                        (filters.ItemRarities == null || filters.ItemRarities.Count() == 0 || filters.ItemRarities.Contains(i.Item.Rarity)) &&
                        (filters.ItemTypes == null || filters.ItemTypes.Count() == 0 || filters.ItemTypes.Contains(i.Item.Type)) &&
                        (filters.WeaponTypes == null || filters.WeaponTypes.Count() == 0 ||
                            (i.Item.WeaponType != null && filters.WeaponTypes.Contains(i.Item.WeaponType.Value))) && 
                        (filters.WeaponRanges == null || filters.WeaponRanges.Count() == 0 ||
                            (i.Item.WeaponRange.HasValue && filters.WeaponRanges.Contains(i.Item.WeaponRange.Value))) && 
                        (filters.MinEnergy == null || filters.MinEnergy <= 0 || 
                            (i.Item.Energy.HasValue && i.Item.Energy >= filters.MinEnergy)) &&
                        (filters.MinImpact == null || filters.MinImpact <= 0 || 
                            (i.Item.Impact.HasValue && i.Item.Impact >= filters.MinImpact)) &&
                        (filters.MinPiercing == null || filters.MinPiercing <= 0 || 
                            (i.Item.Piercing.HasValue && i.Item.Piercing >= filters.MinPiercing)) &&
                        (filters.MinTier == null || filters.MinTier <= 1 || i.Item.Tier >= filters.MinTier) &&
                        (filters.MaxTier == null || filters.MaxTier >= 5 || i.Item.Tier <= filters.MaxTier) &&
                        // Bundle-level filters
                        (filters.AdTypes == null || filters.AdTypes.Count() == 0 ||
                            (filters.AdTypes != null && filters.AdTypes.Contains(MarketAd.AdType.Buy) && i.Bundle.Type == MarketAdBundle.BundleType.Request) ||
                            (filters.AdTypes != null && 
                                (filters.AdTypes.Contains(MarketAd.AdType.Sell) || filters.AdTypes.Contains(MarketAd.AdType.Lend)) &&
                                i.Bundle.Type == MarketAdBundle.BundleType.Offer)) && 
                        // Ad-level filters
                        (filters.AdTypes == null || filters.AdTypes.Count() == 0 || filters.AdTypes.Contains(i.Bundle.Ad.Type)) &&
                        (i.Bundle.Ad.Active) &&
                        // Generic conditions
                        ((i.Bundle.Type == MarketAdBundle.BundleType.Offer && (i.Bundle.Ad.Type == MarketAd.AdType.Sell || i.Bundle.Ad.Type == MarketAd.AdType.Lend)) || 
                            (i.Bundle.Type == MarketAdBundle.BundleType.Request && i.Bundle.Ad.Type == MarketAd.AdType.Buy))
                        )
                    .ToList();
                var allAds = items.Select(i => i.Bundle.Ad)
                    .Distinct();
                var allAdsSorted = filters.Sort == MarketIndexParamsViewModel.SortOrder.DateAscending ?
                    allAds.OrderBy(ad => ad.PlacementDate) :
                    allAds.OrderByDescending(ad => ad.PlacementDate);
                model.OfferAds = allAdsSorted.Where(ad => ad.Type == MarketAd.AdType.Sell || ad.Type == MarketAd.AdType.Lend)
                    .Select(ad => new MarketAdViewModel(ad));
                model.AskAds = allAdsSorted.Where(ad => ad.Type == MarketAd.AdType.Buy)
                    .Select(ad => new MarketAdViewModel(ad));
            } else {
                var allAds = _dbContext.MarketAd
                    .Include(ad => ad.Author)
                    .Include(ad => ad.Bundles)
                    .ThenInclude(ab => ab.Items)
                    .ThenInclude(abi => abi.Item)
                    .Where(ad => 
                        (filters.AdTypes == null || filters.AdTypes.Count() == 0 || filters.AdTypes.Contains(ad.Type)) &&
                        ad.Active)
                    .ToList();

                var allAdsSorted = filters.Sort == MarketIndexParamsViewModel.SortOrder.DateAscending ?
                    allAds.OrderBy(ad => ad.PlacementDate) :
                    allAds.OrderByDescending(ad => ad.PlacementDate);

                model.OfferAds = allAdsSorted.Where(ad => ad.Type == MarketAd.AdType.Sell || ad.Type == MarketAd.AdType.Lend)
                    .Select(ad => new MarketAdViewModel(ad));
                model.AskAds = allAdsSorted.Where(ad => ad.Type == MarketAd.AdType.Buy)
                    .Select(ad => new MarketAdViewModel(ad));
            }
            return model;
        }

        public async Task<PlayerAdsViewModel> GetMyAds(int? playerId)
        {
            var player = await _dbContext.Player.SingleOrDefaultAsync(p => p.Id == playerId);
            if (player == null) return null;
            return new PlayerAdsViewModel{
                Ads = _dbContext.MarketAd.Where(ad => ad.AuthorId == player.Id)
            };
        }

        public async Task<ActionResponseViewModel> EditAd(int id, int? playerId, int adType, string offer, string request, string description, bool active)
        {
            if (adType > byte.MaxValue || adType < byte.MinValue || !Enum.IsDefined(typeof(MarketAd.AdType), (byte)adType)) return ActionResponseViewModel.Error();
            var player = await _dbContext.Player.SingleOrDefaultAsync(p => p.Id == playerId);
            if (player == null) return ActionResponseViewModel.Error();
            var marketAd = await _dbContext.MarketAd.SingleOrDefaultAsync(ad => ad.Id == id && ad.AuthorId == player.Id);
            if (id > 0 && marketAd == null) return ActionResponseViewModel.Error("Ad doesn't exist or you don't have access to edit it.");
            if (id == 0)
            {
                marketAd = new MarketAd 
                {
                    Id = 0,
                    AuthorId = player.Id,
                    Bundles = new List<MarketAdBundle>(),
                    PlacementDate = DateTime.Now,
                };
                _dbContext.Add(marketAd);
            }

            marketAd.Type = (MarketAd.AdType)adType;
            marketAd.Active = active;
            marketAd.Description = description;

            var parsedOffer = await ParseMarketAdPart(offer);
            var parsedRequest = await ParseMarketAdPart(request);

            if (!parsedOffer.Success || !parsedRequest.Success)
            {
                if (id == 0)
                {
                    _dbContext.Remove(marketAd);
                }
                return ActionResponseViewModel.Error(parsedOffer.ParseMessage + " " + parsedRequest.ParseMessage);
            }
            List<MarketAdBundle> allBundles = new List<MarketAdBundle>();
            // processing offer
            marketAd.OfferType = MarketAd.TransactionElementType.Specific;
            if (parsedOffer.IsFree) marketAd.OfferType = MarketAd.TransactionElementType.Nothing;
            if (parsedOffer.IsAny) marketAd.OfferType = MarketAd.TransactionElementType.Bid;
            if (marketAd.OfferType == MarketAd.TransactionElementType.Specific)
            {
                var offerBundles = parsedOffer.ParsedBundles;
                offerBundles.ForEach(b => b.Type = MarketAdBundle.BundleType.Offer);
                allBundles.AddRange(offerBundles);
            }

            //processing request
            marketAd.RequestType = MarketAd.TransactionElementType.Specific;
            if (parsedRequest.IsFree) marketAd.RequestType = MarketAd.TransactionElementType.Nothing;
            if (parsedRequest.IsAny) marketAd.RequestType = MarketAd.TransactionElementType.Bid;
            if (marketAd.RequestType == MarketAd.TransactionElementType.Specific)
            {
                var requestBundles = parsedRequest.ParsedBundles;
                requestBundles.ForEach(b => b.Type = MarketAdBundle.BundleType.Request);
                allBundles.AddRange(requestBundles);
            }

            if (marketAd.Bundles != null && marketAd.Bundles.Count() > 0)
            {
                _dbContext.RemoveRange(marketAd.Bundles);
                marketAd.Bundles = null;
            }
            marketAd.Bundles = allBundles;

            await _dbContext.SaveChangesAsync();

            return ActionResponseViewModel.Success(id: marketAd.Id);
        }

        #region Parsing market ad info

        /*
            Semantically it would make sense to keep parsing and serialization logic
            together in one place. A logical place for this would be a POCO class.
            However, in this case properly parsing the market ad bundle has several
            dependencies that should not be introcuced into POCO: specifically, access
            to the tau.Item database table and TauHead util. Hence parsing lives here, while
            serialization logic is located in MarketAd POCO class.
         */

        private class MarketAdParseInfo
        {
            public bool IsFree { get; set; }
            public bool IsAny { get; set; }
            public string ParseMessage { get; set; }
            public bool Success { get; set; }
            public List<MarketAdBundle> ParsedBundles { get; set; }
        }
        private readonly Regex _regexQuantity = new Regex(@"^(.*)\sx\s?(\d+)|(\d+)\s?x\s(.*)$");
        private readonly Regex _regexAny = new Regex(@"^\s?(\?|\*)\s?$$");
        private readonly Regex _regexFree = new Regex(@"^\s?(0|free)?\s?$");
        private readonly Regex _regexCredits = new Regex(@"^([\d]+(?:[.][\d]+)?)\s?(?:cr|c)?$");
        private readonly Regex _regexUrl = new Regex(@"^(?:(?:https://alpha.taustation.space/item/)|(?:https://www.tauhead.com/item/))(.+)$");
        private readonly Regex _regexRation = new Regex(@"^[Tt]([\d]+)\s?(?:ration|R|Ration|r)$");
        private async Task<MarketAdParseInfo> ParseMarketAdPart(string input)
        {
            var parseResult = new MarketAdParseInfo {
                Success = true,
            };
            if (input != null && input.Length > 1024)
            {
                parseResult.Success = false;
                parseResult.ParseMessage = "Message too long: the current limit is 1024 characters\n";
                return parseResult;
            }
            if (input == null || _regexFree.IsMatch(input))
            {
                parseResult.IsFree = true;
            } else if (_regexAny.IsMatch(input))
            {
                parseResult.IsAny = true;
            } else {
                var logicalAlternatives = input.Split(new string[] { " or ", " || " }, StringSplitOptions.None);
                parseResult.ParsedBundles = new List<MarketAdBundle>();
                foreach (var _alternative in logicalAlternatives)
                {
                    var alternative = _alternative.Trim();
                    var components = alternative.Split(new string[] { " and ", " && " }, StringSplitOptions.None);
                    var newBundle = new MarketAdBundle();
                    var bundleItems = new List<MarketAdBundleItem>();
                    foreach (var _component in components)
                    {
                        var component = _component.Trim();
                        var m = _regexCredits.Match(component);
                        
                        if (m.Success)
                        {
                            decimal credits;
                            var parseSuccess = decimal.TryParse(m.Groups[1].Value, out credits);
                            if (parseSuccess && credits > 0) {
                                if (newBundle.Credits > 0)
                                {
                                    parseResult.Success = false;
                                    parseResult.ParseMessage += "Attempt to set credits amount more than once.\n";
                                    return parseResult;
                                } else {
                                    newBundle.Credits = credits;
                                }
                            }
                        } else { // it's not credits, so it MUST be an item
                            // Checking for quantity first; if not found, set default to 1
                            // and item to full component string
                            var quantity = 1;
                            var item = component;
                            var newBundleItem = new MarketAdBundleItem();
                            m = _regexQuantity.Match(component);
                            if (m.Success)
                            {
                                if (m.Groups[1].Success) {
                                    var parseSuccess = int.TryParse(m.Groups[2].Value, out quantity);
                                    if (!parseSuccess)
                                    {
                                        parseResult.Success = false;
                                        parseResult.ParseMessage += "Failed to parse the quantity component: " + m.Groups[2].Value + "\n";
                                        return parseResult;
                                    }
                                    item = m.Groups[1].Value;
                                } else if (m.Groups[3].Success) {
                                    var parseSuccess = int.TryParse(m.Groups[3].Value, out quantity);
                                    if (!parseSuccess)
                                    {
                                        parseResult.Success = false;
                                        parseResult.ParseMessage += "Failed to parse the quantity component: " + m.Groups[3].Value + "\n";
                                        return parseResult;
                                    }
                                    item = m.Groups[4].Value;
                                }
                            }
                            // at this point we have quantity and item set, so we can continue parsing item.
                            m = _regexUrl.Match(item);
                            Item itemObj = null;
                            if (m.Success)
                            {
                                var slug = m.Groups[1].Value.ToLower();
                                itemObj = _dbContext.Item.SingleOrDefault(i => i.Slug == slug);
                                if (itemObj == null) 
                                {
                                    itemObj = await _tauHead.GetItemData(_tauHead.UrlFromSlug(slug));
                                }
                                if (itemObj == null)
                                {
                                    parseResult.Success = false;
                                    parseResult.ParseMessage = "Can't find the item with slug " + slug + " on TauHead or in the local database\n";
                                    return parseResult;
                                }
                            } else {// if it's not a URL, it must be a ration or an item name
                                m = _regexRation.Match(item);
                                if (m.Success)
                                {
                                    var tier = 0;
                                    var parseSuccess = int.TryParse(m.Groups[1].Value, out tier);
                                    if (!parseSuccess)
                                    {
                                        parseResult.Success = false;
                                        parseResult.ParseMessage = "Can't parse ration tier: " + m.Groups[1].Value + "\n";
                                        return parseResult;
                                    }
                                    var slug = "ration-" + tier.ToString();
                                    itemObj = _dbContext.Item.SingleOrDefault(i => i.Slug == slug);
                                    if (itemObj == null)
                                    {
                                        itemObj = await _tauHead.GetItemData(_tauHead.UrlFromSlug(slug));
                                    }
                                    if (itemObj == null)
                                    {
                                        parseResult.Success = false;
                                        parseResult.ParseMessage = "Can't find the item with slug " + slug + " on TauHead or in the local database\n";
                                        return parseResult;
                                    }
                                } else {
                                    itemObj = _dbContext.Item.SingleOrDefault(i => i.Name == item);
                                    if (itemObj == null)
                                    {
                                        parseResult.Success = false;
                                        parseResult.ParseMessage = "Can't find the item with name '" + item + "' in the local database, please fix it or provide in-game or TauHead URL instead.\n";
                                        return parseResult;
                                    }
                                }
                            }
                            // at this point itemObj is set together with quantity, so we can create a BundleItem.
                            newBundleItem.Item = itemObj;
                            newBundleItem.Quantity = quantity;
                            bundleItems.Add(newBundleItem);
                        }
                    }
                    newBundle.Items = bundleItems;
                    parseResult.ParsedBundles.Add(newBundle);
                }
            }

            return parseResult;
        }
        #endregion

        public MarketAd GetAdById(int id, int? playerId)
        {
            return _dbContext.MarketAd.SingleOrDefault(ad => ad.Id == id && ad.AuthorId == playerId);
        }

        public async Task<ActionResponseViewModel> SetAdActive(int id, int? playerId, bool active)
        {
            var player = await _dbContext.Player.SingleOrDefaultAsync(p => p.Id == playerId);
            if (player == null) return ActionResponseViewModel.Error();
            var marketAd = await _dbContext.MarketAd.SingleOrDefaultAsync(ad => ad.Id == id && ad.AuthorId == player.Id);
            if (marketAd == null) return ActionResponseViewModel.Error("Ad doesn't exist or you don't have access to edit it.");
            marketAd.Active = active;
            await _dbContext.SaveChangesAsync();
            return ActionResponseViewModel.Success();
        }

        public async Task<ActionResponseViewModel> RemoveAd(int id, int? playerId)
        {
            var player = await _dbContext.Player.SingleOrDefaultAsync(p => p.Id == playerId);
            if (player == null) return ActionResponseViewModel.Error();
            var marketAd = await _dbContext.MarketAd.SingleOrDefaultAsync(ad => ad.Id == id && ad.AuthorId == player.Id);
            if (marketAd == null) return ActionResponseViewModel.Error("Ad doesn't exist or you don't have access to edit it.");
            _dbContext.Remove(marketAd);
            await _dbContext.SaveChangesAsync();
            return ActionResponseViewModel.Success();
        }

        public async Task<ActionResponseViewModel> ImportMarketCSV(IFormFile inputFile, int? playerId)
        {
            var player = await _dbContext.Player.SingleOrDefaultAsync(p => p.Id == playerId);
            if (player == null) return ActionResponseViewModel.Error();
            var reader = new StreamReader(inputFile.OpenReadStream());
            string fileContents = reader.ReadToEnd();
            reader.Close();
            var lines = fileContents.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            if (lines.Count() + _dbContext.MarketAd.Count(ad => ad.AuthorId == playerId) > 1000)
            {
                return ActionResponseViewModel.Error("This file will bring your number of ads above 1000. IMHO, this is more than enough :) Please remove excessive lines or import ads above 1000 manually.");
            } else {
                var counter = 1;
                var messages = "";
                var successCount = 0;
                var failCount = 0;
                foreach(var line in lines)
                {
                    var fields = line.Split(';');
                    if (fields.Count() != 3 && fields.Count() != 4) {
                        messages += String.Format("Error parsing line {0}: found {1} fields, expected 3 or 4.\n", counter, fields.Count());
                        failCount++;
                    } else {
                        MarketAd.AdType adType;
                        // TODO: Remove magic numbers
                        var typeParseResult = Enum.TryParse<MarketAd.AdType>(fields[0], true, out adType);
                        if(!typeParseResult)
                        { 
                            messages += String.Format("Error parsing line {0}: ad type {1} not recognized.\n", counter, fields[0]); 
                            failCount++;
                        } else {
                            var adResult = await EditAd(0, playerId, (int)adType, fields[1], fields[2], fields.Count() == 4 ? fields[3] : String.Empty, true);
                            if (!adResult.Result)
                            {
                                messages += String.Format("Error parsing line {0}: {1}\n", counter, adResult.Message);
                                failCount++;
                            } else {
                                successCount++;
                            }
                        }
                    }
                    counter++;
                }
                messages = string.Format("Total lines: {0}; successfully imported ads: {1}; errors: {2}\n\n", lines.Count(), successCount, failCount) + messages;
                return ActionResponseViewModel.Success(messages);
            }
        }
    }
}