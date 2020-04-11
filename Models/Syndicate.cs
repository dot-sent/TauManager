using System.Collections.Generic;

namespace TauManager.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Syndicate
    {
        public int Id { get; set; }
        public string Tag { get; set; }
        public virtual ICollection<SyndicateHistory> History { get; set; }
    }
}