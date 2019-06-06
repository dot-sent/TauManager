using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TauManager.Areas.Identity.Data;

namespace TauManager.Controllers
{
    public class InitialSetupController : Controller
    {
        private RoleManager<IdentityRole> _roleManager { get; set; }
        private ApplicationIdentityUserManager _userManager { get; set; }

        public InitialSetupController(
            RoleManager<IdentityRole> roleManager,
            ApplicationIdentityUserManager userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (!await _roleManager.RoleExistsAsync(ApplicationRoleManager.Administrator))
            {
                await CreateRoles();
            }
            if ((await _userManager.GetUsersInRoleAsync(ApplicationRoleManager.Administrator)).Count == 0)
            {
                return View();
            }

            return View("SetupComplete");
        }

        public class InitialUser
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Index(InitialUser model)
        {
            if ((await _userManager.GetUsersInRoleAsync(ApplicationRoleManager.Administrator)).Count > 0)
            {
                return View("SetupComplete");
            }
            await CreateInitialUser(model);

            return View("SetupComplete");
        }

        private async Task CreateInitialUser(InitialUser model)
        {
            var initialUser = new ApplicationUser
            {
                UserName = model.UserName,
                IsApproved = true
            };
            var userResult = await _userManager.CreateAsync(
                initialUser,
                model.Password
            );
            if (userResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(initialUser, ApplicationRoleManager.Administrator);
            }
        }

        private async Task CreateRoles()
        {
            IdentityResult roleResult;
            // if we got here, we have no roles available. Let's create them!
            foreach (var role in ApplicationRoleManager.AllRoles)
            {
                roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        public IActionResult SetupComplete()
        {
            return View();
        }
    }
}
