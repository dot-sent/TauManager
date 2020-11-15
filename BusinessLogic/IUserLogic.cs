using System.Security.Claims;
using TauManager.ViewModels;
using System.Threading.Tasks;
using TauManager.Models;

namespace TauManager.BusinessLogic
{
    public interface IUserLogic
    {
        Task<UserListViewModel> GetUserList(ClaimsPrincipal currentUser, Syndicate syndicate);
        Task<bool> SetUserRole(ClaimsPrincipal currentUser, string userId, string roleName, bool status);
        Task<bool> SetUserPlayerAssociation(ClaimsPrincipal currentUser, string userId, int playerId);
        Task<bool> SetUserActive(ClaimsPrincipal currentUser, string userId, bool status);
        Task<bool> SetSyndicateOverride(ClaimsPrincipal currentUser, int? syndicateId);
        Task<string> ResetPassword(string userId);
        Task<bool> SetThemeOverride(ClaimsPrincipal currentUser, UserCSSTheme themeId);
    }
}