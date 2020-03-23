using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TauManager.Areas.Identity.Data;
using TauManager.Areas.Identity;
using Microsoft.AspNetCore.Http;
using TauManager.BusinessLogic;
using System.IO;

namespace TauManager.Controllers
{
    public class InitialSetupController : Controller
    {
        private RoleManager<IdentityRole> _roleManager { get; set; }
        private ApplicationIdentityUserManager _userManager { get; set; }
        private IInternalLogic _internalLogic { get; set; }
        public InitialSetupController(
            RoleManager<IdentityRole> roleManager,
            ApplicationIdentityUserManager userManager,
            IInternalLogic internalLogic)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _internalLogic = internalLogic;
        }
        public async Task<IActionResult> Index()
        {
            if (!await _roleManager.RoleExistsAsync(ApplicationRoleManager.Administrator))
            {
                await CreateRoles();
            }
            if (!await _roleManager.RoleExistsAsync(ApplicationRoleManager.MultiSyndicate))
            {
                await _roleManager.CreateAsync(new IdentityRole(ApplicationRoleManager.MultiSyndicate));
            }
            var admins = await _userManager.GetUsersInRoleAsync(ApplicationRoleManager.Administrator);
            if (admins.Count == 0)
            {
                return View();
            } else {
                if ((await _userManager.GetUsersInRoleAsync(ApplicationRoleManager.MultiSyndicate)).Count == 0)
                {
                    foreach (var admin in admins) 
                    {
                        await _userManager.AddToRoleAsync(admin, ApplicationRoleManager.MultiSyndicate);
                    }
                }
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
                await _userManager.AddToRoleAsync(initialUser, ApplicationRoleManager.MultiSyndicate);
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

        [AuthorizeRoles(ApplicationRoleManager.Administrator)]
        [HttpGet]
        public IActionResult ImportItemsFromTauhead()
        {
            return View();
        }

        [AuthorizeRoles(ApplicationRoleManager.Administrator)]
        [HttpPost]
        public async Task<IActionResult> ImportItemsFromTauhead(IFormFile inputFile)
        {
            var reader = new StreamReader(inputFile.OpenReadStream());
            string fileContents = reader.ReadToEnd();
            reader.Close();

            var result = await _internalLogic.ImportItemsFromTauhead(fileContents);
            return View(result);
        }
    }
}