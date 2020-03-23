namespace TauManager.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class SkillGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SkillId { get; set; }
        public virtual Skill Skill { get; set; }
    }
}