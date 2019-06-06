using System.Collections.Generic;
using TauManager.Models;

namespace TauManager.ViewModels
{
    public class LootOverviewViewModel
    {
        public IEnumerable<CampaignLoot> AllLoot { get; set; }
        public Dictionary<int, string> LootStatuses { get; set; }
        public int[] Display { get; set; }
    }
}