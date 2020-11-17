using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Nito.AsyncEx.Synchronous;
using TauManager.Areas.Identity.Data;
using TauManager.Models;

namespace TauManager.ViewComponents
{
    public class TableSorterThemeProviderViewComponent: ViewComponent
    {
        private ApplicationIdentityUserManager _userManager { get; set; }

        public TableSorterThemeProviderViewComponent(ApplicationIdentityUserManager userManager)
        {
            _userManager = userManager;
        }

        public IViewComponentResult Invoke()
        {
            var CSSTheme = _userManager.GetUserCSSThemeAsync(User as ClaimsPrincipal).WaitAndUnwrapException();
            var model = TableSorterTheme.Default;
            switch (CSSTheme)
            {
                case UserCSSTheme.Default:
                case UserCSSTheme.Flatly:
                case UserCSSTheme.Journal:
                case UserCSSTheme.Litera:
                case UserCSSTheme.Lux:
                case UserCSSTheme.Minty:
                case UserCSSTheme.Pulse:
                case UserCSSTheme.Sandstone:
                case UserCSSTheme.Simplex:
                case UserCSSTheme.Sketchy:
                case UserCSSTheme.Spacelab:
                case UserCSSTheme.United:
                    model = TableSorterTheme.Default;
                    break;
                case UserCSSTheme.Cyborg:
                case UserCSSTheme.Darkly:
                case UserCSSTheme.Slate:
                case UserCSSTheme.Solar:
                case UserCSSTheme.Superhero:
                    model = TableSorterTheme.Dark;
                    break;
                case UserCSSTheme.Cerulean:
                case UserCSSTheme.Cosmo:
                case UserCSSTheme.Lumen:
                case UserCSSTheme.Materia:
                case UserCSSTheme.Yeti:
                    model = TableSorterTheme.Blue;
                    break;
            }
            return View(model);
        }

    }
}