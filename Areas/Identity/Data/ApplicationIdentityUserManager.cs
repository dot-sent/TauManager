using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace TauManager.Areas.Identity.Data
{
    public class ApplicationIdentityUserManager: UserManager<ApplicationUser>
    {
        public ApplicationIdentityUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, 
            IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {

        }

        public override Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            if (!user.IsApproved )
            {
                return Task.FromResult<bool>(false);
            }
            return base.CheckPasswordAsync(user, password);
        }

        public async Task<int?> GetPlayerIdAsync(ClaimsPrincipal user)
        {
            var appUser = await this.GetUserAsync(user);
            if (appUser == null) return null;
            return appUser.PlayerId;
        }
    }
}