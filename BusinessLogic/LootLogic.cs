using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        public LootDistributionListModel GetCurrentDistributionOrder(int? campaignId, bool includeInactive, bool undistributedLootOnly, int syndicateId, int? playerId)
        {
            var model = new LootDistributionListModel{
                CampaignId = campaignId,
                UndistributedLootOnly = undistributedLootOnly,
                IncludeInactive = includeInactive,
                CurrentPlayer = _dbContext.Player.SingleOrDefault(p => playerId.HasValue && p.Id == playerId.Value),
            };
            var playerPositions = _dbContext.PlayerListPositionHistory.GroupBy(plph => plph.PlayerId)
                .Select(g => new {
                    PlayerId =  g.Key,
                    Posiion = g.Max(ph => ph.Id)
                })
                .Join(
                    _dbContext.Player,
                    ph => ph.PlayerId,
                    p => p.Id,
                    (ph, p) => new { Player = p, Position = ph.Posiion }
                )
                .OrderBy(p => p.Position)
                .Select(p => p.Player)
                .Where(p => (includeInactive || p.Active) && p.SyndicateId == syndicateId);
            model.CurrentOrder = playerPositions;
            model.AllPlayers = _dbContext.Player.Where(p => p.SyndicateId == syndicateId).OrderBy(p => p.Name).AsEnumerable();
            model.AllCampaignLoot = _dbContext.CampaignLoot.Where(
                    cl => 
                         cl.CampaignId == campaignId || // Loot for specific campaign
                         (!campaignId.HasValue && (
                             !undistributedLootOnly || cl.Status == CampaignLoot.CampaignLootStatus.Undistributed
                         ))
                )
                .Join(
                    _dbContext.Campaign,
                    cl => cl.CampaignId,
                    c => c.Id,
                    (cl, c) => new { CampaignLoot = cl, Campaign = c}
                )
                .Where(clc => clc.Campaign.SyndicateId == syndicateId)
                .Select(clc => clc.CampaignLoot)
                .Include(cl => cl.Item)
                .AsEnumerable()
                .GroupBy(cl => cl.CampaignId)
                .Select(g => new {
                    CampaignId = g.Key,
                    Loot = g.OrderBy(cl => cl.Item.Tier)
                }).ToDictionary(g => g.CampaignId, g => g.Loot.AsEnumerable());
            model.AllLootRequests = _dbContext.LootRequest
                .Join(
                    _dbContext.CampaignLoot,
                    lr => lr.LootId,
                    cl => cl.Id,
                    (lr, cl) => new { LootRequest = lr, CampaignLoot = cl }
                )
                .Join(
                    _dbContext.Campaign,
                    lrcl => lrcl.CampaignLoot.CampaignId,
                    c => c.Id,
                    (lrcl, c) => new { LootRequest = lrcl.LootRequest, Campaign = c }
                )
                .Where(lrc => lrc.Campaign.SyndicateId == syndicateId)
                .Select(lrc => lrc.LootRequest)
                .Where(lr => !campaignId.HasValue || lr.Loot.CampaignId == campaignId)
                .Include(lr => lr.Loot)
                .AsEnumerable()
                .GroupBy(lr => lr.RequestedForId)
                .Select(g => new {
                    PlayerId = g.Key,
                    Requests = g.Where(lr => !campaignId.HasValue || lr.Loot.CampaignId == campaignId)
                        .ToDictionary(lr => lr.Loot.Id, lr => lr)
                }).ToDictionary(g => g.PlayerId, g => g.Requests);
            model.AllCampaigns = _dbContext.Campaign
                .Where(c => c.SyndicateId == syndicateId)
                .Include(c => c.Attendance)
                .ToDictionary(c => c.Id, c => c);
            model.LootStatuses = EnumExtensions.ToDictionary<int>(typeof(CampaignLoot.CampaignLootStatus));
            var attendanceModel = _campaignLogic.GetCampaignAttendance(null, syndicateId);
            model.TotalAttendanceRate = attendanceModel.TotalAttendance;
            model.HardT5AttendanceRate = attendanceModel.Last10T5HardAttendance;
            return model;
        }

        public async Task<bool> AppendPlayerToBottomAsync(int id, int? lootRequestId, string comment)
        {
            if (lootRequestId == null && comment == null) return false;
            var playerExists = _dbContext.Player.Any(p => p.Id == id);
            if (!playerExists) return false;
            var lootRequestExists = !lootRequestId.HasValue || _dbContext.LootRequest.Any(lr => lr.Id == lootRequestId.Value && lr.RequestedForId == id);
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
            var lootItem = _dbContext.CampaignLoot.Include(cl => cl.Campaign).SingleOrDefault(cl => cl.Id == lootItemId);
            if (lootItem == null) return null;
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == playerId);
            if (player == null) return null;
            if (lootItem.Campaign.SyndicateId != player.SyndicateId) return null; // TODO: Allow cross-syndicate LRs?
            var existingRequest = _dbContext.LootRequest.SingleOrDefault(lr => lr.LootId == lootItemId && lr.RequestedForId == playerId);
            LootItemViewModel result;
            if (existingRequest != null) 
            { 
                result = new LootItemViewModel{
                    Request = existingRequest,
                    Loot = existingRequest.Loot,
                    ShowApplyButton = true,
                    RequestExists = true,
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

        public async Task<bool> ApplyForLoot(int lootId, int playerId, string comments, int? currentPlayerId, bool specialOffer, bool deleteRequest)
        {
            var lootItem = _dbContext.CampaignLoot.Include(cl => cl.Campaign).SingleOrDefault(cl => cl.Id == lootId);
            if (lootItem == null) return false;
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == playerId);
            if (player == null) return false;
            if (lootItem.Campaign.SyndicateId != player.SyndicateId) return false; // TODO: Allow cross-syndicate LRs?
            var lootRequest = _dbContext.LootRequest.SingleOrDefault(lr => lr.LootId == lootId && lr.RequestedForId == playerId);
            if (deleteRequest)
            {
                if (lootRequest == null) return false;
                _dbContext.Remove(lootRequest);
                await _dbContext.SaveChangesAsync();
                return true;
            }
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
        /// <summary>This method retrieves loot by syndicate/tier/itemType.</summary>
        /// <param name="display">Array of lootStatuses-based filters</param>
        /// <param name="itemTier">0 - all tiers; 1,2,3,4,5 - particular tier</param>
        /// <param name="itemType">enum Item.ItemTypeFilters: 0 - all types, 1 - armors, 2 - shortRangeWeapons, 3 - longRangeWeapons</param>
        /// <param name="syndicateId">Syndicate ID</param>
        public LootOverviewViewModel GetOverview(int[] display, int itemTier, int itemType, int syndicateId)
        {
            var lootStatuses = EnumExtensions.ToDictionary<int>(typeof(CampaignLoot.CampaignLootStatus));
            if (display == null || display.Length == 0)
            {
                display = lootStatuses.Keys.ToArray();
            }
            var model = new LootOverviewViewModel{
                AllLoot = _dbContext.CampaignLoot
                    .Include(l => l.Campaign)
                    .Include(l => l.Holder)
                    .Include(l => l.Item)
                    .Where(l => itemTier == 0 || l.Item.Tier == itemTier)
                    .Where(l => itemType == (int)Item.ItemTypeFilters.All ||
                        (itemType == (int)Item.ItemTypeFilters.Armor && l.Item.Type == Item.ItemType.Armor) ||
                        (itemType == (int)Item.ItemTypeFilters.ShortRangeWeapon && l.Item.WeaponRange == Item.ItemWeaponRange.Short) ||
                        (itemType == (int)Item.ItemTypeFilters.LongRangeWeapon && l.Item.WeaponRange == Item.ItemWeaponRange.Long)
                   )
                    .Join(
                        _dbContext.Campaign,
                        cl => cl.CampaignId,
                        c => c.Id,
                        (cl, c) => new { CampaignLoot = cl, Campaign = c }
                    )
                    .Where(clc => clc.Campaign.SyndicateId == syndicateId)
                    .Select(clc => clc.CampaignLoot)
                    .Where(cl => display == null || display.Contains((int)cl.Status)).OrderByDescending(cl => cl.Campaign.UTCDateTime),
                OtherSyndicatesLoot = _dbContext.CampaignLoot
                    .Include(cl => cl.Campaign)
                    .ThenInclude(clc => clc.Syndicate)
                    .Where(cl => cl.Campaign.SyndicateId != syndicateId && 
                        cl.AvailableToOtherSyndicates == true &&
                        (cl.Status == CampaignLoot.CampaignLootStatus.Undistributed ||
                        cl.Status == CampaignLoot.CampaignLootStatus.StaysWithSyndicate))
                    .ToList(), // ToList() is, sadly, necessary to persist syndicate info/tag.
                LootStatuses = EnumExtensions.ToDictionary<int>(typeof(CampaignLoot.CampaignLootStatus)),
                TypeFilters = EnumExtensions.ToDictionary<int>(typeof(Item.ItemTypeFilters)),
                Display = display,
                ItemTier = itemTier,
                ItemType = itemType
            };
            return model;
        }
        
        /* 
            I'm not adding syndicateId check here since we might allow cross-syndicate requests
            in the future. Currently it shouldn't be possible anyway since the code related
            to creating loot requests has those checks.
        */
        public LootitemRequestsViewModel GetLootRequestsInfo(int campaignLootId)
        {
            var item = _dbContext.CampaignLoot
                .Include(i => i.Requests)
                .ThenInclude(r => r.RequestedFor)
                .Include(i => i.Requests)
                .ThenInclude(r => r.Loot)
                .ThenInclude(l => l.Campaign)
                .ThenInclude(c => c.Attendance)
                .SingleOrDefault(cl => cl.Id == campaignLootId);
            if (item == null) return null;
            var playersOrdered = _dbContext.PlayerListPositionHistory.GroupBy(plph => plph.PlayerId)
                .Select(g => new {
                    PlayerId =  g.Key,
                    Position = g.Max(ph => ph.Id)
                }).Join(
                    _dbContext.Player,
                    ph => ph.PlayerId,
                    p => p.Id,
                    (ph, p) => new { Player = p, Position = ph.Position }
                ).OrderBy(
                    p => p.Position
                ).Select(
                    p => p.Player
                );
            var playerPositions = playersOrdered.Select(p => p.Id).ToList();
            var activePlayerPositions = playersOrdered.Where(p => p.Active).Select(p => p.Id).ToList();

            return new LootitemRequestsViewModel{ Requests = item.Requests
                .OrderBy(
                    r => activePlayerPositions.IndexOf(r.RequestedForId)
                )
                .Select(
                    r => new LootRequestViewModel {
                        Id = r.Id,
                        PlayerId = r.RequestedForId,
                        PlayerName = r.RequestedFor.Name,
                        PlayerPosition = playerPositions.IndexOf(r.RequestedForId) + 1,
                        ActivePlayerPosition = activePlayerPositions.IndexOf(r.RequestedForId) + 1,
                        Status = (LootRequestViewModel.LootRequestStatus)r.Status,
                        SpecialOfferDescription = r.SpecialOfferDescription,
                        AttendedCampaign = r.Loot.Campaign.Attendance.Any(a => a.PlayerId == r.RequestedForId),
                    }
                )
             };
        }

        public async Task<bool> AwardLoot(int lootId, int? lootRequestId, CampaignLoot.CampaignLootStatus status, bool? lootAvailableToOtherSyndicates)
        {
            var loot = _dbContext.CampaignLoot.SingleOrDefault(cl => cl.Id == lootId);
            if (loot == null) return false;
            var lootRequest = lootRequestId.HasValue ? _dbContext.LootRequest.SingleOrDefault(lr => lr.Id == lootRequestId.Value) : null;
            if (lootRequestId.HasValue && lootRequest == null) return false;
            
            loot.Status = status;
            loot.AvailableToOtherSyndicates = lootAvailableToOtherSyndicates;            
            if (lootRequestId.HasValue)
            {
                loot.HolderId = lootRequest.RequestedForId;
                lootRequest.Status = LootRequest.LootRequestStatus.Awarded;
                var newHistoryEntry = new PlayerListPositionHistory
                {
                    LootRequest = lootRequest,
                    PlayerId = lootRequest.RequestedForId,
                    CreatedAt = DateTime.Now,
                    Comment = "Drop associated with loot request",
                };
                await _dbContext.AddAsync(newHistoryEntry);
            }
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}