using System.Collections.Generic;
using TauManager.Models;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class HomePageViewModel
    {
        public SyndicatePlayersViewModel Metrics { get; set; }
        public IEnumerable<Announcement> Announcements { get; set; }
    }
}