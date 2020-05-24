using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MathNet.Numerics.Statistics;
using TauManager.ViewModels;
using TauManager.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TauManager.Utils;
using static TauManager.Models.Notification;

namespace TauManager.BusinessLogic
{
    public class PlayerLogic : IPlayerLogic
    {
        private TauDbContext _dbContext { get; set; }
        private ICampaignLogic _campaignLogic { get; set; }
        private INotificationLogic _notificationLogic { get; set; }
        public PlayerLogic(TauDbContext dbContext, ICampaignLogic campaignLogic, INotificationLogic notificationLogic)
        {
            _dbContext = dbContext;
            _campaignLogic = campaignLogic;
            _notificationLogic = notificationLogic;
        }

        public SyndicatePlayersViewModel GetSyndicateMetrics(int? playerId, bool includeInactive, int syndicateId)
        {
            var model = new SyndicatePlayersViewModel{
                IncludeInactive = includeInactive,
            };
            var allPlayers = _dbContext.Player.Where(p => (includeInactive || p.Active) && p.SyndicateId == syndicateId).AsEnumerable();
            model.PlayerStats = allPlayers.Where(p => p.Active).ToList().GroupBy(p => p.Tier).Select(g => new SyndicatePlayersViewModel.TierStatistics
            {
                Tier = g.Key,
                PlayerCount = g.Count(),
                Strength = g.Average(p => p.Strength),
                Stamina = g.Average(p => p.Stamina),
                Agility = g.Average(p => p.Agility),
                StatTotalMedian = Statistics.Median(g.Select(p => (double)p.StatTotal)),
                StatTotalStdDev = Statistics.StandardDeviation(g.Select(p => (double)p.StatTotal)),
            }).ToDictionary(p => p.Tier, p => p);
            model.MaxTier = model.PlayerStats.Count > 0 ? model.PlayerStats.Keys.Max() : 0;

            model.PlayerCountByStatTotal = new Dictionary<KeyValuePair<int, int>, int>();
            int[] statBoundaries = {0, 100, 250, 500, 750, 1000, 1250, 1500, 10000};
            for (var i = 0; i < statBoundaries.Count()-1; i++)
            {
                var min = statBoundaries[i];
                var max = statBoundaries[i+1];
                // [StatTotal]
                model.PlayerCountByStatTotal[new KeyValuePair<int, int>(min, max)] = 
                    allPlayers.Count(p => p.Strength + p.Stamina + p.Agility >= min && 
                        p.Strength + p.Stamina + p.Agility < max &&
                        p.Active);
            }
            if (!playerId.HasValue)
            {
                model.Players = allPlayers
                    .GroupBy(p => p.Tier)
                    .Select(g => new {
                        Tier = g.Key,
                        Players = g
                    })
                    .OrderByDescending(t => t.Tier)
                    .ToDictionary(g => g.Tier, g => g.Players.OrderBy(p => p.Name).ToList());
                var histories = _dbContext.PlayerHistory
                    .Join(_dbContext.Player, ph => ph.PlayerId, p => p.Id, (ph, p) => new { PlayerHistory = ph, Player = p})
                    .Where(php => php.Player.SyndicateId == syndicateId)
                    .Select(php => php.PlayerHistory)
                    .AsEnumerable()
                    .GroupBy(ph => ph.PlayerId)
                    .Select( g =>
                        new 
                        {
                            playerId = g.Key,
                            last2w = g.Where(ph => DateTime.Now.Subtract(ph.RecordedAt).TotalDays < 14),
                            olderExist = g.Any(ph => DateTime.Now.Subtract(ph.RecordedAt).TotalDays >= 14)
                        }
                    );
                
                model.LastActivity = new Dictionary<int, SyndicatePlayersViewModel.LastPlayerActivity>();
                foreach (var history in histories)
                {
                    var player = allPlayers.SingleOrDefault(p => p.Id == history.playerId);
                    if (player == null) continue;
                    int daysAgo = -1;
                    if (history.last2w.Count() == 0)
                    {
                        daysAgo = 15;
                    } else {
                        var prevHistory = history.last2w.OrderByDescending(h => h.RecordedAt).FirstOrDefault();
                        foreach(var historyEntry in history.last2w.OrderByDescending(h => h.RecordedAt))
                        {
                            if (historyEntry.IsDifferent(player))
                            {
                                daysAgo = (DateTime.Now - prevHistory.RecordedAt).Days;
                                break;
                            } else {
                                prevHistory = historyEntry;
                            }
                        }
                        if (daysAgo == -1 && history.olderExist)
                        {
                            daysAgo = 15;
                        }
                    }
                    model.LastActivity[history.playerId] = new SyndicatePlayersViewModel.LastPlayerActivity
                    {
                        DaysAgo = daysAgo,
                        Active = player.Active,
                    };
                }
                foreach(var player in allPlayers)
                {
                    if (!model.LastActivity.ContainsKey(player.Id))
                    {
                        model.LastActivity[player.Id] = new SyndicatePlayersViewModel.LastPlayerActivity
                        {
                            DaysAgo = 15,
                            Active = player.Active,
                        };
                    }
                }
                model.Attendance = _campaignLogic.GetCampaignAttendance(null, syndicateId);
            } else {
                model.PlayerToCompare = _dbContext.Player.SingleOrDefault(p => p.Id == playerId.Value);
            }
            return model;
        }

        public async Task<string> ParsePlayerPageAsync(string fileContents, int syndicateId)
        {
            string message;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(fileContents);
            var characterName = document.DocumentNode.SelectSingleNode("//*[contains(concat(\" \",normalize-space(@class),\" \"),\" character-name \")]").InnerHtml.Trim();
            var recordedAt = DateTime.Parse(document.DocumentNode.SelectSingleNode("//html").Attributes["data-time"].Value);
            var levelText = document.DocumentNode.SelectSingleNode("//*[contains(concat(\" \",normalize-space(@class),\" \"),\" statistics \")]/dd[position()=3]").InnerHtml.Trim();
            var levelParts = levelText.Split(" @ ");
            decimal level = int.Parse(levelParts[0]) + (decimal.Parse(levelParts[1].Replace("%", "")) / 100);
            var lastCourseDate = Utils.GCT.ParseGCTDate(document.DocumentNode.SelectSingleNode("//*[contains(concat(\" \",normalize-space(@class),\" \"),\" not-enrolled \")]").InnerHtml.Trim());
            var nodes = document.DocumentNode.SelectNodes("//div[contains(concat(\" \",normalize-space(@class),\" \"),\" accordion-panel \")]");
            var nodeValues = new Dictionary<String, Dictionary<String, String>>();
            foreach (var node in nodes)
            {
                var tableId = node.Attributes["Id"].Value;
                nodeValues[tableId] = new Dictionary<string, string>();
                var tbodyElements = node.Descendants("tbody");
                if (tbodyElements.Count() == 0)
                {
                    tbodyElements = node.Descendants("table");
                }
                if (tbodyElements.Count() > 0)
                {
                    var tbody = tbodyElements.First();
                    foreach (var tr in tbody.Descendants("tr"))
                    {
                        var tds = tr.Descendants("td").Take(2);
                        if (tds.Count() == 2)
                        {
                            nodeValues[tableId][tds.First().InnerHtml] = tds.Last().InnerHtml;
                        }
                        else
                        { // We are dealing with the nested structure inside a single <td>
                            if (tableId == "character_bank" && tds.First().Descendants("div").Count() > 0)
                            {
                                nodeValues[tableId][tds.First().Descendants("div").First().Attributes["class"].Value] =
                                    tds.First().Descendants("span").First().Descendants("span").First().InnerHtml;
                            }
                        }
                    }
                }
            }
            if (!lastCourseDate.HasValue) 
            {
                lastCourseDate = nodeValues["character_education"].Keys.Count > 0 ?
                    Utils.GCT.ParseGCTDate(nodeValues["character_education"].Values.OrderByDescending(v => v).First()) :
                    null;
            }
            var visaText = nodeValues["character_visas"].ContainsKey("Gaule") ? nodeValues["character_visas"]["Gaule"] :
                string.Empty;
            DateTime? visaDate = string.IsNullOrEmpty(visaText) ? null : Utils.GCT.ParseGCTDateExact(visaText);
            var player = _dbContext.Player.FirstOrDefault(p => p.Name.Equals(characterName));
            if (player == null) // We are dealing with completely new player
            {
                player = new Player
                {
                    Strength = Decimal.Parse(nodeValues["character_stats"]["Strength"]),
                    Agility = Decimal.Parse(nodeValues["character_stats"]["Agility"]),
                    Stamina = Decimal.Parse(nodeValues["character_stats"]["Stamina"]),
                    Intelligence = Decimal.Parse(nodeValues["character_stats"]["Intelligence"]),
                    Social = Decimal.Parse(nodeValues["character_stats"]["Social"]),
                    Level = level,
                    Wallet = 0,
                    Bank = Decimal.Parse(nodeValues["character_bank"]["credits"].Replace(",", "")),
                    Bonds = int.Parse(nodeValues["character_bank"]["bonds"].Replace(",", "")),
                    LastUpdate = recordedAt,
                    UniCourseDate = lastCourseDate,
                    GauleVisaExpiry = visaDate,
                    Name = characterName,
                    Active = true, // By default all players start active
                    SyndicateId = syndicateId, // By default all new players are assigned to the same syndicate as the uploading officer
                };
                foreach (var skillName in nodeValues["character_skills"].Keys)
                {
                    var skill = _dbContext.Skill.SingleOrDefault(s => s.Name == skillName);
                    if (skill == null)
                    {
                        skill = new Skill{
                            Name = skillName,
                        };
                        _dbContext.Add(skill);
                    }
                    var playerSkill = new PlayerSkill{
                        Skill = skill,
                        Player = player,
                        SkillLevel = int.Parse(nodeValues["character_skills"][skillName]),
                    };
                    _dbContext.Add(playerSkill);
                }
                var playerHistory = new PlayerHistory(player);
                _dbContext.Add(player);
                _dbContext.Add(playerHistory);

                // Other things that need to happen when a new player is added
                var ldlPosition = new PlayerListPositionHistory{
                    Player = player,
                    Comment = "Initial seeding",
                    CreatedAt = DateTime.Now,
                };
                _dbContext.Add(ldlPosition);

                message = "Player, history entry and loot distribution position added, player activated.";
            }
            else
            {
                if (_dbContext.PlayerHistory
                    .Where(ph => ph.PlayerId == player.Id)
                    .AsEnumerable()
                    // TODO: Bad performance almost 100%; try converting to SQL-suitable
                    .Any(ph => Math.Abs(ph.RecordedAt.Subtract(recordedAt).TotalMinutes) < 1))
                {
                    message = "File has been already processed before";
                }
                else
                {
                    var playerHistory = new PlayerHistory
                    {
                        Strength = Decimal.Parse(nodeValues["character_stats"]["Strength"]),
                        Agility = Decimal.Parse(nodeValues["character_stats"]["Agility"]),
                        Stamina = Decimal.Parse(nodeValues["character_stats"]["Stamina"]),
                        Intelligence = Decimal.Parse(nodeValues["character_stats"]["Intelligence"]),
                        Social = Decimal.Parse(nodeValues["character_stats"]["Social"]),
                        Level = level,
                        Wallet = 0,
                        Bank = Decimal.Parse(nodeValues["character_bank"]["credits"].Replace(",", "")),
                        Bonds = int.Parse(nodeValues["character_bank"]["bonds"].Replace(",", "")),
                        RecordedAt = recordedAt,
                        Player = player,
                        PlayerId = player.Id,
                    };
                    _dbContext.Add(playerHistory);
                    message = "History entry added";
                    if (recordedAt > player.LastUpdate)
                    {
                        message += " and player entry updated";
                        player.Update(playerHistory);
                        if (player.NotificationSettings.HasFlag(Player.NotificationFlags.GauleVisa) &&
                            player.GauleVisaExpiry != visaDate &&
                            visaDate.HasValue)
                        {
                            _dbContext.Notification.Add(
                                new Notification{
                                    Kind = NotificationKind.GauleVisa,
                                    RecipientId = player.Id,
                                    SendAfter = visaDate.Value.AddDays(-2),
                                    Status = NotificationStatus.NotSent
                                }
                            );
                        }
                        if (player.NotificationSettings.HasFlag(Player.NotificationFlags.University) &&
                            player.UniCourseDate != lastCourseDate &&
                            lastCourseDate.HasValue &&
                            !player.UniversityComplete)
                        {
                            _dbContext.Notification.Add(
                                new Notification{
                                    Kind = NotificationKind.University,
                                    RecipientId = player.Id,
                                    SendAfter = lastCourseDate.Value.AddDays(-1),
                                    Status = NotificationStatus.NotSent
                                }
                            );
                        }
                        player.UniCourseDate = lastCourseDate;
                        player.GauleVisaExpiry = visaDate;
                        _dbContext.Update(player);
                    }
                    foreach (var skillName in nodeValues["character_skills"].Keys)
                    {
                        var skill = _dbContext.Skill.SingleOrDefault(s => s.Name == skillName);
                        if (skill == null)
                        {
                            skill = new Skill{
                                Name = skillName,
                            };
                            _dbContext.Add(skill);
                        }
                        var playerSkill = _dbContext.PlayerSkill.SingleOrDefault(ps => ps.PlayerId == player.Id && ps.Skill == skill);
                        if (playerSkill == null)
                        {
                            playerSkill = new PlayerSkill{
                                Skill = skill,
                                Player = player,
                            };
                            _dbContext.Add(playerSkill);
                        }
                        playerSkill.SkillLevel = int.Parse(nodeValues["character_skills"][skillName]);
                    }
                    message += ".";
                }
            }
            await _dbContext.SaveChangesAsync();
            return message;
        }

        public async Task<bool> SetPlayerActiveAsync(int playerId, bool status)
        {
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == playerId);
            if (player == null) return false;
            player.Active = status;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public PlayerDetailsViewModel GetPlayerDetails(int id, bool? loadAll)
        {
            var player = _dbContext.Player.Include(p => p.HeldCampaignLoot).SingleOrDefault(p => p.Id == id);
            if (player == null) return null;
            var playerHistory = player.History
                .Where(ph => (loadAll.HasValue && loadAll.Value) || ((DateTime.Now - ph.RecordedAt).TotalDays < 14) )
                .GroupBy(ph => ph.RecordedAt.Date)
                .Select(g => g.OrderByDescending(ph => ph.RecordedAt).First())
                .OrderByDescending(ph => ph.RecordedAt);
            var moreRows = loadAll.HasValue && loadAll.Value ? 0 : player.History.Count(ph => (DateTime.Now - ph.RecordedAt).TotalDays >= 14);
            long lastActivityId = -1;
            if (playerHistory.LastOrDefault() != null)
            {
                if (!playerHistory.LastOrDefault().IsDifferent(player))
                {
                    if (moreRows > 0)
                    {
                        lastActivityId = 0;
                    }
                } else {
                    var changeTS = playerHistory.First(ph => ph.IsDifferent(player)).RecordedAt;
                    var lastActivity = playerHistory.LastOrDefault(ph => ph.RecordedAt > changeTS);
                    if (lastActivity != null)
                    {
                        lastActivityId = lastActivity.Id;
                    }
                }
            } else {
                if (moreRows > 0)
                {
                    lastActivityId = 0;
                } else {
                    throw new InvalidOperationException(String.Format("Player id {0} has an entry, but no history, please investigate!", player.Id));
                }
            }
            return new PlayerDetailsViewModel
            {
                Player = player,
                History = playerHistory.AsEnumerable(),
                MoreRows = moreRows,
                LastActivityId = lastActivityId,
                Attendance = _campaignLogic.GetCampaignAttendance(id, player.SyndicateId.Value),
            };
        }

        public SkillOverviewViewModel GetSkillsOverview(string skillGroupName, int syndicateId)
        {
            var allPlayers = _dbContext.Player.Where(p => p.Active && p.SyndicateId == syndicateId).OrderBy(p => p.Name).AsEnumerable();
            var allSkillGroups = _dbContext.SkillGroup
                .AsEnumerable()
                .GroupBy(sg => sg.Name)
                .Select(g => g.FirstOrDefault().Name)
                .AsEnumerable();
            var skills = (String.IsNullOrEmpty(skillGroupName) ?
                _dbContext.Skill.AsEnumerable() :
                _dbContext.SkillGroup.Where(sg => sg.Name == skillGroupName)
                    .Select(sg => sg.Skill).AsEnumerable())
                    .OrderBy(s => s.Id);
            var playerSkills = skills.SelectMany(s => s.PlayerRelations)
                .Join(_dbContext.Player, ps => ps.PlayerId, p => p.Id, (ps, p) => new { PlayerSkill = ps, Player = p})
                .Where(php => php.Player.SyndicateId == syndicateId)
                .Select(php => php.PlayerSkill)
                .GroupBy(pr => pr.PlayerId)
                .Select(g => new {
                    PlayerId = g.Key,
                    Skills = g.ToDictionary(
                        ps => ps.SkillId,
                        ps => ps.SkillLevel
                    )
                })
                .ToDictionary(
                    ps => ps.PlayerId,
                    ps => ps.Skills
                );
            allPlayers = allPlayers.OrderByDescending(
                p => playerSkills.ContainsKey(p.Id) ?
                    playerSkills[p.Id].Values.Sum() : 0
            );
            var result = new SkillOverviewViewModel{
                AllSkillGroups = allSkillGroups,
                Players = allPlayers,
                SkillGroupName = skillGroupName,
                SkillValues = playerSkills,
                Skills = skills,
            };
            return result;
        }

        public HomePageViewModel GetHomePageModel(int? playerId)
        {
            if (!playerId.HasValue) return new HomePageViewModel();
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == playerId.Value);
            if (player == null) return new HomePageViewModel();
            var syndicateId = player.SyndicateId;

            var announcements = new List<Announcement>();
            if (playerId.HasValue)
            {
                if (!player.UniCourseActive && player.PlayerSkills.Sum(ps => ps.SkillLevel) < Constants.MaxUniversityCourses)
                {
                    announcements.Add(new Announcement{
                        FromId = null,
                        Text = "You are currently not enrolled in a University course!",
                        Style = Announcement.AnnouncementStyle.Danger,
                        ToId = playerId,
                        To = player,
                    });
                } else if (((DateTime.Today - player.UniCourseDate.Value).Days == 0 ||
                    (DateTime.Today - player.UniCourseDate.Value).Days == -1) && 
                    player.PlayerSkills.Sum(ps => ps.SkillLevel) < 114)
                {
                    announcements.Add(new Announcement{
                        FromId = null,
                        Text = "Your current university course ends today or tomorrow, make sure to pick the next one up on time.",
                        Style = Announcement.AnnouncementStyle.Warning,
                        ToId = playerId,
                        To = player,
                    });
                }
                if (player.GauleVisaExpired)
                {
                    announcements.Add(new Announcement{
                        FromId = null,
                        Text = "Your Gaule visa has expired!",
                        Style = Announcement.AnnouncementStyle.Danger,
                        ToId = playerId,
                        To = player,
                    });
                } else if(player.GauleVisaExpiring) {
                    announcements.Add(new Announcement{
                        FromId = null,
                        Text = "Your Gaule visa expires " + player.GauleVisaExpiryString + ", make sure to get a new one on time!",
                        Style = Announcement.AnnouncementStyle.Warning,
                        ToId = playerId,
                        To = player,
                    });
                }

            }
            var model = new HomePageViewModel{
                Metrics = GetSyndicateMetrics(playerId, false, syndicateId ?? 0),
                Announcements = announcements,
            };
            return model;
        }

        public string GetPlayerPageUploadToken()
        {
            // The length of this string is 32 chars on purpose - 
            // it MUST be one of the integer divisors for 256.
            string randomChars = "ABCDEFGHJKLMNPQRSTUVWXYZ01234567";
            RandomNumberGenerator r = RandomNumberGenerator.Create();
            byte[] randomBytes = new Byte[256];
            r.GetBytes(randomBytes);

            var resultString = "";

            List<char> chars = new List<char>();
            foreach (var b in randomBytes)
            {
                chars.Add(randomChars[b % randomChars.Length]);
            }
            resultString = new string(chars.ToArray());

            return resultString;
        }

        public PlayerDetailsChartData GetPlayerDetailsChartData(int id, byte interval, byte dataKind)
        {
            var result = new PlayerDetailsChartData();
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == id);
            if (player == null) return result;
            var startDate = DateTime.Today;
            switch (interval)
            {
                case (byte)PlayerDetailsChartData.Interval.Week:
                    startDate = startDate.AddDays(-7); break;
                case (byte)PlayerDetailsChartData.Interval.Month1:
                    startDate = startDate.AddMonths(-1); break;
                case (byte)PlayerDetailsChartData.Interval.Month3:
                    startDate = startDate.AddMonths(-3); break;
                case (byte)PlayerDetailsChartData.Interval.Month6:
                    startDate = startDate.AddMonths(-6); break;
                case (byte)PlayerDetailsChartData.Interval.Year:
                    startDate = startDate.AddYears(-1); break;
                case (byte)PlayerDetailsChartData.Interval.Max:
                    startDate = new DateTime(2018, 10, 28); break; // The Manager has received first data in October 2018
                default: // This means invalid input data
                    return result;
            }
            var relevantData = _dbContext.PlayerHistory.Where(ph => ph.PlayerId == id && ph.RecordedAt >= startDate)
                .OrderBy(ph => ph.RecordedAt)
                .AsEnumerable() // Flesh out the data before client-side grouping
                .GroupBy(ph => ph.RecordedAt.Date)
                .Select(g =>
                    new {
                        RecordedAt = g.Key,
                        HistoryEntry = g.OrderByDescending(ph => ph.RecordedAt).First()
                    }
                );
            switch (dataKind)
            {
                case (byte)PlayerDetailsChartData.DataKind.StatsTotal:
                    result.AddRange(
                        relevantData.Select(he => new PlayerDetailsChartDataPoint { t = he.RecordedAt, y = (double)he.HistoryEntry.StatTotal })
                    );
                    break;
                case (byte)PlayerDetailsChartData.DataKind.Credits:
                    result.AddRange(
                        relevantData.Select(he => new PlayerDetailsChartDataPoint { t = he.RecordedAt, y = (double)he.HistoryEntry.Bank })
                    );
                    break;
                case (byte)PlayerDetailsChartData.DataKind.Bonds:
                    result.AddRange(
                        relevantData.Select(he => new PlayerDetailsChartDataPoint { t = he.RecordedAt, y = (double)he.HistoryEntry.Bonds })
                    );
                    break;
                case (byte)PlayerDetailsChartData.DataKind.XP:
                    result.AddRange(
                        relevantData.Select(he => new PlayerDetailsChartDataPoint { t = he.RecordedAt, y = (double)he.HistoryEntry.Level })
                    );
                    break;
                default: // This means invalid input data
                    return result;
            }

            return result;
        }

        public async Task<bool> SetPlayerDiscordAccountAsync(int? playerId, string login)
        {
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == playerId);
            if (player == null) return false;
            player.DiscordLogin = login;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private static bool FlagCombo(Player.NotificationFlags values, bool all, bool ifSignedup)
        {
            return values.HasFlag(Player.NotificationFlags.CampaignSoonAll) == all &&
                values.HasFlag(Player.NotificationFlags.CampaignSoonIfSignedUp) == ifSignedup;
        }

        public bool SetPlayerNotificationByDiscord(string discordLogin, int notificationFlags)
        {
            if (string.IsNullOrWhiteSpace(discordLogin)) return false;
            if (!Player.IsValidNotificationFlag(notificationFlags)) return false;
            var players = _dbContext.Player.Where(p => p.DiscordLogin == discordLogin);
            if (players.Count() != 1) return false; // This limits Discord account connection to max. 1 player, which is what I want.
            var player = players.First();
            var oldValues = player.NotificationSettings;
            player.NotificationSettings = (Player.NotificationFlags)notificationFlags;
            var flagValues = EnumExtensions.ToDictionary<Player.NotificationFlags>(typeof(Player.NotificationFlags));
            foreach(var flagKey in flagValues.Keys)
            {
                // valueDiff will be 0 for no changes, >0 if turning flag on, <0 if turning flag off
                var valueDiff = (player.NotificationSettings & flagKey) - (oldValues & flagKey);
                if (valueDiff != 0)
                {
                    switch(flagKey) {
                        case Player.NotificationFlags.GauleVisa:
                            if (valueDiff > 0)
                            {
                                _dbContext.Notification.Add(
                                    new Notification{
                                        Kind = NotificationKind.GauleVisa,
                                        RecipientId = player.Id,
                                        SendAfter = player.GauleVisaExpiry.Value.AddDays(-2),
                                        Status = NotificationStatus.NotSent
                                    }
                                );
                            } else {
                                var notifications =_dbContext.Notification
                                    .Where(n => n.RecipientId == player.Id &&
                                        n.Kind == NotificationKind.GauleVisa);
                                _dbContext.RemoveRange(notifications);
                            }
                            break;
                        case Player.NotificationFlags.University:
                            if (valueDiff > 0)
                            {
                                if (!player.UniversityComplete)
                                {
                                    _dbContext.Notification.Add(
                                        new Notification{
                                            Kind = NotificationKind.University,
                                            RecipientId = player.Id,
                                            SendAfter = player.UniCourseDate.Value.AddDays(-1),
                                            Status = NotificationStatus.NotSent
                                        }
                                    );
                                }
                            } else {
                                var notifications =_dbContext.Notification
                                    .Where(n => n.RecipientId == player.Id &&
                                        n.Kind == NotificationKind.University);
                                _dbContext.RemoveRange(notifications);
                            }
                            break;
                        case Player.NotificationFlags.CampaignSoonAll:
                            // Too complex logic to implement it just like this, see below
                            break;
                        default:
                            break;
                    }
                }
            }

            if(FlagCombo(oldValues, false, false) &&
                (player.NotificationSettings &
                    (Player.NotificationFlags.CampaignSoonAll | Player.NotificationFlags.CampaignSoonIfSignedUp)) != 0)
            {
                var campaignsSoon =
                    player.NotificationSettings.HasFlag(Player.NotificationFlags.CampaignSoonAll)
                    ?
                    _dbContext.Campaign
                    .Where(c => c.UTCDateTime.Value > DateTime.UtcNow && c.SyndicateId == player.SyndicateId)
                    :
                    _dbContext.CampaignSignup
                    .Include(c => c.Campaign)
                    .Where(c => c.PlayerId == player.Id && c.Campaign.UTCDateTime.Value > DateTime.UtcNow)
                    .Select(c => c.Campaign);
                foreach (var campaign in campaignsSoon)
                {
                    var sendAfter = campaign.UTCDateTime.Value.EnsureUTC().AddHours(-4);
                    sendAfter = TimeZoneInfo.ConvertTimeFromUtc(sendAfter, TimeZoneInfo.Local);
                    _dbContext.Notification.Add(
                        new Notification{
                            Kind = NotificationKind.CampaignSoon,
                            RecipientId = player.Id,
                            RelatedId = campaign.Id,
                            SendAfter = sendAfter,
                            Status = NotificationStatus.NotSent
                        }
                    );
                }
            }

            if (FlagCombo(oldValues, false, true) &&
                (player.NotificationSettings &
                    Player.NotificationFlags.CampaignSoonAll) != 0)
            {
                var campaignsSoon = _dbContext.Campaign
                    .Where(c => c.UTCDateTime.Value > DateTime.UtcNow && c.SyndicateId == player.SyndicateId
                    && !_dbContext.CampaignSignup.Any(cs => cs.CampaignId == c.Id && cs.PlayerId == player.Id));
                foreach (var campaign in campaignsSoon)
                {
                    var sendAfter = campaign.UTCDateTime.Value.EnsureUTC().AddHours(-4);
                    sendAfter = TimeZoneInfo.ConvertTimeFromUtc(sendAfter, TimeZoneInfo.Local);
                    _dbContext.Notification.Add(
                        new Notification{
                            Kind = NotificationKind.CampaignSoon,
                            RecipientId = player.Id,
                            RelatedId = campaign.Id,
                            SendAfter = sendAfter,
                            Status = NotificationStatus.NotSent
                        }
                    );
                }
            }

            if (FlagCombo(player.NotificationSettings, false, true) &&
                (oldValues & Player.NotificationFlags.CampaignSoonAll) != 0)
            {
                var notificationsToRemove = _dbContext.Campaign
                    .Join(_dbContext.Notification,
                        c => c.Id,
                        n => n.RelatedId,
                        (c, n) => new{Campaign = c, Notification = n})
                    .Where(c => c.Campaign.UTCDateTime.Value > DateTime.UtcNow && c.Campaign.SyndicateId == player.SyndicateId
                    && !_dbContext.CampaignSignup.Any(cs => cs.CampaignId == c.Campaign.Id && cs.PlayerId == player.Id)
                    && c.Notification.Kind == NotificationKind.CampaignSoon
                    && c.Notification.RecipientId == player.Id
                    && c.Notification.Status == NotificationStatus.NotSent)
                    .Select(c => c.Notification);
                _dbContext.RemoveRange(notificationsToRemove);
            }

            if (FlagCombo(player.NotificationSettings, false, false) &&
                (oldValues &
                    (Player.NotificationFlags.CampaignSoonAll | Player.NotificationFlags.CampaignSoonIfSignedUp)) != 0)
            {
                var notificationsToRemove = _dbContext.Notification
                    .Where(n => n.RecipientId == player.Id
                        && n.Kind == NotificationKind.CampaignSoon
                        && n.Status == NotificationStatus.NotSent);
                _dbContext.RemoveRange(notificationsToRemove);
            }

            _dbContext.SaveChanges();
            return true;
        }

        public Player GetPlayerById(int id)
        {
            var player = _dbContext.Player.FirstOrDefault(p => p.Id == id);
            return player;
        }
    }
}