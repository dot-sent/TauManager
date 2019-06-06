using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HtmlAgilityPack;
using TauManager.Areas.Identity.Data;
using TauManager.Models;
using TauManager.Utils;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public class CampaignLogic : ICampaignLogic
    {
        private TauDbContext _dbContext { get; set; }
        public CampaignLogic(TauDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public CampaignOverviewViewModel GetCampaignOverview(int playerId, bool showLootApplyButton = false, bool showLootEditControls = false)
        {
            var players = _dbContext.Player.OrderBy(p => p.Name).AsEnumerable();
            var playerPositions = _dbContext.PlayerListPositionHistory.GroupBy(plph => plph.PlayerId)
                .Select(g => new {
                    PlayerId =  g.Key,
                    Position = g.Max(ph => ph.Id)
                })
                .OrderBy(p => p.Position)
                .Select(p => p.PlayerId)
                .ToList();
            var model = new CampaignOverviewViewModel{
                CurrentCampaigns = _dbContext.Campaign.Where(c => 
                    c.Status == Campaign.CampaignStatus.InProgress ||
                    c.Status == Campaign.CampaignStatus.Abandoned)
                    .OrderByDescending(c => c.UTCDateTime).AsEnumerable(),
                PastCampaigns = _dbContext.Campaign.Where(c => 
                    c.Status == Campaign.CampaignStatus.Completed ||
                    c.Status == Campaign.CampaignStatus.Failed ||
                    c.Status == Campaign.CampaignStatus.Skipped)
                    .OrderByDescending(c => c.UTCDateTime).AsEnumerable(),
                FutureCampaigns = _dbContext.Campaign.Where(c => 
                    c.Status == Campaign.CampaignStatus.Assigned ||
                    c.Status == Campaign.CampaignStatus.Planned ||
                    c.Status == Campaign.CampaignStatus.Unknown)
                    .OrderByDescending(c => c.UTCDateTime).AsEnumerable(),
                LootToDistribute = _dbContext.CampaignLoot.Where(cl =>
                    cl.Status == CampaignLoot.CampaignLootStatus.Undistributed)
                    .Select(l => new LootItemViewModel
                    {
                        Loot = l,
                        ShowApplyButton = showLootApplyButton,
                        ShowEditControls = showLootEditControls,
                        LootStatuses = EnumExtensions.ToDictionary<int>(typeof(CampaignLoot.CampaignLootStatus)),
                        Players = players,
                        AllRequests = l.Requests.Select(r => new
                            {
                                Player = r.RequestedFor,
                                Position = playerPositions.IndexOf(r.RequestedForId) + 1,
                            })
                            .OrderBy(pp => pp.Position)
                            .ToDictionary(
                                pp => pp.Position,
                                pp => pp.Player.Name
                            ),
                    }),
                MySignups = _dbContext.CampaignSignup
                    .Where(cs => cs.PlayerId == playerId)
                    .ToDictionary(cs => cs.CampaignId, cs => 1),
                MyAttendance = _dbContext.CampaignAttendance
                    .Where(a => a.PlayerId == playerId)
                    .ToDictionary( a => a.CampaignId, a => 1),
                MyPosition = playerPositions.IndexOf(playerId) + 1,
            };
            return model;
        }

        public CampaignDetailsViewModel GetCampaignById(int id, bool showLootApplyButton = false, bool showLootEditControls = false)
        {
            var campaign =  _dbContext.Campaign.SingleOrDefault(c => c.Id == id);
            return campaign == null ? null : GetExtendedCampaign(campaign, showLootApplyButton, showLootEditControls);
        }

        public CampaignDetailsViewModel GetExtendedCampaign(Campaign campaign, bool showLootApplyButton = false, bool showLootEditControls = false)
        {
            var players = _dbContext.Player.OrderBy(p => p.Name).AsEnumerable();
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

        public async Task<Campaign> CreateOrEditCampaign(Campaign campaign)
        {
            if (campaign.ManagerId.HasValue && campaign.ManagerId.Value == 0)
            {
                campaign.ManagerId = null;
            }
            if (campaign.Id == 0)
            {
                _dbContext.Add(campaign);
            } else {
                _dbContext.Update(campaign);
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
            var itemData = await TauHead.GetItemData(url);
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
            var players = _dbContext.Player.OrderBy(p => p.Name).AsEnumerable();
            var model = new LootItemViewModel{
                Loot = campaignLootItem,
                ShowApplyButton = showApplyButton,
                ShowEditControls = showLootEditControls,
                LootStatuses = EnumExtensions.ToDictionary<int>(typeof(CampaignLoot.CampaignLootStatus)),
                Players = players,
            };
            return model;
        }

        public CampaignDetailsViewModel GetNewCampaign()
        {
            var campaign = new Campaign{
                Difficulty = Campaign.CampaignDifficulty.Easy,
                UTCDateTime = DateTime.Now,
                Tiers = 0,
                ManagerId = null,
                Loot = new List<CampaignLoot>(),
            };
            return GetExtendedCampaign(campaign);
        }

        public async Task<CampaignPageParseResultViewModel> ParseCampaignPage(string fileContents, int campaignId)
        {
            var campaign = _dbContext.Campaign.SingleOrDefault(c => c.Id == campaignId);
            if (campaign == null) return null;

            var model = new CampaignPageParseResultViewModel();
            var document = new HtmlDocument();
            document.LoadHtml(fileContents);
            var epicImages = document.DocumentNode.SelectNodes("//div[contains(concat(\" \",normalize-space(@class),\" \"),\" epic \")]/img");
            var leaderboardLines = document.DocumentNode.SelectNodes("//div[@id=\"campaign-result-leaderboard\"]/table/tbody/tr");
            foreach (var epicNode in epicImages)
            {
                var linkifyLinks = epicNode.ParentNode.ParentNode.NextSibling.NextSibling.Descendants("a");
                if (linkifyLinks.Count() > 0)
                {
                    var url = linkifyLinks.First().Attributes["href"].Value;
                    var urlParts = url.Split('/');
                    var slug = urlParts.Last();
                    var epic = _dbContext.Item.SingleOrDefault(i => i.Slug == slug);
                    if (epic == null) 
                    {
                        var tauHeadUrl = TauHead.UrlBase + "/" + urlParts.TakeLast(2).Aggregate((result, item) => result + "/" + item);
                        epic = await TauHead.GetItemData(tauHeadUrl);
                        await _dbContext.AddAsync(epic);
                    }
                    if (epic != null)
                    {
                        if (epic.Type == Item.ItemType.Armor || epic.Type == Item.ItemType.Weapon)
                        {
                            var existingLoot = _dbContext.CampaignLoot.Any(cl => cl.ItemId == epic.Id && cl.CampaignId == campaignId);
                            if (!existingLoot)
                            {
                                var loot = new CampaignLoot{
                                    CampaignId = campaignId,
                                    Status = CampaignLoot.CampaignLootStatus.Undistributed,
                                    ItemId = epic.Id,
                                };
                                await _dbContext.AddAsync(loot);
                                // TODO: log successfully added loot
                            }
                        } else {
                            // TODO: log non-armor, non-weapon epic
                        }
                    } else {
                        // TODO: log incorrect slug
                    }
                } else {
                    // TODO: log item without linkify URL
                }
            }
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
                    // TODO: Handle non-existing player
                }
            }

            await _dbContext.SaveChangesAsync();
            return model;
        }

        public async Task<bool> SetSignupStatus(int playerId, int campaignId, bool status)
        {
            var campaign = _dbContext.Campaign.SingleOrDefault(c => c.Id == campaignId);
            if (campaign == null) return false;
            var signup = _dbContext.CampaignSignup.SingleOrDefault(s => s.PlayerId == playerId && s.CampaignId == campaignId);
            if (signup == null)
            {
                if (!status) return false;
                signup = new CampaignSignup{
                    PlayerId = playerId,
                    CampaignId = campaignId,
                };
                await _dbContext.AddAsync(signup);
            } else {
                if (status) return false;
                _dbContext.Remove(signup);
            }
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ParseAttendanceCSV(string fileContents)
        {
            var lines = fileContents.Split("\n");
            var headerLine = lines[0].Split(',');
            foreach (var playerLine in lines.Skip(1))
            {
                var cells = playerLine.Split(',');
                var playerName = cells[0];
                var player = _dbContext.Player.SingleOrDefault(p => p.Name == playerName);
                if (player == null) continue;
                for (int i = 1; i < headerLine.Count(); i++)
                {
                    if (cells[i] == "1")
                    {
                        var campaignId = headerLine[i];
                        var campaign = _dbContext.Campaign.SingleOrDefault(c => c.Id.ToString() == campaignId);
                        if (campaign == null) continue;
                        var attendance = _dbContext.CampaignAttendance.SingleOrDefault(a => a.CampaignId == campaign.Id && a.PlayerId == player.Id);
                        if (attendance == null) {
                            attendance = new CampaignAttendance{
                                PlayerId = player.Id,
                                CampaignId = campaign.Id
                            };
                            await _dbContext.AddAsync(attendance);
                        }
                    }
                }
            }
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public AttendanceViewModel GetCampaignAttendance(int? playerId)
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
                .Where(p => !playerId.HasValue || p.Id == playerId.Value)
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
            var totalCampaignCount = _dbContext.Campaign.Count();
            var last10T5HardCampaigns = _dbContext.Campaign
                .OrderByDescending(c => c.UTCDateTime)
                .Where(c => (c.Difficulty == Campaign.CampaignDifficulty.Hard ||
                    c.Difficulty == Campaign.CampaignDifficulty.Extreme) &&
                    c.Tiers.HasValue && c.Tiers.Value > 15 &&
                    (c.Status == Campaign.CampaignStatus.Completed || c.Status == Campaign.CampaignStatus.Failed))
                .Select(c => c.Id)
                .Take(10)
                .ToDictionary(
                    c => c,
                    c => 1
                );
            model.TotalAttendance = attendanceData.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Count() * 100 / totalCampaignCount
            );
            model.Last10T5HardAttendance = attendanceData.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Count(e => last10T5HardCampaigns.ContainsKey(e)) * 100 / last10T5HardCampaigns.Count() // Last number should be 10, but you never know :-D
            );
            return model;
       }
    }
}