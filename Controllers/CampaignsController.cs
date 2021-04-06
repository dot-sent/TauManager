using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TauManager.Areas.Identity;
using TauManager.Areas.Identity.Data;
using TauManager.BusinessLogic;
using TauManager.Models;
using TauManager.Utils;
using TauManager.ViewModels;

namespace TauManager.Controllers
{
    [Authorize]
    public class CampaignsController : SyndicateControllerBase
    {
        #region session keys
        private const string KeyIncludeInactive = "LootDistributionList_IncludeInactive";
        private const string KeyCampaignId = "LootDistributionList_CampaignId";
        private const string KeyUndistributedLootOnly = "LootDistributionList_UndistributedLootOnly";
        #endregion

        private ICampaignLogic _campaignLogic { get; set; }
        private ILootLogic _lootLogic { get; set; }
        public CampaignsController(ICampaignLogic logic, ILootLogic lootLogic, ApplicationIdentityUserManager userManager, ISyndicateLogic syndicateLogic): base(syndicateLogic, userManager)
        {
            _campaignLogic = logic;
            _lootLogic = lootLogic;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var playerId = await _userManager.GetPlayerIdAsync(User);
            var model = _campaignLogic.GetCampaignOverview(
                playerId ?? 0,
                true,
                false,
                User.IsInRole(ApplicationRoleManager.Leader) || User.IsInRole(ApplicationRoleManager.Officer),
                (await GetSyndicate()).Id);
            return View(model);
        }

        [AuthorizeRoles(ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
        public async Task<IActionResult> Create()
        {
            var model = _campaignLogic.GetNewCampaign((await GetSyndicate()).Id);
            return View("Details", model);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id, string alert = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var canEdit = _campaignLogic.PlayerCanEditCampaign(await _userManager.GetPlayerIdAsync(User), id) ||
                await _userManager.IsInRoleAsync(user, ApplicationRoleManager.Officer) ||
                await _userManager.IsInRoleAsync(user, ApplicationRoleManager.Leader);
            if (!canEdit) return Forbid();
            var model = _campaignLogic.GetCampaignById(id, false, true, (await GetSyndicate()).Id);
            if (model == null || model.Campaign == null) return NotFound();
            model.Alert = alert;
            model.Messages = TempData.ContainsKey("CampaignImportMessages") ?
                JsonConvert.DeserializeObject<CampaignPageParseResultViewModel>((string)TempData["CampaignImportMessages"]) : null;
            return View(model);
        }

        public async Task<IActionResult> View(int id)
        {
            var model = _campaignLogic.GetCampaignById(id, false, false, (await GetSyndicate()).Id);
            if (model == null || model.Campaign == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(Campaign model)
        {
            var user = await _userManager.GetUserAsync(User);
            var canEdit = _campaignLogic.PlayerCanEditCampaign(await _userManager.GetPlayerIdAsync(User), model.Id) ||
                await _userManager.IsInRoleAsync(user, ApplicationRoleManager.Officer) ||
                await _userManager.IsInRoleAsync(user, ApplicationRoleManager.Leader);
            if (!canEdit) return Forbid();
            var isNew = model.Id == 0;
            var newModel = await _campaignLogic.CreateOrEditCampaign(model, (await GetSyndicate()).Id);
            if (newModel == null) return Forbid();
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
        public async Task<IActionResult> Loot(int[] display = null, int itemTier = 0, int itemType = 0)
        {
            var model = _lootLogic.GetOverview(display, itemTier, itemType, (await GetSyndicate()).Id);
            return View(model);
        }
        public async Task<IActionResult> LootDistributionList()
        {
            var id = HttpContext.Session.Get<int?>(KeyCampaignId, null);
            var includeInactive = HttpContext.Session.Get<bool>(KeyIncludeInactive, false);
            var undistributedLootOnly = HttpContext.Session.Get<bool>(KeyUndistributedLootOnly, true);
            var model = _lootLogic.GetCurrentDistributionOrder(campaignId: id, includeInactive, undistributedLootOnly, (await GetSyndicate()).Id, await _userManager.GetPlayerIdAsync(User));
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
        public async Task<IActionResult> ApplyForLoot(int id, bool isPersonalRequest = false)
        {
            var currentPlayerId = await _userManager.GetPlayerIdAsync(User);
            var newModel = _lootLogic.CreateNewLootApplication(id, currentPlayerId ?? 0, currentPlayerId, isPersonalRequest);
            return View(newModel);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyForLoot(int id, string comments, bool specialOffer, bool collectorRequest, bool isPersonalRequest, bool deleteRequest = false)
        {
            var currentPlayerId = await _userManager.GetPlayerIdAsync(User);
            var newModel = await _lootLogic.ApplyForLoot(id, currentPlayerId ?? 0, comments, currentPlayerId, specialOffer, collectorRequest, isPersonalRequest, deleteRequest);
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
            TempData["CampaignImportMessages"] = JsonConvert.SerializeObject(model);
            return RedirectToAction("Details", new{ 
                id = campaignId, 
                alert = "Campaign results imported successfully. Campaign status set to Completed."
            });
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

        // [HttpPost]
        // [AuthorizeRoles(ApplicationRoleManager.Administrator)]
        // public async Task<IActionResult> ImportAttendanceCSV(IFormFile csvFile)
        // {
        //     var reader = new StreamReader(csvFile.OpenReadStream());
        //     string fileContents = reader.ReadToEnd();
        //     reader.Close();
        //     var result = await _campaignLogic.ParseAttendanceCSV(fileContents);
        //     return View(result);
        // }

        [HttpGet]
        [AuthorizeRoles(ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
        public IActionResult GetLootRequestsJson(int id, bool personalRequests = false)
        {
            var model = _lootLogic.GetLootRequestsInfo(id, personalRequests);
            return Json(model);
        }

        [HttpPost]
        [AuthorizeRoles(ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
        public async Task<IActionResult> AwardLoot(int lootId, int? lootRequestId, CampaignLoot.CampaignLootStatus status, bool? lootAvailableToOtherSyndicates, bool dropRequestorDown = true)
        {
            var result = await _lootLogic.AwardLoot(lootId, lootRequestId, status, lootAvailableToOtherSyndicates, dropRequestorDown);
            if (!result) return Conflict();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Volunteer(int campaignId)
        {
            var playerId = await _userManager.GetPlayerIdAsync(User);
            var result = await _campaignLogic.VolunteerForCampaign(playerId, campaignId);
            if (!result) return Conflict();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Leaderboards()
        {
            var playerId = await _userManager.GetPlayerIdAsync(User);
            var model = _campaignLogic.GetLeaderboard(playerId);
            return View(model);
        }

        [HttpGet]
        [AuthorizeRoles(ApplicationRoleManager.Administrator, ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
        public async Task<IActionResult> PersonalRequests()
        {
            var model = _lootLogic.GetPersonalRequests(await _userManager.GetPlayerIdAsync(User), (await GetSyndicate()).Id);
            return View(model);
        }
    }
}