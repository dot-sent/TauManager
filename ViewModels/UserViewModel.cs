using System.Collections.Generic;
using TauManager.Areas.Identity.Data;
using TauManager.Models;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class UserViewModel
    {
        public ApplicationUser User { get; set; }
        public IDictionary<string, bool> Roles { get; set; }
        public Player Player { get; set; }
        public bool AllowActivation { get; set; }
    }
}