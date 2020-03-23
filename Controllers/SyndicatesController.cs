using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TauManager.Areas.Identity;
using TauManager.Areas.Identity.Data;
using TauManager.BusinessLogic;

namespace TauManager.Controllers
{
    [Authorize]
    public class SyndicatesController: SyndicateControllerBase
    {
        public SyndicatesController(ApplicationIdentityUserManager userManager, ISyndicateLogic syndicateLogic): base(syndicateLogic, userManager)
        {
            
        }

        [AuthorizeRoles(ApplicationRoleManager.MultiSyndicate)]
        public async Task<IActionResult> GetAll()
        {
            var syndicates = await _syndicateLogic.GetAllSyndicates(User);
            return Json(syndicates);
        }

        [HttpGet]
        [AuthorizeRoles(ApplicationRoleManager.Administrator, ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
        [AuthorizeRoles(ApplicationRoleManager.MultiSyndicate)]
        public IActionResult UserAssignment()
        {
            var model = _syndicateLogic.GetSyndicatePlayerAssignment();
            return View(model);
        }

        [HttpPost]
        [AuthorizeRoles(ApplicationRoleManager.Administrator, ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
        [AuthorizeRoles(ApplicationRoleManager.MultiSyndicate)]
        public async Task<IActionResult> SetPlayerSyndicate(int playerId, int syndicateId)
        {
            var result = await _syndicateLogic.SetPlayerSyndicate(playerId, syndicateId == 0 ? null : (int?)syndicateId);
            if (!result) return Conflict();
            return Ok();
        }
    }
}