using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TauManager.Areas.Identity;
using TauManager.Areas.Identity.Data;
using TauManager.BusinessLogic;
using TauManager.Models;
using TauManager.Utils;

namespace TauManager.Controllers
{
    [Authorize]
    public class CampaignsController : Controller
    {
        #region session keys
        private const string KeyIncludeInactive = "LootDistributionList_IncludeInactive";
        private const string KeyCampaignId = "LootDistributionList_CampaignId";
        private const string KeyUndistributedLootOnly = "LootDistributionList_UndistributedLootOnly";
        #endregion

        private ICampaignLogic _campaignLogic { get; set; }
        private ILootLogic _lootLogic { get; set; }
        public ApplicationIdentityUserManager _userManager { get; set; }

        public CampaignsController(ICampaignLogic logic, ILootLogic lootLogic, ApplicationIdentityUserManager userManager)
        {
            _campaignLogic = logic;
            _lootLogic = lootLogic;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var playerId = await _userManager.GetPlayerIdAsync(User);
            var model = _campaignLogic.GetCampaignOverview(
                playerId ?? 0,
                true,
                User.IsInRole(ApplicationRoleManager.Leader) || User.IsInRole(ApplicationRoleManager.Officer));
            return View(model);
        }

        [AuthorizeRoles(ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
        public IActionResult Create()
        {
            var model = _campaignLogic.GetNewCampaign();
            return View("Details", model);
        }

        [AuthorizeRoles(ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
        public IActionResult Details(int id, string alert = null)
        {
            var model = _campaignLogic.GetCampaignById(id, false, true);
            if (model == null || model.Campaign == null) return NotFound();
            model.Alert = alert;
            return View(model);
        }

        [HttpPost]
        [AuthorizeRoles(ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
        public async Task<IActionResult> Edit(Campaign model)
        {
            var isNew = model.Id == 0;
            var newModel = await _campaignLogic.CreateOrEditCampaign(model);
            return RedirectToAction("Details", new {id = newModel.Id, alert = @"Campaign #" + newModel.Id + " successfully " + (isNew ? "created" : "updated") + "."});
        }

        [HttpPost]
        [AuthorizeRoles(ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
        public async Task<IActionResult> AddLootByTauheadURL(int campaignId, string url)
        {
            var model = await _campaignLogic.AddLootByTauheadURL(campaignId, url, showLootApplyButton: false, showLootEditControls: true);
            if (model == null) return Conflict();
            return PartialView("_LootCardPartial", model);
        }

        public IActionResult Loot(int[] display = null)
        {
            var model = _lootLogic.GetOverview(display);
            return View(model);
        }

        public IActionResult LootDistributionList()
        {
            var id = HttpContext.Session.Get<int?>(KeyCampaignId, null);
            var includeInactive = HttpContext.Session.Get<bool>(KeyIncludeInactive, true);
            var undistributedLootOnly = HttpContext.Session.Get<bool>(KeyUndistributedLootOnly, true);
            var model = _lootLogic.GetCurrentDistributionOrder(campaignId: id, includeInactive, undistributedLootOnly);
            return View(model);
        }

        public IActionResult SetLootDistributionListParams(int? id, bool includeInactive, bool undistributedLootOnly)
        {
            HttpContext.Session.Set<int?>(KeyCampaignId, id.HasValue && id == 0 ? null : id);
            HttpContext.Session.Set<bool>(KeyIncludeInactive, includeInactive);
            HttpContext.Session.Set<bool>(KeyUndistributedLootOnly, undistributedLootOnly);
            return Ok();
        }

        [HttpPost]
        [AuthorizeRoles(ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
        public async Task<IActionResult> AppendPlayerToDistributionListBottom(int id, int lootRequestId, string comment)
        {
            var result = await _lootLogic.AppendPlayerToBottomAsync(id, 
                lootRequestId == -1 ? null : (int?)lootRequestId,
                comment);
            if (!result) return Conflict();
            return RedirectToAction("LootDistributionList");
        }

        [AuthorizeRoles(ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
        public async Task<IActionResult> SetLootStatus(int id, CampaignLoot.CampaignLootStatus status)
        {
            var result = await _lootLogic.SetLootStatusAsync(id, status);
            if (!result) return NotFound();
            return Ok();
        }

        [AuthorizeRoles(ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
        public async Task<IActionResult> SetLootHolder(int id, int holderId)
        {
            var result = await _lootLogic.SetLootHolderAsync(id, holderId);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ApplyForLoot(int id)
        {
            var currentPlayerId = await _userManager.GetPlayerIdAsync(User);
            var newModel = _lootLogic.CreateNewLootApplication(id, currentPlayerId ?? 0, currentPlayerId);
            return View(newModel);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyForLoot(int id, string comments, bool specialOffer)
        {
            var currentPlayerId = await _userManager.GetPlayerIdAsync(User);
            var newModel = await _lootLogic.ApplyForLoot(id, currentPlayerId ?? 0, comments, currentPlayerId, specialOffer);
            return View("LootApplicationStatus", newModel);
        }

        [HttpPost]
        [AuthorizeRoles(ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
        public async Task<IActionResult> SetLootRequestStatus(int playerId, int campaignLootId, int status, int lootStatus, string comments, bool dropDown)
        {
            var currentPlayerId = await _userManager.GetPlayerIdAsync(User);
            var result = await _lootLogic.SetLootRequestStatus(playerId, currentPlayerId ?? 0, campaignLootId, status, lootStatus, comments, dropDown);
            if (result)
            {
                return Ok();
            }
            return Conflict();
        }

        [HttpPost]
        [AuthorizeRoles(ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
        public async Task<IActionResult> ImportCampaignFile(IFormFile campaignResult, int campaignId)
        {
            var reader = new StreamReader(campaignResult.OpenReadStream());
            string fileContents = reader.ReadToEnd();
            reader.Close();
            var model = await _campaignLogic.ParseCampaignPage(fileContents, campaignId);
            return RedirectToAction("Details", new{ id = campaignId, alert = "Campaign results imported successfully."});
        }

        [HttpPost]
        public async Task<IActionResult> SetSignupStatus(int campaignId, bool status)
        {
            var currentPlayerId = await _userManager.GetPlayerIdAsync(User);
            var result = await _campaignLogic.SetSignupStatus(currentPlayerId ?? 0, campaignId, status);
            if (!result) return Conflict();
            return Ok();
        }

        [HttpGet]
        [AuthorizeRoles(ApplicationRoleManager.Administrator)]
        public IActionResult ImportAttendanceCSV()
        {
            return View();
        }

        [HttpPost]
        [AuthorizeRoles(ApplicationRoleManager.Administrator)]
        public async Task<IActionResult> ImportAttendanceCSV(IFormFile csvFile)
        {
            var reader = new StreamReader(csvFile.OpenReadStream());
            string fileContents = reader.ReadToEnd();
            reader.Close();
            var result = await _campaignLogic.ParseAttendanceCSV(fileContents);
            return View(result);
        }
    }
}
