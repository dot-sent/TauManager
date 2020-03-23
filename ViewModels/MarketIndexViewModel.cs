using System.Collections.Generic;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class MarketIndexViewModel
    {
        public int OwnActiveAds { get; set; }
        public int OwnInactiveAds { get; set; }
        public int OwnAdReactions { get; set; }
        public IEnumerable<MarketAdViewModel> Ads { get; set; }
        public IEnumerable<MarketAdViewModel> OfferAds { get; set; }
        public IEnumerable<MarketAdViewModel> AskAds { get; set; }
        public Dictionary<int, string> ItemTypes { get; set; }
        public Dictionary<int, string> WeaponTypes { get; set; }
        public MarketIndexParamsViewModel Filters { get; set; }
    }
}