using System.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TauManager.Models;

namespace TauManager.Utils
{
    public static class TauHead
    {
        public const string UrlBase = "https://www.tauhead.com";
        public static async Task<Item> GetItemData(string url)
        {
            Item item = null;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            var result = await client.GetAsync(url);
            if (result.IsSuccessStatusCode) 
            {
                var responseObject = JsonConvert.DeserializeObject<TauHeadItem>(await result.Content.ReadAsStringAsync());
                Item.ItemType itemType;
                var itemTypeParseResult = Enum.TryParse<Item.ItemType>(responseObject.item_type_slug.FirstCharToUpper(), false, out itemType);
                if (!itemTypeParseResult) return null;
                Item.ItemRarity ItemRarity;
                var itemRarityParseResult = Enum.TryParse<Item.ItemRarity>(responseObject.rarity.FirstCharToUpper(), false, out ItemRarity);
                if (!itemRarityParseResult) return null;
                item = new Item{
                    Type = itemType,
                    Name = responseObject.name,
                    ImageUrl = UrlBase + responseObject.image,
                    Tier = responseObject.tier,
                    Rarity = ItemRarity,
                    Price = responseObject.value,
                    Slug = responseObject.slug,
                    Weight = responseObject.mass
                };
                if (itemType == Item.ItemType.Armor)
                {
                    if (responseObject.item_component_armor == null) return null;
                    item.Piercing = responseObject.item_component_armor.piercing;
                    item.Impact = responseObject.item_component_armor.impact;
                    item.Energy = responseObject.item_component_armor.energy;
                }
                if (itemType == Item.ItemType.Weapon)
                {
                    if (responseObject.item_component_weapon == null) return null;
                    item.Piercing = responseObject.item_component_weapon.piercing_damage;
                    item.Impact = responseObject.item_component_weapon.impact_damage;
                    item.Energy = responseObject.item_component_weapon.energy_damage;
                    item.Accuracy = responseObject.item_component_weapon.accuracy;
                    item.HandToHand = responseObject.item_component_weapon.hand_to_hand > 0;
                    Item.ItemWeaponType weaponType;
                    var weaponTypeParseResult = Enum.TryParse<Item.ItemWeaponType>(responseObject.item_component_weapon.weapon_type.FirstCharToUpper().Replace(" ",""), false, out weaponType);
                    if (!weaponTypeParseResult) return null;
                    item.WeaponType = weaponType;
                    item.WeaponRange = responseObject.item_component_weapon.is_long_range == 1 ? Item.ItemWeaponRange.Long : Item.ItemWeaponRange.Short;
                }
            }
            return item;
        }
    }
}