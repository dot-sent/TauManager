using Microsoft.AspNetCore.Authorization;

namespace TauManager.Areas.Identity
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        public AuthorizeRolesAttribute(params string[] roles) : base()
        {
            Roles = string.Join(",", roles);
        }
    }
}