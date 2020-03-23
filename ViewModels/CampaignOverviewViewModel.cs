using System.Collections.Generic;
using TauManager.Models;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class CampaignOverviewViewModel
    {
        public List<Campaign> CurrentCampaigns { get; set; }
        public List<Campaign> FutureCampaigns { get; set; }
        public List<Campaign> PastCampaigns { get; set; }
        public List<LootItemViewModel> LootToDistribute { get; set; }
        public Dictionary<int, int> MySignups { get; set; }
        public Dictionary<int, int> MyAttendance { get; set; }
        public int MyPosition { get; set; }
        public Dictionary<int, string> LootStatuses { get; set; }
        public int PlayerId { get; set; }
    }
}