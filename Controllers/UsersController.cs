using Microsoft.AspNetCore.Mvc;
using TauManager.Areas.Identity.Data;
using TauManager.Areas.Identity;
using System.Threading.Tasks;
using TauManager.BusinessLogic;

namespace TauManager.Controllers
{
    [AuthorizeRoles(ApplicationRoleManager.Administrator, ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
    public class UsersController : SyndicateControllerBase
    {
        private IUserLogic _userLogic { get; set; }
        public UsersController(IUserLogic userLogic, ApplicationIdentityUserManager userManager, ISyndicateLogic syndicateLogic): base(syndicateLogic, userManager)
        {
            _userLogic = userLogic;
        }

        public async Task<IActionResult> Index()
        {
            var syndicate = await GetSyndicate();
            var model = await _userLogic.GetUserList(User, syndicate);
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
            var result = await _userLogic.SetUserPlayerAssociation(User, userId, playerId);
            if (!result) return Conflict();
            return Ok();
        }

        public async Task<IActionResult> SetActive(string userId, bool status)
        {
            var result = await _userLogic.SetUserActive(User, userId, status);
            if (!result) return Conflict();
            return Ok();
        }

        [AuthorizeRoles(ApplicationRoleManager.MultiSyndicate)]
        public async Task<IActionResult> SetSyndicateOverride(int? syndicateId)
        {
            var result = await _userLogic.SetSyndicateOverride(User, syndicateId);
            if (!result) return Conflict();
            return Ok();
        }

        [AuthorizeRoles(ApplicationRoleManager.Administrator)]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            var newPassword = await _userLogic.ResetPassword(userId);
            if (newPassword == null) return Conflict();
            return Json(new { newPassword = newPassword });
        }
    }
}