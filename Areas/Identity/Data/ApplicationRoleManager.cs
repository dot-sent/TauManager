using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace TauManager.Areas.Identity.Data
{
    public class ApplicationRoleManager: RoleManager<IdentityRole>
    {
        public ApplicationRoleManager(IRoleStore<IdentityRole> store, 
            IEnumerable<IRoleValidator<IdentityRole>> validators,
            ILookupNormalizer lookupNormalizer,
            IdentityErrorDescriber errorDescriber,
            ILogger<RoleManager<IdentityRole>> logger
        ) : base (store, validators, lookupNormalizer, errorDescriber, logger)
        {
            
        }
        public const string Administrator = "Administrator";
        public const string Leader = "Leader";
        public const string Officer = "Officer";
        public static string[] AllRoles 
        {
            get 
            {
                string[] result = { Administrator, Leader, Officer }; 
                return result;
            }
        }
        public static bool CanEdit(ClaimsPrincipal user, string role)
        {
            return user.IsInRole(Administrator) ? true : user.IsInRole(Leader) && (role == Officer) ? true : false;
        }

        public static bool CanActivate(ClaimsPrincipal user, string role)
        {
            return user.IsInRole(Administrator) ? true : user.IsInRole(Leader) && (role == Officer) ? true : user.IsInRole(Officer) && role == "" ? true : false;
        }
    }
}