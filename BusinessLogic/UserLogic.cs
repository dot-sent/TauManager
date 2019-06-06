using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TauManager.Areas.Identity.Data;
using TauManager.ViewModels;

namespace TauManager.BusinessLogic
{
    public class UserLogic: IUserLogic
    {
        private ApplicationIdentityUserManager _userManager { get; set; }
        private RoleManager<IdentityRole> _roleManager { get; set; }
        private TauDbContext _dbContext { get; set; }
        
        public UserLogic(ApplicationIdentityUserManager userManager, RoleManager<IdentityRole> roleManager, TauDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        public async Task<UserListViewModel> GetUserList(ClaimsPrincipal currentUser)
        {
            var playerList = _dbContext.Player.OrderBy(p => p.Name).AsEnumerable();
            var roleList = _roleManager.Roles.ToDictionary(r => r.Name, r => ApplicationRoleManager.CanEdit(currentUser, r.Name));
            var userList = _userManager.Users
                .OrderBy(u => u.UserName)
                .Select(u => new UserViewModel
                    {
                        User = u,
                        Player = null, // have to select it further down the road
                        Roles = roleList.ToDictionary(r => r.Key, r => false), // We need to populate the roles later since ET lambdas do not support async ops
                        AllowActivation = false, // Same as above
                    }
                ).ToArray();

            for (var i = 0; i< userList.Count(); i++)
            {
                userList[i].Player = playerList.SingleOrDefault(p => p.Id == userList[i].User.PlayerId);
                var userRoles = await _userManager.GetRolesAsync(userList[i].User);
                foreach(var role in roleList.Keys)
                {
                    userList[i].Roles[role] = userRoles.Contains(role);
                }
                var maxRole = userRoles.Contains(ApplicationRoleManager.Administrator) ? ApplicationRoleManager.Administrator :
                    userRoles.Contains(ApplicationRoleManager.Leader) ? ApplicationRoleManager.Leader :
                    userRoles.Contains(ApplicationRoleManager.Officer) ? ApplicationRoleManager.Officer : "";
                userList[i].AllowActivation = userList[i].User.UserName == currentUser.Identity.Name ? false : ApplicationRoleManager.CanActivate(currentUser, maxRole);
            }
            return new UserListViewModel{
                Roles = roleList,
                Users = userList,
                Players = playerList,
            };
        }

        public async Task<bool> SetUserRole(ClaimsPrincipal currentUser, string userId, string roleName, bool status)
        {
            if (!ApplicationRoleManager.CanEdit(currentUser, roleName)) return false;

            var user = _userManager.Users.SingleOrDefault(u => u.Id == userId);
            if (user == null) return false;

            if (user.UserName == currentUser.Identity.Name && roleName == ApplicationRoleManager.Administrator) return false;

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists) return false;

            if (status) 
            {
                var result = await _userManager.AddToRoleAsync(user, roleName);
                if (!result.Succeeded) return false;
            } else {
                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                if (!result.Succeeded) return false;
            }
            return true;
        }

        public async Task<bool> SetUserPlayerAssociation(string userId, int playerId)
        {
            var user = _userManager.Users.SingleOrDefault(u => u.Id == userId);
            if (user == null) return false;
            if (playerId > 0)
            {
                var playerExists = _dbContext.Player.Any(p => p.Id == playerId);
                if (!playerExists) return false;
            }
            user.PlayerId = playerId;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return false;
            return true;
        }

        public async Task<bool> SetUserActive(ClaimsPrincipal currentUser, string userId, bool status)
        {
            var user = _userManager.Users.SingleOrDefault(u => u.Id == userId);
            if (user == null) return false;
            if (user.UserName == currentUser.Identity.Name) return false;
            
            var roles = await _userManager.GetRolesAsync(user);
            var maxRole = roles.Contains(ApplicationRoleManager.Administrator) ? ApplicationRoleManager.Administrator :
                roles.Contains(ApplicationRoleManager.Leader) ? ApplicationRoleManager.Leader :
                roles.Contains(ApplicationRoleManager.Officer) ? ApplicationRoleManager.Officer : "";
            if (!ApplicationRoleManager.CanActivate(currentUser, maxRole)) return false;
            user.IsApproved = status;
            await _userManager.UpdateAsync(user);
            return true;
        }
    }
}