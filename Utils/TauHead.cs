using System.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TauManager.Models;
using System.Collections.Generic;

namespace TauManager.Utils
{
    public class TauHead: ITauHeadClient
    {
        public const string UrlBase = "https://www.tauhead.com";
        public const string ItemUrlBase = "https://www.tauhead.com/item/";

        public string UrlFromSlug(string slug)
        {
            return ItemUrlBase + slug;
        }
        public async Task<Item> GetItemData(string url)
        {
            Item item = null;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            var result = await client.GetAsync(url);
            if (result.IsSuccessStatusCode) 
            {
                var responseObject = JsonConvert.DeserializeObject<TauHeadItem>(await result.Content.ReadAsStringAsync());
                item = _parseItemData(responseObject);
            }
            return item;
        }

        public IDictionary<string, Item> BulkParseItems(string jsonContent)
        {
            var result = new Dictionary<string, Item>();
            var itemObjects = JsonConvert.DeserializeObject<TauHeadItemList>(jsonContent);
            foreach(var itemObj in itemObjects.items)
            {
                var newItem = _parseItemData(itemObj);
                result[itemObj.slug] = newItem;
            }
            return result;
        }

        private Item _parseItemData(TauHeadItem responseObject)
        {
            Item item;
            Item.ItemType itemType;
            var itemTypeParseResult = Enum.TryParse<Item.ItemType>(responseObject.item_type_slug, true, out itemType);
            if (!itemTypeParseResult) return null;
            Item.ItemRarity ItemRarity;
            var itemRarityParseResult = Enum.TryParse<Item.ItemRarity>(responseObject.rarity, true, out ItemRarity);
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
            return item;
        }
    }
}