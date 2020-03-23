using System.Collections.Generic;

namespace TauManager.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IEnumerable<PlayerSkill> PlayerRelations { get; set; }
        public virtual SkillGroup Groups { get; set; }
    }
}