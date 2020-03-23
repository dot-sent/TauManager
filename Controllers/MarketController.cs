using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TauManager.Areas.Identity;
using TauManager.Areas.Identity.Data;
using TauManager.BusinessLogic;
using TauManager.ViewModels;

namespace TauManager.Controllers
{
    [Authorize]
    public class MarketController : SyndicateControllerBase
    {
        private IMarketLogic _marketLogic { get; set; }
        public MarketController(IMarketLogic marketLogic, ISyndicateLogic syndicateLogic, ApplicationIdentityUserManager userManager) : base(syndicateLogic, userManager)
        {
            _marketLogic = marketLogic;
        }

        public async Task<IActionResult> Index([FromForm]MarketIndexParamsViewModel filters)
        {
            var userObj = await _userManager.GetUserAsync(User);
            var filtersObj = filters;
            if (filtersObj.Sort == null)
            {
                filtersObj.Sort = userObj.MarketSort == null ? MarketIndexParamsViewModel.SortOrder.DateDescending : (MarketIndexParamsViewModel.SortOrder)userObj.MarketSort.Value;
            } else {
                userObj.MarketSort = (byte)filtersObj.Sort;
            }
            if (filtersObj.View == null)
            {
                filtersObj.View = userObj.MarketView == null ? MarketIndexParamsViewModel.ViewKind.Tiles : (MarketIndexParamsViewModel.ViewKind)userObj.MarketView.Value;
            } else {
                userObj.MarketView = (byte)filtersObj.View;
            }
            if (filtersObj.FilterTabPinned == null)
            {
                filtersObj.FilterTabPinned = userObj.MarketIsViewPinned == null ? false : userObj.MarketIsViewPinned.Value;
            } else {
                userObj.MarketIsViewPinned = filtersObj.FilterTabPinned;
            }
            await _userManager.UpdateAsync(userObj);

            var model = _marketLogic.GetIndexViewModel(await _userManager.GetPlayerIdAsync(User), filtersObj);
            return View(model);
        }

        public async Task<IActionResult> MyAds(string messages = "")
        {
            var model = await _marketLogic.GetMyAds(await _userManager.GetPlayerIdAsync(User));
            model.Messages = messages;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditAd(int id, int adType, string offer, string request, string description, bool active = true)
        {
            var result = await _marketLogic.EditAd(id, await _userManager.GetPlayerIdAsync(User), adType, offer, request, description, active);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> SetAdActive(int id, bool active)
        {
            var result = await _marketLogic.SetAdActive(id, await _userManager.GetPlayerIdAsync(User), active);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAd(int id)
        {
            var result = await _marketLogic.RemoveAd(id, await _userManager.GetPlayerIdAsync(User));
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> ImportMarketCSV(IFormFile inputFile)
        {
            var result = await _marketLogic.ImportMarketCSV(inputFile, await _userManager.GetPlayerIdAsync(User));
            return RedirectToAction("MyAds", new {messages = result.Message});
        }
    }
}