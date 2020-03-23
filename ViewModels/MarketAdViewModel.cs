using System;
using System.Linq;
using TauManager.Models;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class MarketAdViewModel
    {
        public int Id { get; set; }
        public string AdType { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public string AskingPrice { get; set; }
        public string OfferString { get; set; }
        public string RequestString { get; set; }
        public string AuthorName { get; set; }
        public string Description { get; set; }
        public DateTime PlacementDate { get; set; }
        public Item Item { get; set; }
        public MarketAdViewModel(MarketAd ad)
        {
            this.Id = ad.Id;
            this.AdType = ad.Type.ToString();
            this.Description = ad.Description;
            this.AuthorName = ad.Author.Name;
            var offerBundle = ad.Bundles.FirstOrDefault(b =>
                (ad.Type == MarketAd.AdType.Sell || ad.Type == MarketAd.AdType.Lend) ?
                b.Type == MarketAdBundle.BundleType.Offer : 
                b.Type == MarketAdBundle.BundleType.Request);
            this.ImageUrl = offerBundle == null ? "#" : offerBundle.Items.Count() > 0 ? offerBundle.Items.FirstOrDefault().Item.ImageUrl : "#";
            this.Item = offerBundle == null || offerBundle.Items.Count() == 0 ? null : offerBundle.Items.FirstOrDefault().Item;
            this.AskingPrice = (ad.Type == MarketAd.AdType.Sell || ad.Type == MarketAd.AdType.Lend) ?
                ad.RequestString : ad.OfferString;
            this.Caption = (ad.Type == MarketAd.AdType.Sell || ad.Type == MarketAd.AdType.Lend) ?
                ad.OfferString : ad.RequestString;
            this.PlacementDate = ad.PlacementDate;
            this.OfferString = ad.OfferString;
            this.RequestString = ad.RequestString;
        }
    }
}