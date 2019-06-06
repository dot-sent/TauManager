using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MathNet.Numerics.Statistics;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public class PlayerLogic : IPlayerLogic
    {
        private TauDbContext _dbContext { get; set; }
        private ICampaignLogic _campaignLogic { get; set; }
        public PlayerLogic(TauDbContext dbContext, ICampaignLogic campaignLogic)
        {
            _dbContext = dbContext;
            _campaignLogic = campaignLogic;
        }

        public SyndicateMetricsViewModel GetSyndicateMetrics(int? playerId)
        {
            var model = new SyndicateMetricsViewModel();
            var allPlayers = _dbContext.Player.AsQueryable(); 
            model.PlayerStats = allPlayers.Where(p => p.Active).GroupBy(p => p.Tier).Select(g => new SyndicateMetricsViewModel.TierStatistics
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

            model.PlayerCountByStatTotal = new Dictionary<string, int>();
            int[] statBoundaries = {0, 100, 250, 500, 750, 1000, 1250, 1500, 10000};
            for (var i = 0; i < statBoundaries.Count()-1; i++)
            {
                var min = statBoundaries[i];
                var max = statBoundaries[i+1];
                model.PlayerCountByStatTotal[min.ToString() + "-" + max.ToString()] = 
                    allPlayers.Count(p => p.StatTotal >= min && p.StatTotal < max && p.Active);
            }
            if (!playerId.HasValue)
            {
                model.Players = allPlayers.GroupBy(p => p.Tier)
                    .Select(g => new {
                        Tier = g.Key,
                        Players = g
                    })
                    .OrderByDescending(t => t.Tier)
                    .ToDictionary(g => g.Tier, g => g.Players.OrderBy(p => p.Name).ToList());
                var histories = _dbContext.PlayerHistory.GroupBy(ph => ph.PlayerId)
                    .Select( g =>
                        new 
                        {
                            playerId = g.Key,
                            last2w = g.Where(ph => DateTime.Now.Subtract(ph.RecordedAt).TotalDays < 14),
                            olderExist = g.Any(ph => DateTime.Now.Subtract(ph.RecordedAt).TotalDays >= 14)
                        }
                    );
                
                model.LastActivity = new Dictionary<int, SyndicateMetricsViewModel.LastPlayerActivity>();
                foreach (var history in histories)
                {
                    var player = allPlayers.SingleOrDefault(p => p.Id == history.playerId);
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
                    model.LastActivity[history.playerId] = new SyndicateMetricsViewModel.LastPlayerActivity
                    {
                        DaysAgo = daysAgo,
                        Active = player.Active,
                    };
                }
                foreach(var player in allPlayers)
                {
                    if (!model.LastActivity.ContainsKey(player.Id))
                    {
                        model.LastActivity[player.Id] = new SyndicateMetricsViewModel.LastPlayerActivity
                        {
                            DaysAgo = 15,
                            Active = player.Active,
                        };
                    }
                }
                model.Attendance = _campaignLogic.GetCampaignAttendance(null);
            } else {
                model.PlayerToCompare = _dbContext.Player.SingleOrDefault(p => p.Id == playerId.Value);
            }
            return model;
        }

        public async Task<string> ParsePlayerPageAsync(string fileContents)
        {
            string message;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(fileContents);
            var characterName = document.DocumentNode.SelectSingleNode("//*[contains(concat(\" \",normalize-space(@class),\" \"),\" character-name \")]").InnerHtml.Trim();
            var recordedAt = DateTime.Parse(document.DocumentNode.SelectSingleNode("//html").Attributes["data-time"].Value);
            var levelText = document.DocumentNode.SelectSingleNode("//*[contains(concat(\" \",normalize-space(@class),\" \"),\" statistics \")]/dd[position()=3]").InnerHtml.Trim();
            var levelParts = levelText.Split(" @ ");
            decimal level = int.Parse(levelParts[0]) + (decimal.Parse(levelParts[1].Replace("%", "")) / 100);
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
            var player = _dbContext.Player.FirstOrDefault(p => p.Name.Equals(characterName));
            if (player == null)
            {
                player = new Models.Player
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
                    Name = characterName,
                };
                var playerHistory = new Models.PlayerHistory(player);
                _dbContext.Add(player);
                _dbContext.Add(playerHistory);
                message = "Player and history entry added.";
            }
            else
            {
                if (_dbContext.PlayerHistory.Any(ph => ph.PlayerId == player.Id && Math.Abs(ph.RecordedAt.Subtract(recordedAt).TotalMinutes) < 1))
                {
                    message = "File has been already processed before";
                }
                else
                {
                    var playerHistory = new Models.PlayerHistory
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
                        _dbContext.Update(player);
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
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == id);
            if (player == null) return null;
            var playerHistory = player.History.Where(ph => (loadAll.HasValue && loadAll.Value)
                || ((DateTime.Now - ph.RecordedAt).TotalDays < 14) ).OrderByDescending(ph => ph.RecordedAt);
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
                Attendance = _campaignLogic.GetCampaignAttendance(id),
            };
        }
    }
}