using System.Collections.Generic;
using TauManager.Models;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class UserListViewModel
    {
        public IEnumerable<UserViewModel> Users { get; set; }
        public IDictionary<string, bool> Roles { get; set; }
        public IEnumerable<Player> Players { get; set; }
    }
}