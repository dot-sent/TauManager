using System.Security.Claims;
using TauManager.ViewModels;
using System.Threading.Tasks;

namespace TauManager.BusinessLogic
{
    public interface IUserLogic
    {
        Task<UserListViewModel> GetUserList(ClaimsPrincipal currentUser);
        Task<bool> SetUserRole(ClaimsPrincipal currentUser, string userId, string roleName, bool status);
        Task<bool> SetUserPlayerAssociation(string userId, int playerId);
        Task<bool> SetUserActive(ClaimsPrincipal currentUser, string userId, bool status);
    }
}