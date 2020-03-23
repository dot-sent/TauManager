using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using TauManager.Areas.Identity.Data;
using TauManager.Models;
using System.Threading.Tasks;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public class SyndicateLogic : ISyndicateLogic
    {
        private TauDbContext _dbContext { get; set; }
        private ApplicationIdentityUserManager _userManager { get; set; }
        public SyndicateLogic(TauDbContext dbContext, ApplicationIdentityUserManager userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public Syndicate GetSyndicateByPlayerId(int playerId)
        {
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == playerId);
               if (player == null) return null;
            return player.Syndicate;
        }

        public Syndicate GetSyndicateById(int id)
        {
            return _dbContext.Syndicate.SingleOrDefault(s => s.Id == id);
        }

        public async Task<SyndicateListViewModel> GetAllSyndicates(ClaimsPrincipal currentUser)
        {
            var userSyndicate = await _userManager.GetSyndicateOverrideAsync(currentUser);
            var result = _dbContext.Syndicate.ToDictionary(s => s.Id, s => s.Tag);

            var userPlayerId = await _userManager.GetPlayerIdAsync(currentUser);
            var userPlayer = _dbContext.Player.SingleOrDefault(p => p.Id == userPlayerId);

            return new SyndicateListViewModel{
                AllSyndicates = result,
                CurrentOverride = userSyndicate.HasValue ? 
                    new KeyValuePair<int, string>(userSyndicate.Value, result[userSyndicate.Value]) :
                    new KeyValuePair<int, string>(0, "default"),
                DefaultSyndicate = userPlayer == null || userPlayer.Syndicate == null ?
                    new KeyValuePair<int, string>(0, "not set") :
                    new KeyValuePair<int, string>(userPlayer.SyndicateId.Value, userPlayer.Syndicate.Tag),
            };
        }

        public SyndicatePlayerViewModel GetSyndicatePlayerAssignment()
        {
            var result = new SyndicatePlayerViewModel();

            result.AllSyndicates = _dbContext.Syndicate.AsEnumerable();
            result.SyndicatePlayers = _dbContext.Player
                .AsEnumerable()
                .GroupBy(p => p.SyndicateId)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => g.Key.HasValue ? g.Key.Value : 0,
                    g => g.OrderBy(p => p.Name).AsEnumerable()
                );
            return result;
        }

        public async Task<bool> SetPlayerSyndicate(int playerId, int? syndicateId)
        {
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == playerId);
            if (player == null) return false;
            var syndicate = _dbContext.Syndicate.SingleOrDefault(s => s.Id == syndicateId);
            if (syndicateId.HasValue && syndicate == null) return false;

            player.SyndicateId = syndicateId;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public Player GetPlayerById(int? id)
        {
            return id.HasValue ? _dbContext.Player.SingleOrDefault(p => p.Id == id.Value) : null;
        }
    }
}