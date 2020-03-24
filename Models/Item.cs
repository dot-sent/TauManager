using System;
using System.ComponentModel.DataAnnotations;
using TauManager.Utils;

namespace TauManager.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Item
    {
        public const string UrlBase = "https://alpha.taustation.space/item/";
        public enum ItemType : byte { Armor, Weapon, Medical, Blueprint, Event, Mission, Mod, Ration, TradeGood, VIP };
        public enum ItemTypeFilters : byte { All, Armor, ShortRangeWeapon, LongRangeWeapon};
        public enum ItemRarity : byte { Common, Uncommon, Rare, Epic};
        public enum ItemWeaponRange : byte { Long, Short };
        public enum ItemWeaponType : byte { Blade, Club, Handgun, Rifle, ShortBarrelRifle, Shotgun, SniperRifle };
        #region Common properties
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public int Tier { get; set; }
        public decimal Weight { get; set; }
        public decimal Price { get; set; }
        public ItemRarity Rarity { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Slug { get; set; }

        public string Url 
        { 
            get
            {
                return UrlBase + Slug;
            }
        }
        #endregion
        #region Weapon properties
        public decimal? Accuracy { get; set; }
        public bool? HandToHand { get; set; }
        public ItemWeaponRange? WeaponRange { get; set; }
        public ItemWeaponType? WeaponType { get; set; }
        #endregion
        #region Weapon and armor properties
        public decimal? Piercing { get; set; }
        public decimal? Impact { get; set; }
        public decimal? Energy { get; set; }
        #endregion
        #region Calculated properties
        public string Caption 
        { 
            get
            {
                return String.Format("{0} Tier {1} {2}",
                    Rarity.ToString(),
                    Tier, 
                    (Type == ItemType.Weapon && WeaponType.HasValue ? WeaponType.Value.ToString() : Type.ToString()));
            }
        }
        public string LDLCaption
        { 
            get
            {
                return String.Format("T{0} {1} ({2:N0}/{3:N0}/{4:N0})",
                    Tier,
                    (Type == ItemType.Weapon && WeaponType.HasValue ? WeaponType.Value.ToStringSplit() : Type.ToStringSplit()),
                    Piercing.HasValue ? Piercing.Value : 0,
                    Impact.HasValue ? Impact.Value : 0,
                    Energy.HasValue ? Energy.Value : 0);
            }
        }
        public string CampaignOverviewCaption {
            get
            {
                return String.Format("{0} Tier {1} {2} ({3:N0}/{4:N0}/{5:N0})",
                    Rarity.ToString(),
                    Tier,
                    (Type == ItemType.Weapon && WeaponType.HasValue ? WeaponType.Value.ToString() : Type.ToString()),
                    Piercing.HasValue ? Piercing.Value : 0,
                    Impact.HasValue ? Impact.Value : 0,
                    Energy.HasValue ? Energy.Value : 0);
            }
        }
        #endregion
    }
}