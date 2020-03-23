using System.Collections.Generic;

namespace TauManager.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class MarketAdBundle
    {
        public enum BundleType : byte { Offer, Request };
        public int Id { get; set; }
        public decimal Credits { get; set; }
        public int AdId { get; set; }
        public virtual MarketAd Ad { get; set; }
        public BundleType Type { get; set; }
        public virtual IEnumerable<MarketAdBundleItem> Items { get; set; }
    }
}