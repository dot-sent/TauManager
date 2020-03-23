using System.Security.Cryptography;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TauManager.Areas.Identity.Data;
using TauManager.ViewModels;
using TauManager.Models;
using System;

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

        public async Task<UserListViewModel> GetUserList(ClaimsPrincipal currentUser, Syndicate syndicate)
        {
            var loadAll = currentUser.IsInRole(ApplicationRoleManager.MultiSyndicate);
            if (!loadAll && syndicate == null) return new UserListViewModel{
                Players = new List<Player>(),
                Roles = new Dictionary<string, bool>(),
                Users = new List<UserViewModel>(),
            };
            
            var playerList = _dbContext.Player.Where(p => loadAll || p.SyndicateId == syndicate.Id).OrderBy(p => p.Name).AsEnumerable();
            var roleList = _roleManager.Roles.ToDictionary(r => r.Name, r => ApplicationRoleManager.CanEdit(currentUser, r.Name));
            var userList = _userManager.Users
                .AsEnumerable() //TODO: !BAD! for performance - think about the way to rewrite it
                .Where(
                    u => loadAll || playerList.Any(p => p.Id == u.PlayerId) || u.SyndicateOverride == syndicate.Id
                )
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

            if (!currentUser.IsInRole(ApplicationRoleManager.MultiSyndicate) && 
                await _userManager.IsInRoleAsync(user, ApplicationRoleManager.MultiSyndicate)) return false;

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

        public async Task<bool> SetUserPlayerAssociation(ClaimsPrincipal currentUser, string userId, int playerId)
        {
            var user = _userManager.Users.SingleOrDefault(u => u.Id == userId);
            if (user == null) return false;
            var player = _dbContext.Player.SingleOrDefault(p => p.Id == playerId);
            if (playerId > 0 && player == null) return false;
            
            // We only check for syndicate correctness if current user is not MultiSyndicate user
            if (!currentUser.IsInRole(ApplicationRoleManager.MultiSyndicate)) 
            {
                var currentUserSyndicate = await _userManager.GetSyndicateOverrideAsync(currentUser);
                if (!currentUserSyndicate.HasValue)
                {
                    var currentUserPlayerId = await _userManager.GetPlayerIdAsync(currentUser);
                    if (currentUserPlayerId.HasValue)
                    {
                        var currentUserPlayer = _dbContext.Player.SingleOrDefault(p => p.Id == currentUserPlayerId);
                        if (currentUserPlayer != null)
                        {
                            currentUserSyndicate = currentUserPlayer.SyndicateId;
                        }
                    }
                }
                var userSyndicate = user.SyndicateOverride;
                if (!userSyndicate.HasValue)
                {
                    var userPlayer = _dbContext.Player.SingleOrDefault(p => p.Id == user.PlayerId);
                    if (userPlayer != null)
                    {
                        userSyndicate = userPlayer.SyndicateId;
                    }
                }
                if (currentUserSyndicate != player.SyndicateId ||
                    userSyndicate != player.SyndicateId ||
                    currentUserSyndicate != userSyndicate) return false;
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

            if (!currentUser.IsInRole(ApplicationRoleManager.MultiSyndicate)) 
            {
                var currentUserSyndicate = await _userManager.GetSyndicateOverrideAsync(currentUser);
                if (!currentUserSyndicate.HasValue)
                {
                    var currentUserPlayerId = await _userManager.GetPlayerIdAsync(currentUser);
                    if (currentUserPlayerId.HasValue)
                    {
                        var currentUserPlayer = _dbContext.Player.SingleOrDefault(p => p.Id == currentUserPlayerId);
                        if (currentUserPlayer != null)
                        {
                            currentUserSyndicate = currentUserPlayer.SyndicateId;
                        }
                    }
                }
                var userSyndicate = user.SyndicateOverride;
                if (!userSyndicate.HasValue)
                {
                    var userPlayer = _dbContext.Player.SingleOrDefault(p => p.Id == user.PlayerId);
                    if (userPlayer != null)
                    {
                        userSyndicate = userPlayer.SyndicateId;
                    }
                }
                if (currentUserSyndicate != userSyndicate) return false;
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            var maxRole = roles.Contains(ApplicationRoleManager.Administrator) ? ApplicationRoleManager.Administrator :
                roles.Contains(ApplicationRoleManager.Leader) ? ApplicationRoleManager.Leader :
                roles.Contains(ApplicationRoleManager.Officer) ? ApplicationRoleManager.Officer : "";
            if (!ApplicationRoleManager.CanActivate(currentUser, maxRole)) return false;
            user.IsApproved = status;
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<bool> SetSyndicateOverride(ClaimsPrincipal currentUser, int? syndicateId)
        {
            if (!currentUser.IsInRole(ApplicationRoleManager.MultiSyndicate)) return false;
            var user = await _userManager.GetUserAsync(currentUser);
            user.SyndicateOverride = syndicateId;
            await _userManager.UpdateAsync(user);
            return true;
        }

        public async Task<string> ResetPassword(string userId)
        {
            var user = _userManager.Users.SingleOrDefault(u => u.Id == userId);
            if (user == null) return null;
            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var newPassword = GenerateRandomPassword(_userManager.Options.Password);
            var passwordChangeResult = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            if (passwordChangeResult.Succeeded) 
            {
                return newPassword;
            }
            return null;
        }

        /// <summary>
        /// Generates a Random Password
        /// respecting the given strength requirements.
        ///
        /// This probably won't pass any serious security audit,
        /// but good enough for my purposes.
        ///
        /// Credits: https://stackoverflow.com/a/46229180/625594,
        /// although I changed it to use different and hopefully
        /// more secure generation method.
        /// </summary>
        /// <param name="opts">A valid PasswordOptions object
        /// containing the password strength requirements.</param>
        /// <returns>A random password</returns>
        private string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            // The length of this string is 64 chars on purpose - 
            // it MUST be one of the integer divisors for 256.
            string randomChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$-_"; 
            var charClasses = new[] {
                "ABCDEFGHJKLMNPQRSTUVWXYZ",
                "abcdefghijkmnopqrstuvwxyz",
                "0123456789",
                "!@$-_",
            };
            RandomNumberGenerator r = RandomNumberGenerator.Create();
            byte[] randomBytes = new Byte[opts.RequiredLength*2];
            r.GetBytes(randomBytes);

            var resultString = "";

            while (resultString.Length < opts.RequiredLength &&
                (!opts.RequireUppercase || !resultString.AsEnumerable().Any(c => charClasses[0].Contains(c))) &&
                (!opts.RequireLowercase || !resultString.AsEnumerable().Any(c => charClasses[1].Contains(c))) &&
                (!opts.RequireDigit || !resultString.AsEnumerable().Any(c => charClasses[2].Contains(c))) &&
                (!opts.RequireNonAlphanumeric || !resultString.AsEnumerable().Any(c => charClasses[3].Contains(c))) && 
                (resultString.AsEnumerable().GroupBy(c => c).Select(g => new { Char = g.Key, Count = g.Count()}).Count() < opts.RequiredUniqueChars) )
            {
                List<char> chars = new List<char>();
                foreach (var b in randomBytes)
                {
                    chars.Add(randomChars[b % randomChars.Length]);
                }
                resultString = new string(chars.ToArray());
            }

            return resultString;
        }
    }
}