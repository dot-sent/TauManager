using System.Collections.Generic;
using TauManager.Models;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class SyndicatePlayerViewModel
    {
        public IEnumerable<Syndicate> AllSyndicates { get; set; }
        public Dictionary<int, IEnumerable<Player>> SyndicatePlayers { get; set; }
    }
}