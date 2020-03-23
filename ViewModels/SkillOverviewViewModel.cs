using System.Collections.Generic;
using TauManager.Models;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class SkillOverviewViewModel
    {
        public IEnumerable<Player> Players { get; set; }
        public IEnumerable<Skill> Skills { get; set; }
        public IDictionary<int, Dictionary<int, int>> SkillValues { get; set; }
        public IEnumerable<string> AllSkillGroups { get; set; }
        public string SkillGroupName { get; set; }
    }
}