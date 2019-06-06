using System.Collections.Generic;
using TauManager.Models;

namespace TauManager.ViewModels
{
    public class CampaignOverviewViewModel
    {
        public IEnumerable<Campaign> CurrentCampaigns { get; set; }
        public IEnumerable<Campaign> FutureCampaigns { get; set; }
        public IEnumerable<Campaign> PastCampaigns { get; set; }
        public IEnumerable<LootItemViewModel> LootToDistribute { get; set; }
        public Dictionary<int, int> MySignups { get; set; }
        public Dictionary<int, int> MyAttendance { get; set; }
        public int MyPosition { get; set; }
    }
}