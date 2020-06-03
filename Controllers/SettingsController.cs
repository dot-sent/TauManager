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

        public async Task<IActionResult> ConnectDiscordAccount(string login, string authCode)
        {
            var result = await _playerLogic.SetPlayerDiscordAccountAsync(await _userManager.GetPlayerIdAsync(User), login, authCode);
            if (result)
            {
                return View("SingleMessage", new Announcement{
                    Style = Announcement.AnnouncementStyle.Success,
                    Text = "Discord account " + login + " successfully linked."
                });
            }
            return View("SingleMessage", new Announcement{
                Style = Announcement.AnnouncementStyle.Danger,
                Text = "Failed to link Discord account " + login + " - please contact Dotsent"
            });
        }

        [HttpGet]
        public async Task<IActionResult> Discord()
        {
            var playerId = await _userManager.GetPlayerIdAsync(User);
            if (!playerId.HasValue) return NotFound();
            var player = _playerLogic.GetPlayerById(playerId.Value);
            return View(player);
        }

        [HttpPost]
        public async Task<IActionResult> Discord(int notificationSettings)
        {
            var result =  _playerLogic.SetPlayerNotificationFlags(await _userManager.GetPlayerIdAsync(User), notificationSettings);
            if (!result)
            {
                return View("SingleMessage", new Announcement{
                    Style = Announcement.AnnouncementStyle.Danger,
                    Text = "Failed to update notifcation settings - please contact Dotsent"
                });
            }
            return View("SingleMessage", new Announcement{
                Style = Announcement.AnnouncementStyle.Success,
                Text = "Notification settings saved successfully."
            });
        }

        [HttpGet]
        public async Task<IActionResult> DiscordDisconnect()
        {
            var result = await _playerLogic.DisconnectDiscordAccount(await _userManager.GetPlayerIdAsync(User));
            if (!result)
            {
                return View("SingleMessage", new Announcement{
                    Style = Announcement.AnnouncementStyle.Danger,
                    Text = "Failed to disconnect Discord account - maybe, it was not connected? If this problem persists, please contact Dotsent."
                });
            }
            return View("SingleMessage", new Announcement{
                Style = Announcement.AnnouncementStyle.Success,
                Text = "Discord account disconnected successfully. Please use !connect <player_name> bot command to connect Discord account again."
            });
        }
    }
}