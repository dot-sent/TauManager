using System.Collections.Specialized;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TauManager.Areas.Identity.Data;
using TauManager.Areas.Identity;
using System.Collections.Generic;
using TauManager.Models;
using System.Threading.Tasks;
using TauManager.ViewModels;
using TauManager.BusinessLogic;

namespace TauManager.Controllers
{
    [AuthorizeRoles(ApplicationRoleManager.Administrator, ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
    public class UsersController : Controller
    {
        private IUserLogic _userLogic { get; set; }
        public UsersController(IUserLogic userLogic)
        {
            _userLogic = userLogic;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _userLogic.GetUserList(User);
            return View(model);
        }

        [HttpPost]
        [AuthorizeRoles(ApplicationRoleManager.Administrator, ApplicationRoleManager.Leader)]
        public async Task<IActionResult> ChangeRole(string userId, string roleName, bool status)
        {
            var result = await _userLogic.SetUserRole(User, userId, roleName, status);
            if (!result) return Conflict();
            return Ok();
        }

        public async Task<IActionResult> SetPlayer(string userId, int playerId)
        {
            var result = await _userLogic.SetUserPlayerAssociation(userId, playerId);
            if (!result) return Conflict();
            return Ok();
        }

        public async Task<IActionResult> SetActive(string userId, bool status)
        {
            var result = await _userLogic.SetUserActive(User, userId, status);
            if (!result) return Conflict();
            return Ok();
        }
    }
}