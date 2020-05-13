using TauManager.Areas.Identity.Data;
using TauManager.BusinessLogic;

namespace TauManager.Controllers
{
    public class SettingsController: SyndicateControllerBase
    {
        private IPlayerLogic _playerLogic { get; set; }
        public SettingsController(IPlayerLogic playerLogic, ApplicationIdentityUserManager userManager, ISyndicateLogic syndicateLogic): base(syndicateLogic, userManager)
        {
            _playerLogic = playerLogic;
        }

        
    }
}