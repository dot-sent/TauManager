namespace TauManager.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class PlayerSkill
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int SkillId { get; set; }
        public int SkillLevel { get; set; }
        public virtual Player Player { get; set; }
        public virtual Skill Skill { get; set; }
    }
}