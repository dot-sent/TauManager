using System.Security.Claims;
using TauManager.Models;
using System.Threading.Tasks;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public interface ISyndicateLogic
    {
        Syndicate GetSyndicateByPlayerId(int playerId);
        Syndicate GetSyndicateById(int id);
        Task<SyndicateListViewModel> GetAllSyndicates(ClaimsPrincipal currentUser);
        SyndicatePlayerViewModel GetSyndicatePlayerAssignment();
        Task<bool> SetPlayerSyndicate(int playerId, int? syndicateId);
        Task<bool> SubmitSyndicateHistory(SyndicateInfoViewModel historyEntry);
    }
}