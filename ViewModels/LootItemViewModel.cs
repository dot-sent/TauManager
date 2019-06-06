using System.ComponentModel;
using System.Collections.Generic;
using TauManager.Models;

namespace TauManager.ViewModels
{
    public class LootItemViewModel
    {
        public CampaignLoot Loot { get; set; }
        public bool ShowApplyButton { get; set; }
        [DefaultValue(false)]
        public bool ShowSingleItemInterface { get; set; }
        public bool ShowEditControls { get; set; }
        public Dictionary<int,string> LootStatuses { get; set; }
        public IEnumerable<Player> Players { get; set; }
        [DefaultValue(null)]
        public LootRequest Request { get; set; }
        public Dictionary<int, string> AllRequests { get; set; }
    }
}