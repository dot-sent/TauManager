using System.Collections.Generic;

namespace TauManager.Utils
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class TauHeadItem
    {
        public class ItemComponentArmor
        {
            public decimal piercing { get; set; }
            public decimal impact { get; set; }
            public decimal energy { get; set; }
        }

        public class ItemComponentWeapon
        {
            public int hand_to_hand { get; set; }
            public string weapon_type { get; set; }
            public decimal piercing_damage { get; set; }
            public decimal impact_damage { get; set; }
            public decimal energy_damage { get; set; }
            public decimal accuracy { get; set; }
            public int is_long_range { get; set; }
        }
        public string rarity { get; set; }
        public string image { get; set; }
        public decimal value { get; set; }
        public string slug { get; set; }
        public string item_type_slug { get; set; }
        public ItemComponentArmor item_component_armor { get; set; }
        public ItemComponentWeapon item_component_weapon { get; set; }
        public decimal mass { get; set; }
        public int tier { get; set; }
        public string name { get; set; }
    }
}