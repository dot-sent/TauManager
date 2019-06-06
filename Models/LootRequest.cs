namespace TauManager.Models
{
    public class LootRequest
    {
        public enum LootRequestStatus : byte { Interested, Awarded, SpecialOffer };
        public int Id { get; set; }
        public LootRequestStatus Status { get; set; }
        public int RequestedForId { get; set; }
        public virtual Player RequestedFor { get; set; }
        public int RequestedById { get; set; }
        public virtual Player RequestedBy { get; set; }
        public int LootId { get; set; }
        public virtual CampaignLoot Loot { get; set; }
        public string SpecialOfferDescription { get; set; }
        public virtual PlayerListPositionHistory HistoryEntry { get; set; }
    }
}