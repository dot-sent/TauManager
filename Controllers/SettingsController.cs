using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TauManager.Areas.Identity.Data;
using TauManager.BusinessLogic;
using TauManager.Models;

namespace TauManager.Controllers
{
    [Authorize]
    public class SettingsController: SyndicateControllerBase
    {
        private IPlayerLogic _playerLogic { get; set; }
        public SettingsController(IPlayerLogic playerLogic, ApplicationIdentityUserManager userManager, ISyndicateLogic syndicateLogic): base(syndicateLogic, userManager)
        {
            _playerLogic = playerLogic;
        }

        public async Task<IActionResult> ConnectDiscordAccount(string login)
        {
            var result = await _playerLogic.SetPlayerDiscordAccountAsync(await _userManager.GetPlayerIdAsync(User), login);
            if (result) return View("SingleMessage", new Announcement{
                Style = Announcement.AnnouncementStyle.Success,
                Text = "Discord account " + login + " successfully linked."
            });
            return View("SingleMessage", new Announcement{
                Style = Announcement.AnnouncementStyle.Danger,
                Text = "Failed to link Discord account " + login + " - please contact Dotsent"
            });
        }
    }
}