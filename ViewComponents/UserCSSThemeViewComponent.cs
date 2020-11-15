using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TauManager.Areas.Identity.Data;
using Nito.AsyncEx.Synchronous;

namespace TauManager.ViewComponents
{
    public class UserCSSThemeViewComponent: ViewComponent
    {
        private ApplicationIdentityUserManager _userManager { get; set; }

        public UserCSSThemeViewComponent(ApplicationIdentityUserManager userManager)
        {
            _userManager = userManager;
        }
        public IViewComponentResult Invoke()
        {
            var model = _userManager.GetUserCSSThemeAsync(User as ClaimsPrincipal).WaitAndUnwrapException();
            return View(model);
        }
    }
}