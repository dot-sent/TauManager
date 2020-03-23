using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TauManager.Areas.Identity.Data;
using TauManager.BusinessLogic;
using TauManager.Models;

namespace TauManager.Controllers
{
    public class HomeController : Controller
    {
        private IPlayerLogic _playerLogic { get; set; }
        private ApplicationIdentityUserManager _userManager { get; set; }

        public HomeController(IPlayerLogic playerLogic, ApplicationIdentityUserManager userManager)
        {
            _playerLogic = playerLogic;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var playerId = await _userManager.GetPlayerIdAsync(User);
            var model = _playerLogic.GetHomePageModel(playerId);
            return View(model);
        }

        public async Task<IActionResult> PlayerDetailsChartData(byte interval, byte dataKind)
        {
            var playerId = await _userManager.GetPlayerIdAsync(User);
            if (!playerId.HasValue) return NotFound();
            var model = _playerLogic.GetPlayerDetailsChartData(playerId.Value, interval, dataKind);
            return Json(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult RegistrationSuccess()
        {
            return View();
        }

        public IActionResult Acknowledgements()
        {
            return View();
        }
    }
}
