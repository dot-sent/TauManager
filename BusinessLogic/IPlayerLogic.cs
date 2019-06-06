using System.Threading.Tasks;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public interface IPlayerLogic
    {
        SyndicateMetricsViewModel GetSyndicateMetrics(int? playerId);
        Task<string> ParsePlayerPageAsync(string fileContents);
        Task<bool> SetPlayerActiveAsync(int playerId, bool status);
        PlayerDetailsViewModel GetPlayerDetails(int id, bool? loadAll);
    }
}