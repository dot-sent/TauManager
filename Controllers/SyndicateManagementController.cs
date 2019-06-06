using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TauManager.Areas.Identity;
using TauManager.Areas.Identity.Data;
using TauManager.BusinessLogic;
using System.Threading.Tasks;

namespace TauManager.Controllers
{
    [AuthorizeRoles(ApplicationRoleManager.Administrator, ApplicationRoleManager.Leader, ApplicationRoleManager.Officer)]
    public class SyndicateManagementController : Controller
    {
        private IPlayerLogic _playerLogic{ get; set; }
        public SyndicateManagementController(IPlayerLogic logic)
        {
            _playerLogic = logic;
        }

        public IActionResult Index()
        {
            var model = _playerLogic.GetSyndicateMetrics(null);
            return View(model);
        }

        public IActionResult ImportFile()
        {
            return View();
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> ImportFile(List<IFormFile> files)
        {
            var model = new Dictionary<string, string>();
            foreach(var formFile in files)
            {
                string message = "";
                if (formFile.Length > 128000) 
                {
                    message = $"Cannnot be processed: File size ({formFile.Length} bytes) too large";
                } else
                {
                    StreamReader reader = new StreamReader(formFile.OpenReadStream());
                    string fileContents = reader.ReadToEnd();
                    reader.Close();
                    message = await _playerLogic.ParsePlayerPageAsync(fileContents);
                    model[formFile.FileName] = message;
                }
            }
            return View(model);
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
    }
}