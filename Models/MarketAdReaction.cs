using System;

namespace TauManager.Models
{
    /*
        This class is likely to be refactored in the future and 
        transformed into generic notification class. I, however,
        don't want to optimize prematurely, which is why I decided
        to make it a separate class for now.
     */
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class MarketAdReaction
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public virtual MarketAd Ad { get; set; }
        public int InterestedId { get; set; }
        public virtual Player Interested { get; set; }
        public DateTime ReactionDate { get; set; }
        public string Message { get; set; }
    }
}