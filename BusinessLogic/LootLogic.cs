using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TauManager.Areas.Identity.Data;
using TauManager.Models;
using TauManager.Utils;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public class LootLogic : ILootLogic
    {
        private TauDbContext _dbContext { get; set; }
        private ICampaignLogic _campaignLogic { get; set; }
        public LootLogic(TauDbContext dbContext, ICampaignLogic campaignLogic)
        {
            _dbContext = dbContext;
            _campaignLogic = campaignLogic;
        }
        public LootDistributionListModel GetCurrentDistributionOrder(int? campaignId, bool includeInactive, bool undistributedLootOnly)
        {
            var model = new LootDistributionListModel{
                CampaignId = campaignId,
                UndistributedLootOnly = undistributedLootOnly,
                IncludeInactive = includeInactive,
            };
            var playerPositions = _dbContext.PlayerListPositionHistory.GroupBy(plph => plph.PlayerId)
                .Select(g => new {
                    PlayerId =  g.Key,
                    Posiion = g.Max(ph => ph.Id)
                }).Join(
                    _dbContext.Player,
                    ph => ph.PlayerId,
                    p => p.Id,
                    (ph, p) => new { Player = p, Position = ph.Posiion }
                ).OrderBy(
                    p => p.Position
                ).Select(
                    p => p.Player
                ).Where(
                    p => includeInactive || p.Active
                );
            model.CurrentOrder = playerPositions;
            model.AllPlayers = _dbContext.Player.OrderBy(p => p.Name).AsEnumerable();
            model.AllCampaignLoot = _dbContext.CampaignLoot.Where(
                    cl => 
                         cl.CampaignId == campaignId || // Loot for specific campaign
                         (!campaignId.HasValue && (
                             !undistributedLootOnly || cl.Status == CampaignLoot.CampaignLootStatus.Undistributed
                         ))
                )
                .Include(cl => cl.Item)
                .GroupBy(cl => cl.CampaignId)
                .Select(g => new {
                    CampaignId = g.Key,
                    Loot = g.OrderBy(cl => cl.Item.Tier)
                }).ToDictionary(g => g.CampaignId, g => g.Loot.AsEnumerable());
            model.AllLootRequests = _dbContext.LootRequest
                .Include(lr => lr.Loot)
                .GroupBy(lr => lr.RequestedForId)
                .Select(g => new {
                    PlayerId = g.Key,
                    Requests = g.Where(lr => !campaignId.HasValue || lr.Loot.CampaignId == campaignId)
                        .ToDictionary(lr => lr.Loot.Id, lr => lr)
                }).ToDictionary(g => g.PlayerId, g => g.Requests);
            model.AllCampaigns = _dbContext.Campaign.Where(c => model.AllCampaignLoot.ContainsKey(c.Id))
                .ToDictionary(c => c.Id, c => c);
            model.LootStatuses = EnumExtensions.ToDictionary<int>(typeof(CampaignLoot.CampaignLootStatus));
            var attendanceModel = _campaignLogic.GetCampaignAttendance(null);
            model.TotalAttendanceRate = attendanceModel.TotalAttendance;
            model.HardT5AttendanceRate = attendanceModel.Last10T5HardAttendance;
            return model;
        }

        public async Task<bool> AppendPlayerToBottomAsync(int id, int? lootRequestId, string comment)
        {
            if (lootRequestId == null && comment == null) return false;
            var playerExists = _dbContext.Player.Any(p => p.Id == id);
            if (!playerExists) return false;
            var lootRequestExists = !lootRequestId.HasValue || _dbContext.LootRequest.Any(lr => lr.Id == lootRequestId.Value);
            if (!lootRequestExists) return false;
            var newEntry = new PlayerListPositionHistory{
                PlayerId = id,
                LootRequestId = lootRequestId,
                Comment = comment,
                CreatedAt = DateTime.Now
            };
            _dbContext.Add(newEntry);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetLootStatusAsync(int id, CampaignLoot.CampaignLootStatus status)
        {
            var loot = _dbContext.CampaignLoot.SingleOrDefault(lr => lr.Id == id);
            if (loot == null) return false;
            loot.Status = status;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetLootHolderAsync(int id, int playerId)
        {
            var loot = _dbContext.CampaignLoot.SingleOrDefault(lr => lr.Id == id);
            if (loot == null) return false;
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == playerId);
            if (playerId != 0 && player == null) return false;
            loot.HolderId = playerId == 0 ? null : (int?)playerId;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public LootItemViewModel CreateNewLootApplication(int lootItemId, int playerId, int? currentPlayerId)
        {
            var lootItem = _dbContext.CampaignLoot.SingleOrDefault(cl => cl.Id == lootItemId);
            if (lootItem == null) return null;
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == playerId);
            if (player == null) return null;
            var existingRequest = _dbContext.LootRequest.SingleOrDefault(lr => lr.LootId == lootItemId && lr.RequestedForId == playerId);
            LootItemViewModel result;
            if (existingRequest != null) 
            { 
                result = new LootItemViewModel{
                    Request = existingRequest,
                    Loot = existingRequest.Loot,
                    ShowApplyButton = true,
                    ShowSingleItemInterface = true,
                    ShowEditControls = false,
                    LootStatuses = EnumExtensions.ToDictionary<int>(typeof(CampaignLoot.CampaignLootStatus)),
                };
            } else {
                var newRequest = new LootRequest
                {
                    LootId = lootItemId,
                    Loot = lootItem,
                    RequestedForId = playerId,
                    RequestedById = currentPlayerId ?? 0,
                };
                result = new LootItemViewModel{
                    Request = newRequest,
                    Loot = lootItem,
                    ShowApplyButton = true,
                    ShowSingleItemInterface = true,
                    ShowEditControls = false,
                    LootStatuses = EnumExtensions.ToDictionary<int>(typeof(CampaignLoot.CampaignLootStatus)),
                };
            }
            return result;
        }

        public async Task<bool> ApplyForLoot(int lootId, int playerId, string comments, int? currentPlayerId, bool specialOffer)
        {
            var lootItem = _dbContext.CampaignLoot.SingleOrDefault(cl => cl.Id == lootId);
            if (lootItem == null) return false;
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == playerId);
            if (player == null) return false;
            var lootRequest = _dbContext.LootRequest.SingleOrDefault(lr => lr.LootId == lootId && lr.RequestedForId == playerId);
            if (lootRequest == null)
            {
                lootRequest = new LootRequest
                {
                    LootId = lootId,
                    RequestedById = currentPlayerId ?? 0,
                    RequestedForId = playerId,
                    Status = specialOffer ? LootRequest.LootRequestStatus.SpecialOffer : LootRequest.LootRequestStatus.Interested,
                    SpecialOfferDescription = comments,
                };
                await _dbContext.AddAsync(lootRequest);
            } else {
                lootRequest.Status = specialOffer ? LootRequest.LootRequestStatus.SpecialOffer : LootRequest.LootRequestStatus.Interested;
                lootRequest.SpecialOfferDescription = comments;
            }
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetLootRequestStatus(int playerId, int currentPlayerId, int campaignLootId, int status, int lootStatus, string comments, bool dropRequestorDown)
        {
            var lootItem = _dbContext.CampaignLoot.SingleOrDefault(cl => cl.Id == campaignLootId);
            if (lootItem == null) return false;
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == playerId);
            if (player == null) return false;
            var lootRequest = _dbContext.LootRequest.SingleOrDefault(lr => lr.LootId == campaignLootId && lr.RequestedForId == playerId);
            if (lootRequest == null)
            {
                if (status == -1) return false; // What is dead may never die
                lootRequest = new LootRequest
                {
                    LootId = campaignLootId,
                    RequestedById = currentPlayerId,
                    RequestedForId = playerId,
                    Status = (LootRequest.LootRequestStatus)status,
                    SpecialOfferDescription = comments,
                };
                if (lootStatus > -1)
                {
                    lootItem.HolderId = playerId;
                    lootItem.Status = (CampaignLoot.CampaignLootStatus)lootStatus;
                }
                await _dbContext.AddAsync(lootRequest);
            } else if (status == -1) {
                if (lootRequest.HistoryEntry != null)
                {
                    _dbContext.Remove(lootRequest.HistoryEntry);
                }                
                _dbContext.Remove(lootRequest);
            } else {
                lootRequest.Status = (LootRequest.LootRequestStatus)status;
                lootRequest.SpecialOfferDescription = comments;
                if (status == (int)LootRequest.LootRequestStatus.Awarded)
                {
                    lootRequest.Loot.HolderId = playerId;
                }
                if (lootStatus > -1)
                {
                    lootItem.HolderId = playerId;
                    lootItem.Status = (CampaignLoot.CampaignLootStatus)lootStatus;
                }
            }
            if (dropRequestorDown)
            {
                var newHistoryEntry = new PlayerListPositionHistory
                {
                    LootRequest = lootRequest,
                    PlayerId = playerId,
                    CreatedAt = DateTime.Now,
                    Comment = "Drop associated with loot request",
                };
                await _dbContext.AddAsync(newHistoryEntry);
            }
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public LootOverviewViewModel GetOverview(int[] display)
        {
            var lootStatuses = EnumExtensions.ToDictionary<int>(typeof(CampaignLoot.CampaignLootStatus));
            if (display == null || display.Length == 0)
            {
                display = lootStatuses.Keys.ToArray();
            }
            var model = new LootOverviewViewModel{
                AllLoot = _dbContext.CampaignLoot.Where(cl => display == null || display.Contains((int)cl.Status)).OrderByDescending(cl => cl.CampaignId),
                LootStatuses = EnumExtensions.ToDictionary<int>(typeof(CampaignLoot.CampaignLootStatus)),
                Display = display,
            };
            return model;
        }
    }
}