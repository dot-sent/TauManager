using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TauManager.Models;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class MarketIndexParamsViewModel
    {
        // Required for model binding
        public MarketIndexParamsViewModel() { }
        public enum ViewKind: byte { Tiles, Table, Cards };
        public enum SortOrder: byte { DateAscending, DateDescending }
        public string NamePart { get; set; }
        public IEnumerable<MarketAd.AdType> AdTypes { get; set; }
        public IEnumerable<Item.ItemType> ItemTypes { get; set; }
        public int? MinTier { get; set; }
        public int? MaxTier { get; set; }
        public decimal? MinEnergy { get; set; }
        public decimal? MinImpact { get; set; }
        public decimal? MinPiercing { get; set; }
        public IEnumerable<Item.ItemRarity> ItemRarities { get; set; }
        public IEnumerable<Item.ItemWeaponType> WeaponTypes { get; set; }
        public IEnumerable<Item.ItemWeaponRange> WeaponRanges { get; set; }
        public bool? WeaponIsHandToHand { get; set; }
        [DefaultValue(false)]
        public bool ShowAll { get; set; }
        public ViewKind? View { get; set; }
        public SortOrder? Sort { get; set; }
        public bool? FilterTabPinned { get; set; }

        public bool HasItemLevelFilters {
            get 
            {
                return (MinTier != null && MinTier > 1) ||
                    (MaxTier != null && MaxTier < 5) ||
                    (MinEnergy != null && MinEnergy > 0) ||
                    (MinImpact != null && MinImpact > 0) ||
                    (MinPiercing != null && MinPiercing > 0) ||
                    (ItemRarities != null && ItemRarities.Count() > 0) ||
                    (WeaponTypes != null && WeaponTypes.Count() > 0) ||
                    (WeaponRanges != null && WeaponRanges.Count() > 0) ||
                    (ItemTypes != null && ItemTypes.Count() > 0) ||
                    !string.IsNullOrWhiteSpace(NamePart);
            }
        }
    }
}