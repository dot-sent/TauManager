using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TauManager.Areas.Identity.Data;
using TauManager.BusinessLogic;
using TauManager.Models;

namespace TauManager.Controllers
{
    public abstract class SyndicateControllerBase: Controller
    {
        protected ISyndicateLogic _syndicateLogic { get; set; }
        protected ApplicationIdentityUserManager _userManager { get; set; }

        public SyndicateControllerBase(ISyndicateLogic syndicateLogic, ApplicationIdentityUserManager userManager)
        {
            _syndicateLogic = syndicateLogic;
            _userManager = userManager;
        }
        protected async Task<Syndicate> GetSyndicate()
        {
            Syndicate _syndicate = null;
            var syndicateOverride = await _userManager.GetSyndicateOverrideAsync(User);
            if (syndicateOverride.HasValue) {
                _syndicate = _syndicateLogic.GetSyndicateById(syndicateOverride.Value);
            } else {
                var playerId = await _userManager.GetPlayerIdAsync(User);
                _syndicate = _syndicateLogic.GetSyndicateByPlayerId(playerId.HasValue ? playerId.Value : 0);
            }
            return _syndicate;
        }

        protected async Task<Syndicate> GetNativeSyndicate()
        {
            Syndicate _syndicate = null;
            var playerId = await _userManager.GetPlayerIdAsync(User);
            _syndicate = _syndicateLogic.GetSyndicateByPlayerId(playerId.HasValue ? playerId.Value : 0);
            return _syndicate;
        }
    }
}