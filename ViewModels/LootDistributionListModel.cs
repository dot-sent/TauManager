using System.Collections.Generic;
using TauManager.Models;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class LootDistributionListModel
    {
        public IEnumerable<Player> CurrentOrder { get; set; }
        public IEnumerable<Player> AllPlayers { get; set; }
        public Dictionary<int, IEnumerable<CampaignLoot>> AllCampaignLoot { get; set; }
        public Dictionary<int, Dictionary<int, LootRequest>> AllLootRequests { get; set; }
        public Dictionary<int, Campaign> AllCampaigns { get; set; }
        public Dictionary<int, string> LootStatuses { get; set; }
        public int? CampaignId { get; set; }
        public bool UndistributedLootOnly { get; set; }
        public bool IncludeInactive { get; set; }
        public Dictionary<int, int> TotalAttendanceRate { get; set; }
        public Dictionary<int, int> HardT5AttendanceRate { get; set; }
        public Player CurrentPlayer { get; set; }
    }
}