using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using TauManager.Models;
using TauManager.Utils;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public class CampaignLogic : ICampaignLogic
    {
        private TauDbContext _dbContext { get; set; }
        private ITauHeadClient _tauHead { get; set; }
        public CampaignLogic(TauDbContext dbContext, ITauHeadClient tauHead)
        {
            _dbContext = dbContext;
            _tauHead = tauHead;
        }
        public CampaignOverviewViewModel GetCampaignOverview(int playerId, bool showLootApplyButton, bool showLootEditControls, bool showAwardButton, int syndicateId)
        {
            var players = _dbContext.Player.Where(p => p.SyndicateId == syndicateId).OrderBy(p => p.Name).AsEnumerable();
            var currentPlayer = _dbContext.Player.SingleOrDefault(p => p.Id == playerId);
            var playersOrdered = _dbContext.PlayerListPositionHistory.GroupBy(plph => plph.PlayerId)
                .Select(g => new {
                    PlayerId =  g.Key,
                    Position = g.Max(ph => ph.Id)
                })
                .Join(
                    _dbContext.Player,
                    ph => ph.PlayerId,
                    p => p.Id,
                    (ph, p) => new { Player = p, Position = ph.Position }
                )
                .Where(php => php.Player.SyndicateId == syndicateId)
                .OrderBy(p => p.Position)
                .Select(p => p.Player);
            var playerPositions = playersOrdered.Select(p => p.Id).ToList();
            var activePlayerPositions = playersOrdered.Where(p => p.Active).Select(p => p.Id).ToList();
            var model = new CampaignOverviewViewModel{
                CurrentCampaigns = _dbContext.Campaign.Where(c => 
                    c.SyndicateId == syndicateId &&
                    (c.Status == Campaign.CampaignStatus.InProgress ||
                    c.Status == Campaign.CampaignStatus.Abandoned))
                    .OrderByDescending(c => c.UTCDateTime).ToList(),
                PastCampaigns = _dbContext.Campaign.Where(c => 
                    c.SyndicateId == syndicateId &&
                    (c.Status == Campaign.CampaignStatus.Completed ||
                    c.Status == Campaign.CampaignStatus.Failed ||
                    c.Status == Campaign.CampaignStatus.Skipped))
                    .OrderByDescending(c => c.UTCDateTime).ToList(),
                FutureCampaigns = _dbContext.Campaign.Where(c => 
                    c.SyndicateId == syndicateId &&
                    (c.Status == Campaign.CampaignStatus.Assigned ||
                    c.Status == Campaign.CampaignStatus.Planned ||
                    c.Status == Campaign.CampaignStatus.Unknown))
                    .OrderByDescending(c => c.UTCDateTime).ToList(),
                LootToDistribute = _dbContext.CampaignLoot
                    .Where(cl => cl.Status == CampaignLoot.CampaignLootStatus.Undistributed)
                    .Include(l => l.Requests)
                    .ThenInclude(lr => lr.RequestedFor)
                    .Include(l => l.Item)
                    .Join(
                        _dbContext.Campaign,
                        cl => cl.CampaignId,
                        c => c.Id,
                        (cl, c) => new {CampaignLoot = cl, Campaign = c}
                    )
                    .Where(clc => clc.Campaign.SyndicateId == syndicateId)
                    .ToList()
                    .Select(clc => clc.CampaignLoot)
                    .Select(l => new LootItemViewModel
                    {
                        Loot = l,
                        ShowApplyButton = showLootApplyButton,
                        ShowEditControls = showLootEditControls,
                        ShowAwardButton = showAwardButton,
                        RequestExists = l.Requests.Any(r => r.RequestedForId == playerId),
                        Request = l.Requests.SingleOrDefault(r => r.RequestedForId == playerId),
                        LootStatuses = EnumExtensions.ToDictionary<int>(typeof(CampaignLoot.CampaignLootStatus)),
                        Players = players,
                        AllRequests = l.Requests
                            .Where(r => r.RequestedFor.SyndicateId == syndicateId)
                            .Select(r => new
                            {
                                Player = r.RequestedFor,
                                Position = activePlayerPositions.IndexOf(r.RequestedForId) + 1,
                            })
                            .OrderBy(pp => pp.Position)
                            .ToDictionary(
                                pp => pp.Position,
                                pp => pp.Player.Name
                            ),
                        SpecialRequests = l.Requests
                            .Where(r => r.RequestedFor.SyndicateId == syndicateId)
                            .Where(r => !String.IsNullOrEmpty(r.SpecialOfferDescription))
                            .Select(r => new
                            {
                                Offer = r.SpecialOfferDescription,
                                Position = activePlayerPositions.IndexOf(r.RequestedForId) + 1,
                            })
                            .OrderBy(pp => pp.Position)
                            .ToDictionary(
                                pp => pp.Position,
                                pp => pp.Offer
                            ),
                        TierRestriction = l.Item.Tier > currentPlayer.Tier,
                    })
                    .ToList(),
                MySignups = _dbContext.CampaignSignup
                    .Where(cs => cs.PlayerId == playerId)
                    .ToDictionary(cs => cs.CampaignId, cs => 1),
                MyAttendance = _dbContext.CampaignAttendance
                    .Where(a => a.PlayerId == playerId)
                    .ToDictionary( a => a.CampaignId, a => 1),
                MyPosition = activePlayerPositions.IndexOf(playerId) + 1,
                LootStatuses = EnumExtensions.ToDictionary<int>(typeof(CampaignLoot.CampaignLootStatus)),
                PlayerId = playerId
            };
            return model;
        }

        public CampaignDetailsViewModel GetCampaignById(int id, bool showLootApplyButton, bool showLootEditControls, int syndicateId)
        {
            var campaign =  _dbContext.Campaign
                .Include(c => c.Signups)
                .Include(c => c.Loot)
                .SingleOrDefault(c => c.Id == id);
            if (campaign == null || campaign.SyndicateId != syndicateId) return null;
            return GetExtendedCampaign(campaign, showLootApplyButton, showLootEditControls);
        }

        public CampaignDetailsViewModel GetExtendedCampaign(Campaign campaign, bool showLootApplyButton = false, bool showLootEditControls = false)
        {
            var players = _dbContext.Player.Where(p => p.SyndicateId == campaign.SyndicateId).OrderBy(p => p.Name).AsEnumerable();
            var model = new CampaignDetailsViewModel
            {
                Campaign = campaign,
                Players = players,
                DifficultyLevels = EnumExtensions.ToDictionary<int>(typeof(Campaign.CampaignDifficulty)),
                Statuses = EnumExtensions.ToDictionary<int>(typeof(Campaign.CampaignStatus)),
                Loot = campaign.Loot == null ? null : campaign.Loot.Select(l => new LootItemViewModel{
                    Loot = l,
                    ShowApplyButton = showLootApplyButton,
                    ShowEditControls = showLootEditControls,
                    LootStatuses = EnumExtensions.ToDictionary<int>(typeof(CampaignLoot.CampaignLootStatus)),
                    Players = players,
                }),
                KnownEpics = _dbContext.Item.Where(i => i.Rarity == Item.ItemRarity.Epic),
            };
            return model;
        }

        public async Task<Campaign> CreateOrEditCampaign(Campaign campaign, int syndicateId)
        {
            if (campaign.SyndicateId == null) campaign.SyndicateId = syndicateId;
            if (campaign.SyndicateId != syndicateId) return null;
            if (campaign.ManagerId.HasValue && campaign.ManagerId.Value == 0)
            {
                campaign.ManagerId = null;
            }
            var playersToNotifyOfNewCampaign = new List<int>();
            var playersToNotifyOfUpdatedCampaign = new List<int>();
            var playersToNotifyOfCampaignSoon = new List<int>();
            if (campaign.Id == 0)
            {
                _dbContext.Add(campaign);
                playersToNotifyOfNewCampaign.AddRange(_dbContext.Player
                    .Where(p => p.SyndicateId == campaign.SyndicateId &&
                        p.NotificationSettings.HasFlag(Player.NotificationFlags.NewCampaign))
                    .Select(p => p.Id));
                playersToNotifyOfCampaignSoon.AddRange(_dbContext.Player
                    .Where(p => p.SyndicateId == campaign.SyndicateId &&
                        p.NotificationSettings.HasFlag(Player.NotificationFlags.CampaignSoonAll))
                    .Select(p => p.Id));
            } else {
                var campaignExists = _dbContext.Campaign
                    .AsNoTracking() // We are going to just save the object from the input parameter
                    .FirstOrDefault(c => c.Id == campaign.Id && c.SyndicateId == syndicateId);
                if (campaignExists == null) return null;
                _dbContext.Update(campaign);
                playersToNotifyOfUpdatedCampaign.AddRange(_dbContext.Player
                    .Where(p => p.SyndicateId == campaign.SyndicateId &&
                        p.NotificationSettings.HasFlag(Player.NotificationFlags.CampaignUpdatedAll))
                    .Select(p => p.Id));
                playersToNotifyOfUpdatedCampaign = playersToNotifyOfUpdatedCampaign.Union(
                    _dbContext.CampaignSignup
                        .Include(cs => cs.Player)
                        .Where(cs => cs.CampaignId == campaign.Id &&
                            cs.Player.NotificationSettings.HasFlag(Player.NotificationFlags.CampaignUpdatedIfSignedUp))
                        .Select(cs => cs.PlayerId)
                ).ToList();
                if (campaignExists.UTCDateString != campaign.UTCDateString)
                {
                    // Campaign time has been changed, we need to invalidate the notifications
                    var notificationsToRemove = _dbContext.Notification
                        .Where(n => n.Kind == NotificationKind.CampaignSoon &&
                            n.RelatedId == campaign.Id);
                    _dbContext.RemoveRange(notificationsToRemove);
                    playersToNotifyOfCampaignSoon.AddRange(_dbContext.Player
                        .Where(p => p.SyndicateId == campaign.SyndicateId &&
                            p.NotificationSettings.HasFlag(Player.NotificationFlags.CampaignSoonAll))
                        .Select(p => p.Id));
                    playersToNotifyOfCampaignSoon = playersToNotifyOfCampaignSoon.Union(
                        _dbContext.CampaignSignup
                        .Include(cs => cs.Player)
                        .Where(cs => cs.CampaignId == campaign.Id &&
                            cs.Player.NotificationSettings.HasFlag(Player.NotificationFlags.CampaignSoonIfSignedUp))
                        .Select(cs => cs.PlayerId)
                    ).ToList();
                }
            }
            await _dbContext.SaveChangesAsync();

            // Generate instant notifications
            foreach(var id in playersToNotifyOfNewCampaign)
            {
                _dbContext.Notification.Add(
                    new Notification{
                        RecipientId = id,
                        RelatedId = campaign.Id,
                        Kind = NotificationKind.NewCampaign,
                        SendAfter = DateTime.Now
                    }
                );
            }
            foreach(var id in playersToNotifyOfUpdatedCampaign)
            {
                _dbContext.Notification.Add(
                    new Notification{
                        RecipientId = id,
                        RelatedId = campaign.Id,
                        Kind = NotificationKind.CampaignUpdated,
                        SendAfter = DateTime.Now
                    }
                );
            }
            // Generate delayed notifications
            var sendAfter = campaign.UTCDateTime.Value.EnsureUTC().AddHours(-4);
            sendAfter = TimeZoneInfo.ConvertTimeFromUtc(sendAfter, TimeZoneInfo.Local);
            foreach(var id in playersToNotifyOfCampaignSoon)
            {
                _dbContext.Notification.Add(
                    new Notification{
                        RecipientId = id,
                        RelatedId = campaign.Id,
                        Kind = NotificationKind.CampaignSoon,
                        SendAfter = sendAfter
                    }
                );
            }
            await _dbContext.SaveChangesAsync();
            return campaign;
        }

        public async Task<LootItemViewModel> AddLootByTauheadURL(int campaignId, string url, bool showApplyButton = false, bool showLootEditControls=true)
        {
            var campaign = _dbContext.Campaign.SingleOrDefault(c => c.Id == campaignId);
            if (campaign == null)
            {
                return null;
            }
            var itemData = await _tauHead.GetItemData(url);
            if (itemData == null) // Failed to parse TauHead data or no connection
            {
                return null;
            }
            var existingItem = _dbContext.Item.SingleOrDefault(i => i.Slug == itemData.Slug);
            if (existingItem == null)
            {
                await _dbContext.AddAsync(itemData);
                //TODO: Not sure if needed here - might be logical to perform a save at the end of the method only
                await _dbContext.SaveChangesAsync(); 
                existingItem = itemData;
            }
            var campaignLootItem = new CampaignLoot{
                CampaignId = campaignId,
                ItemId = existingItem.Id,
                Status = CampaignLoot.CampaignLootStatus.Undistributed,
            };
            await _dbContext.AddAsync(campaignLootItem);
            await _dbContext.SaveChangesAsync();
            var players = _dbContext.Player.Where(p => p.SyndicateId == campaign.SyndicateId).OrderBy(p => p.Name).AsEnumerable();
            var model = new LootItemViewModel{
                Loot = campaignLootItem,
                ShowApplyButton = showApplyButton,
                ShowEditControls = showLootEditControls,
                LootStatuses = EnumExtensions.ToDictionary<int>(typeof(CampaignLoot.CampaignLootStatus)),
                Players = players,
            };
            return model;
        }

        public CampaignDetailsViewModel GetNewCampaign(int syndicateId)
        {
            var campaign = new Campaign{
                Difficulty = Campaign.CampaignDifficulty.Easy,
                UTCDateTime = DateTime.Now,
                Tiers = 0,
                ManagerId = null,
                Loot = new List<CampaignLoot>(),
                SyndicateId = syndicateId
            };
            return GetExtendedCampaign(campaign);
        }

        public async Task<CampaignPageParseResultViewModel> ParseCampaignPage(string fileContents, int campaignId)
        {
            var campaign = _dbContext.Campaign.SingleOrDefault(c => c.Id == campaignId);
            if (campaign == null) return null;

            var model = new CampaignPageParseResultViewModel {
                ErrorMessages = new List<string>(),
                SuccessMessages = new List<string>(),
                WarningMessages = new List<string>()
            };
            var document = new HtmlDocument();
            document.LoadHtml(fileContents);
            var epicImages = document.DocumentNode.SelectNodes("//div[contains(concat(\" \",normalize-space(@class),\" \"),\" epic \")]/img");
            var leaderboardLines = document.DocumentNode.SelectNodes("//div[@id=\"campaign-result-leaderboard\"]/table/tbody/tr");
            if (epicImages != null)
            {
                foreach (var epicNode in epicImages)
                {
                    var countBlock = epicNode.ParentNode.ParentNode.NextSibling.NextSibling;
                    var countLine = countBlock.Descendants("#text").FirstOrDefault().InnerText.Trim().TrimEnd(' ', 'x');
                    var count = 1;
                    if (!int.TryParse(countLine, out count))
                    {
                        count = 1;
                    }
                    var linkifyLinks = countBlock.Descendants("a");
                    if (linkifyLinks.Count() > 0)
                    {
                        var url = linkifyLinks.First().Attributes["href"].Value;
                        var urlParts = url.Split('/');
                        var slug = urlParts.Last();
                        var epic = _dbContext.Item.SingleOrDefault(i => i.Slug == slug);
                        if (epic == null) 
                        {
                            var tauHeadUrl = TauHead.UrlBase + "/" + urlParts.TakeLast(2).Aggregate((result, item) => result + "/" + item);
                            epic = await _tauHead.GetItemData(tauHeadUrl);
                            if (epic == null) // Item is missing from TauHead
                            {
                                model.ErrorMessages.Add(
                                    string.Format(
                                        "Item with slug {0} is missing from TauHead! Please contact Dotsent and firefu.",
                                         slug
                                    ));
                            } else {
                                model.SuccessMessages.Add(
                                    string.Format(
                                        "Item with slug {0} has been successfully imported from TauHead.",
                                        slug
                                    ));
                                await _dbContext.AddAsync(epic);
                            }
                        }
                        if (epic != null)
                        {
                            if (epic.Type == Item.ItemType.Armor || epic.Type == Item.ItemType.Weapon)
                            {
                                var existingLootCount = _dbContext.CampaignLoot.Count(cl => cl.ItemId == epic.Id && cl.CampaignId == campaignId);
                                while (existingLootCount < count)
                                {
                                    var loot = new CampaignLoot{
                                        CampaignId = campaignId,
                                        Status = CampaignLoot.CampaignLootStatus.Undistributed,
                                        ItemId = epic.Id,
                                    };
                                    await _dbContext.AddAsync(loot);
                                    existingLootCount++;
                                    model.SuccessMessages.Add(
                                        string.Format(
                                            "Item with slug {0} has been added to the loot list.",
                                            slug
                                        ));
                                }
                            } else {
                                model.WarningMessages.Add(
                                    string.Format(
                                        "Ignoring epic item with slug {0} since it's neither weapon nor armor.",
                                        slug
                                    ));
                            }
                        }
                    } else {
                        // This check is probably going to be very noisy since it will activate on items
                        // that Linkify doesn't decorate, yet they still have Epic frames

                        // model.WarningMessages.Add(
                        //     "Skipping epic item without URL. Perhaps, Linkify.user.js is not installed?"
                        // );
                    }
                }
            }
            if (leaderboardLines != null)
            {
                foreach (var playerLine in leaderboardLines)
                {
                    var cells = playerLine.Descendants("td").ToArray();
                    var player = _dbContext.Player.SingleOrDefault(p => p.Name == cells[2].InnerText);
                    if (player != null)
                    {
                        var attendance = _dbContext.CampaignAttendance.SingleOrDefault(a => a.PlayerId == player.Id && a.CampaignId == campaignId);
                        if (attendance == null)
                        {
                            attendance = new CampaignAttendance{
                                CampaignId = campaignId,
                                PlayerId = player.Id,
                            };
                            await _dbContext.AddAsync(attendance);
                        }
                    } else {
                        model.WarningMessages.Add(
                            string.Format(
                                "Player {0} does not seem to belong to the syndicate. Please ensure that members' list is up to date!",
                                cells[2].InnerText
                            ));
                    }
                }
            }
            campaign.Status = Campaign.CampaignStatus.Completed;
            await _dbContext.SaveChangesAsync();
            return model;
        }

        public async Task<bool> SetSignupStatus(int playerId, int campaignId, bool status)
        {
            var campaign = _dbContext.Campaign.SingleOrDefault(c => c.Id == campaignId);
            if (campaign == null) return false;
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == playerId);
            if (player == null) return false;
            var signup = _dbContext.CampaignSignup.SingleOrDefault(s => s.PlayerId == playerId && s.CampaignId == campaignId);
            if (signup == null)
            {
                if (!status) return false;
                signup = new CampaignSignup{
                    PlayerId = playerId,
                    CampaignId = campaignId,
                };
                await _dbContext.AddAsync(signup);
                if ((player.NotificationSettings & Player.NotificationFlags.CampaignSoonIfSignedUp)
                    == Player.NotificationFlags.CampaignSoonIfSignedUp &&
                    (player.NotificationSettings & Player.NotificationFlags.CampaignSoonAll) == 0)
                {
                    var sendAfter = campaign.UTCDateTime.Value.EnsureUTC().AddHours(-4);
                    sendAfter = TimeZoneInfo.ConvertTimeFromUtc(sendAfter, TimeZoneInfo.Local);
                    await _dbContext.AddAsync(
                        new Notification{
                            Kind = NotificationKind.CampaignSoon,
                            RecipientId = player.Id,
                            RelatedId = campaign.Id,
                            SendAfter = sendAfter,
                            Status = Notification.NotificationStatus.NotSent
                        }
                    );
                }
            } else {
                if (status) return false;
                _dbContext.Remove(signup);
                if ((player.NotificationSettings & Player.NotificationFlags.CampaignSoonIfSignedUp)
                    == Player.NotificationFlags.CampaignSoonIfSignedUp &&
                    (player.NotificationSettings & Player.NotificationFlags.CampaignSoonAll) == 0)
                {
                    var notificationsToRemove = _dbContext.Notification.
                        Where(n => n.RecipientId == player.Id &&
                            n.Kind == NotificationKind.CampaignSoon &&
                            n.RelatedId == campaign.Id);
                    _dbContext.RemoveRange(notificationsToRemove);
                }
            }
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // public async Task<bool> ParseAttendanceCSV(string fileContents)
        // {
        //     var lines = fileContents.Split("\n");
        //     var headerLine = lines[0].Split(',');
        //     foreach (var playerLine in lines.Skip(1))
        //     {
        //         var cells = playerLine.Split(',');
        //         var playerName = cells[0];
        //         var player = _dbContext.Player.SingleOrDefault(p => p.Name == playerName);
        //         if (player == null) continue;
        //         for (int i = 1; i < headerLine.Count(); i++)
        //         {
        //             if (cells[i] == "1")
        //             {
        //                 var campaignId = headerLine[i];
        //                 var campaign = _dbContext.Campaign.SingleOrDefault(c => c.Id.ToString() == campaignId);
        //                 if (campaign == null) continue;
        //                 var attendance = _dbContext.CampaignAttendance.SingleOrDefault(a => a.CampaignId == campaign.Id && a.PlayerId == player.Id);
        //                 if (attendance == null) {
        //                     attendance = new CampaignAttendance{
        //                         PlayerId = player.Id,
        //                         CampaignId = campaign.Id
        //                     };
        //                     await _dbContext.AddAsync(attendance);
        //                 }
        //             }
        //         }
        //     }
        //     await _dbContext.SaveChangesAsync();
        //     return true;
        // }

        public AttendanceViewModel GetCampaignAttendance(int? playerId, int syndicateId)
        {
            var model = new AttendanceViewModel();
            if (playerId.HasValue)
            {
                var playerExists = _dbContext.Player.Any(p => p.Id == playerId.Value);
                if (!playerExists)
                {
                    model.TotalAttendance = new Dictionary<int, int>();
                    model.Last10T5HardAttendance = new Dictionary<int, int>();
                    return model;
                }
            }
            var attendanceData = _dbContext.Player
                .Where(p => (!playerId.HasValue || p.Id == playerId.Value) && p.SyndicateId == syndicateId)
                .Join(
                    _dbContext.CampaignAttendance, 
                    p => p.Id, 
                    a => a.PlayerId,
                    (p, a) => new {
                        Player = p,
                        Attendance = a
                    })
                .Join(
                    _dbContext.Campaign,
                    p => p.Attendance.CampaignId,
                    c => c.Id,
                    (p, c) => new {
                        Player = p.Player,
                        Campaign = c
                    })
                .Where(pc => pc.Campaign.SyndicateId == syndicateId && !pc.Campaign.ExcludeFromLeaderboards)
                .ToList()
                .GroupBy(p => p.Player)
                .Select(
                    g => new {
                        Player = g.Key,
                        Camapaigns = g.Select(p => p.Campaign.Id).AsEnumerable()
                    }
                ).ToDictionary(
                    p => p.Player.Id,
                    v => v.Camapaigns
                );
            var totalCampaignCount = _dbContext.Campaign.Count(c => c.SyndicateId == syndicateId && !c.ExcludeFromLeaderboards);
            var allT5HardCampaigns = _dbContext.Campaign
                .OrderByDescending(c => c.UTCDateTime)
                .Where(c => (c.Difficulty == Campaign.CampaignDifficulty.Hard ||
                    c.Difficulty == Campaign.CampaignDifficulty.Extreme) &&
                    c.Tiers.HasValue && c.Tiers.Value > 15 &&
                    (c.Status == Campaign.CampaignStatus.Completed || c.Status == Campaign.CampaignStatus.Failed) &&
                    c.SyndicateId == syndicateId &&
                    !c.ExcludeFromLeaderboards)
                .Select(c => c.Id);
            var T5HardCampaigns = allT5HardCampaigns
                .ToDictionary(
                    c => c,
                    c => 1
                );
            var last10T5HardCampaigns = allT5HardCampaigns
                .Take(10)
                .ToDictionary(
                    c => c,
                    c => 1
                );

            model.TotalAttendance = attendanceData.ToDictionary(
                entry => entry.Key,
                entry => totalCampaignCount == 0 ? 0 : entry.Value.Count() * 100 / totalCampaignCount
            );
            model.T5HardAttendance = attendanceData.ToDictionary(
                entry => entry.Key,
                entry => T5HardCampaigns.Count() == 0 ? 0 : entry.Value.Count(e => T5HardCampaigns.ContainsKey(e)) * 100 / T5HardCampaigns.Count()
            );
            model.Last10T5HardAttendance = attendanceData.ToDictionary(
                entry => entry.Key,
                entry => last10T5HardCampaigns.Count() == 0 ? 0 : entry.Value.Count(e => last10T5HardCampaigns.ContainsKey(e)) * 100 / last10T5HardCampaigns.Count()
            );
            return model;
       }

        public bool PlayerCanEditCampaign(int? playerId, int campaignId)
        {
            if (!playerId.HasValue) return false;
            var playerExists = _dbContext.Player.Any(p => p.Id == playerId.Value && p.Active);
            if (!playerExists) return false;
            var campaignExists = _dbContext.Campaign.Any(c => c.ManagerId == playerId && c.Id == campaignId);
            return campaignExists;
        }

        public bool PlayerCanVolunteerForCampaign(int? playerId, int campaignId)
        {
            return PlayerCanVolunteerForCampaign(playerId, campaignId, out _, out _);
        }

        private bool PlayerCanVolunteerForCampaign(int? playerId, int campaignId, out Player player, out Campaign campaign)
        {
            campaign = null;
            player = null;
            if (!playerId.HasValue) return false;
            player = _dbContext.Player.SingleOrDefault(p => p.Id == playerId);
            if (player == null) return false;
            var syndicateId = player.SyndicateId;
            campaign = _dbContext.Campaign.SingleOrDefault(c => c.Id == campaignId && 
                    c.SyndicateId == syndicateId && 
                    c.ManagerId == null && 
                    (c.Status == Campaign.CampaignStatus.Planned || c.Status == Campaign.CampaignStatus.Unknown));
            return !(campaign == null);
        }

        public async Task<bool> VolunteerForCampaign(int? playerId, int campaignId)
        {
            Player player; Campaign campaign;
            var canVolunteer = PlayerCanVolunteerForCampaign(playerId, campaignId, out player, out campaign);
            if (!canVolunteer) return false;
            campaign.ManagerId = playerId;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public LeaderboardViewModel GetLeaderboard(int? playerId)
        {
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == playerId.Value);
            if (player == null) return new LeaderboardViewModel();
            var syndicateId = player.SyndicateId;
            var attendance = GetCampaignAttendance(null, syndicateId.Value);
            var allPlayers = _dbContext.Player.Where(p => p.SyndicateId == syndicateId).ToDictionary(p => p.Id, p => p);
            var allCampaignsPositions = attendance.TotalAttendance.AsEnumerable()
                .OrderByDescending(k => k.Value)
                .ThenBy(k => allPlayers[k.Key].Name)
                .Select(k => k.Key)
                .ToList();
            var allCampaignsPositionsDict =
                allCampaignsPositions.Where(p => p == playerId.Value || allCampaignsPositions.IndexOf(p) < 10)
                .ToDictionary(p => allCampaignsPositions.IndexOf(p) + 1, p => p);
            var allT5HardPositions = attendance.T5HardAttendance.AsEnumerable()
                .OrderByDescending(k => k.Value)
                .ThenByDescending(k => attendance.TotalAttendance[k.Key])
                .ThenBy(k => allPlayers[k.Key].Name)
                .Select(k => k.Key)
                .ToList();
            var allT5HardPositionsDict =
                allT5HardPositions.Where(p => p == playerId.Value || allT5HardPositions.IndexOf(p) < 10)
                .ToDictionary(p => allT5HardPositions.IndexOf(p) + 1, p => p);
            var last10T5HardPositions = attendance.Last10T5HardAttendance.AsEnumerable()
                .OrderByDescending(k => k.Value)
                .ThenByDescending(k => attendance.T5HardAttendance[k.Key])
                .ThenByDescending(k => attendance.TotalAttendance[k.Key])
                .ThenBy(k => allPlayers[k.Key].Name)
                .Select(k => k.Key)
                .ToList();
            var last10T5HardPositionsDict =
                last10T5HardPositions.Where(p => p == playerId.Value || last10T5HardPositions.IndexOf(p) < 10)
                .ToDictionary(p => last10T5HardPositions.IndexOf(p) + 1, p => p);
            var leaderboard = new LeaderboardViewModel{
                AllPlayers = allPlayers,
                Attendance = attendance,
                TotalLeaderboardPositions = allCampaignsPositionsDict,
                T5HardLeaderboardPositions = allT5HardPositionsDict,
                Last10T5HardLeaderboardPositions = last10T5HardPositionsDict,
                PlayerToCompare = player,
            };
            return leaderboard;
        }
    }
}