using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Nito.AsyncEx.Synchronous;
using TauManager.Areas.Identity.Data;
using TauManager.Models;
using TauManager.Utils;
using TauManager.ViewModels;

namespace TauManager.ViewComponents
{
    public class UserThemeSwitcherViewComponent : ViewComponent
    {
        private ApplicationIdentityUserManager _userManager { get; set; }

        public UserThemeSwitcherViewComponent(ApplicationIdentityUserManager userManager)
        {
            _userManager = userManager;
        }

        public IViewComponentResult Invoke()
        {
            var model = new UserThemeSwitcherViewModel{
                AllThemes = EnumExtensions.ToDictionary<int>(typeof(UserCSSTheme)),
                CurrentTheme = _userManager.GetUserCSSThemeAsync(User as ClaimsPrincipal).WaitAndUnwrapException()
            };
            return View(model);
        }
    }
}