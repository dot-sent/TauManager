using System;

namespace TauManager.Models
{
    public enum NotificationKind: byte { GauleVisa, University, NewCampaign, CampaignUpdated, CampaignSoon, NewMarketAd }

    public class Notification
    {
        public const int MaxRetries = 3;
        public enum NotificationStatus: byte { NotSent, Sent, PermanentlyFailed }
        public long Id { get; set; }
        public DateTime SendAfter { get; set; }
        public int RecipientId { get; set; }
        public virtual Player Recipient { get; set; }
        public NotificationStatus Status { get; set; }
        public long? RelatedId { get; set; }
        public byte RetryCount { get; set; }
        public NotificationKind Kind { get; set; }
        public string Audit { get; set; }
    }
}