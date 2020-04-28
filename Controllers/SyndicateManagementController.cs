using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TauManager.Areas.Identity;
using TauManager.Areas.Identity.Data;
using TauManager.BusinessLogic;
using System.Threading.Tasks;
using TauManager.Utils;
using Microsoft.AspNetCore.Authorization;
using TauManager.Models;
using TauManager.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace TauManager.Controllers
{
    [AuthorizeRoles(ApplicationRoleManager.Administrator, ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
    public class SyndicateManagementController : SyndicateControllerBase
    {
        #region Session keys
        private const string KeyIncludeInactive = "SyndicateManagement_IncludeInactive";
        #endregion

        private IPlayerLogic _playerLogic{ get; set; }
        public SyndicateManagementController(IPlayerLogic logic, ISyndicateLogic syndicateLogic, ApplicationIdentityUserManager userManager): 
            base(syndicateLogic, userManager)
        {
            _playerLogic = logic;
        }

        public async Task<IActionResult> Index()
        {
            var includeInactive = HttpContext.Session.Get<bool>(KeyIncludeInactive, false);
            var model = _playerLogic.GetSyndicateMetrics(null, includeInactive, (await GetSyndicate()).Id);
            return View(model);
        }

        public IActionResult ImportFile()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ImportFile(List<IFormFile> files, string token)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null && string.IsNullOrWhiteSpace(token)) return Unauthorized();
            if (user == null) user = _userManager.Users.SingleOrDefault(u => u.PlayerPageUploadToken == token);
            if (user == null ||
                (
                    !(await _userManager.IsInRoleAsync(user, ApplicationRoleManager.Leader)) &&
                    !(await _userManager.IsInRoleAsync(user, ApplicationRoleManager.Officer)) &&
                    !(await _userManager.IsInRoleAsync(user, ApplicationRoleManager.Administrator))
                )) return Unauthorized();

            var syndicateId = (await GetNativeSyndicate()).Id;

            var model = new Dictionary<string, string>();
            foreach(var formFile in files)
            {
                string message = "";
                if (formFile.Length > 256000)
                {
                    message = $"Cannnot be processed: File size ({formFile.Length} bytes) too large";
                } else
                {
                    StreamReader reader = new StreamReader(formFile.OpenReadStream());
                    string fileContents = reader.ReadToEnd();
                    reader.Close();
                    message = await _playerLogic.ParsePlayerPageAsync(fileContents, syndicateId);
                }
                model[formFile.FileName] = message;
            }
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ImportSyndicateStats(SyndicateInfoViewModel entry)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null && string.IsNullOrWhiteSpace(entry.Token)) return Unauthorized();
            if (user == null) user = _userManager.Users.SingleOrDefault(u => u.PlayerPageUploadToken == entry.Token);
            if (user == null ||
                (
                    !(await _userManager.IsInRoleAsync(user, ApplicationRoleManager.Leader)) &&
                    !(await _userManager.IsInRoleAsync(user, ApplicationRoleManager.Officer)) &&
                    !(await _userManager.IsInRoleAsync(user, ApplicationRoleManager.Administrator))
                )) return Unauthorized();
            if (entry.Level == 0) {
                return BadRequest("Zero syndicate level reported - please contact Dotsent");
            }
            var result = await _syndicateLogic.SubmitSyndicateHistory(entry);
            return Json(new {result = result});
        }

        [HttpPost]
        public async Task<IActionResult> SetPlayerActive(int playerId, bool status)
        {
            var result = await _playerLogic.SetPlayerActiveAsync(playerId, status);
            if (!result) return Conflict();
            return Ok();
        }

        public IActionResult PlayerDetails(int id, bool? loadAll)
        {
            var model = _playerLogic.GetPlayerDetails(id, loadAll);
            if (model == null) return NotFound();
            return View(model);
        }

        public IActionResult PlayerDetailsChartData(int id, byte interval, byte dataKind)
        {
            var model = _playerLogic.GetPlayerDetailsChartData(id, interval, dataKind);
            return Json(model);
        }

        public async Task<IActionResult> SkillsOverview(string skillGroupName)
        {
            var model = _playerLogic.GetSkillsOverview(skillGroupName, (await GetSyndicate()).Id);
            return View(model);
        }

        public IActionResult SetSyndicateOverviewParams(bool includeInactive)
        {
            HttpContext.Session.Set<bool>(KeyIncludeInactive, includeInactive);
            return Ok();
        }

        public async Task<IActionResult> GetPlayerPageUploadToken()
        {
            var user = await _userManager.GetUserAsync(User);
            var token = _playerLogic.GetPlayerPageUploadToken();
            user.PlayerPageUploadToken = token;
            await _userManager.UpdateAsync(user);
            return View(model: token);
        }
    }
}