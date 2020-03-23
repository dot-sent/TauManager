using System.Collections.Generic;
namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

    public class SyndicateListViewModel
    {
        public Dictionary<int, string> AllSyndicates { get; set; }
        public KeyValuePair<int, string> CurrentOverride { get; set; }
        public KeyValuePair<int, string> DefaultSyndicate { get; set; }
    }
}