using System.Collections.Generic;
using TauManager.Models;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class CampaignDetailsViewModel
    {
        public IEnumerable<Player> Players { get; set; }
        public Campaign Campaign { get; set; }
        public IEnumerable<LootItemViewModel> Loot { get; set; }
        public Dictionary<int, string> DifficultyLevels { get; set; }
        public Dictionary<int, string> Statuses { get; set; }
        public string Alert { get; set; }
        public IEnumerable<Item> KnownEpics { get; set; }
        public CampaignPageParseResultViewModel Messages { get; set; }
    }
}