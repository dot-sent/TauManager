using System.Threading.Tasks;
using TauManager.Models;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public interface ILootLogic
    {
        LootDistributionListModel GetCurrentDistributionOrder(int? campaignId, bool includeInactive, bool undistributedLootOnly, int syndicateId, int? playerId);
        Task<bool> AppendPlayerToBottomAsync(int id, int? lootRequestId, string comment);
        Task<bool> SetLootStatusAsync(int id, CampaignLoot.CampaignLootStatus status);
        Task<bool> SetLootHolderAsync(int id, int playerId);
        LootItemViewModel CreateNewLootApplication(int id, int playerId, int? currentPlayerId);
        Task<bool> ApplyForLoot(int lootId, int playerId, string comments, int? currentPlayerId, bool specialOffer, bool deleteRequest);
        Task<bool> SetLootRequestStatus(int playerId, int currentPlayerId, int campaignLootId, int status, int lootStatus, string comments, bool dropRequestorDown);
        LootOverviewViewModel GetOverview(int[] display, int itemTier, int itemType, int syndicateId);
        LootitemRequestsViewModel GetLootRequestsInfo(int campaignLootId);
        Task<bool> AwardLoot(int lootId, int? lootRequestId, CampaignLoot.CampaignLootStatus status, bool? lootAvailableToOtherSyndicates);
    }
}