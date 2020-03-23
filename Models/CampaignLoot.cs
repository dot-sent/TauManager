using System.Collections.Generic;
using System.ComponentModel;

namespace TauManager.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class CampaignLoot
    {
        public enum CampaignLootStatus : byte { Undistributed, StaysWithSyndicate, PermanentlyAwarded, OnLoan, Other };
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }
        public int ItemId { get; set; }
        public virtual Item Item { get; set; }
        public int? HolderId { get; set; }
        public virtual Player Holder { get; set; }
        [DefaultValue(CampaignLootStatus.Undistributed)]
        public CampaignLootStatus Status { get; set; }
        public string Comments { get; set; }
        public virtual List<LootRequest> Requests { get; set; }
        [DefaultValue(false)]
        public bool? AvailableToOtherSyndicates { get; set; }
    }
}