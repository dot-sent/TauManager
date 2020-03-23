namespace TauManager.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class MarketAdBundleItem
    {
        public int Id { get; set; }
        public int BundleId { get; set; }
        public virtual MarketAdBundle Bundle { get; set; }
        public int ItemId { get; set; }
        public virtual Item Item { get; set; }
        public int Quantity { get; set; }
    }
}