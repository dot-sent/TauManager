using System.Collections.Generic;
using System.Threading.Tasks;
using TauManager.Models;

namespace TauManager.Utils
{
    public interface ITauHeadClient
    {
        string UrlFromSlug(string slug);
        Task<Item> GetItemData(string url);
        IDictionary<string, Item> BulkParseItems(string jsonContent);
    }
}