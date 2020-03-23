using System;

namespace TauManager.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class PlayerListPositionHistory
    {
        public int Id { get; set; }
        public int? LootRequestId { get; set; }
        public virtual LootRequest LootRequest { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
        public string Comment { get; set; }
    }
}