using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TauManager.Models;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public interface IMarketLogic
    {
        MarketIndexViewModel GetIndexViewModel(int? playerId, MarketIndexParamsViewModel filters);
        Task<PlayerAdsViewModel> GetMyAds(int? playerId);
        Task<ActionResponseViewModel> EditAd(int id, int? playerId, int adType, string offer, string request, string description, bool active);
        MarketAd GetAdById(int id, int? playerId);
        Task<ActionResponseViewModel> SetAdActive(int id, int? playerId, bool active);
        Task<ActionResponseViewModel> RemoveAd(int id, int? playerId);
        Task<ActionResponseViewModel> ImportMarketCSV(IFormFile inputFile, int? playerId);
    }
}