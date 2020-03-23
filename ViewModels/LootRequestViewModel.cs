namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class LootRequestViewModel
    {
        public enum LootRequestStatus : byte { Interested, Awarded, SpecialOffer };
        public int Id { get; set; }
        public LootRequestStatus Status { get; set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int PlayerPosition { get; set; }
        public int ActivePlayerPosition { get; set; }
        public string SpecialOfferDescription { get; set; }
        public bool AttendedCampaign { get; set; }
    }
}