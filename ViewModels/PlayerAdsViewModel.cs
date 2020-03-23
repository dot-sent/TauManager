using System.Collections.Generic;
using TauManager.Models;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class PlayerAdsViewModel
    {
        public IEnumerable<MarketAd> Ads { get; set; }
        public string Messages { get; set; }
    }
}