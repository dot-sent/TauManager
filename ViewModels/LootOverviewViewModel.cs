using System.Collections.Generic;
using TauManager.Models;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class LootOverviewViewModel
    {
        public IEnumerable<CampaignLoot> AllLoot { get; set; }
        public IEnumerable<CampaignLoot> OtherSyndicatesLoot { get; set; }
        public Dictionary<int, string> LootStatuses { get; set; }
        public int[] Display { get; set; }

        public int TierSelect {get; set;}
        public int TypeSelect {get; set;}
    }
}