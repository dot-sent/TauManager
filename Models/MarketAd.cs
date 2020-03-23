using System.Linq;
using System.Runtime.Serialization;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TauManager.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class MarketAd
    {
        public enum AdType : byte { Sell, Buy, Lend, Borrow };
        public enum TransactionElementType : byte { Specific, Nothing, Bid };
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public virtual Player Author { get; set; }
        public AdType Type { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime PlacementDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Description { get; set; }
        [DefaultValue(true)]
        public bool Active { get; set; }
        public TransactionElementType OfferType { get; set; }
        public TransactionElementType RequestType { get; set; }
        public virtual IEnumerable<MarketAdBundle> Bundles { get; set; }
        public virtual IEnumerable<MarketAdReaction> Reactions { get; set; }
        [NotMapped]
        public IEnumerable<MarketAdBundle> OfferBundles { 
            get 
            {
                return Bundles.Where(b => b.Type == MarketAdBundle.BundleType.Offer);
            }
        }
        [NotMapped]
        public IEnumerable<MarketAdBundle> RequestBundles { 
            get 
            {
                return Bundles.Where(b => b.Type == MarketAdBundle.BundleType.Request);
            }
        }
        public string OfferString
        { 
            get
            {
                return _serializeAdPart(this.OfferType, this.OfferBundles);
            }
        }
        public string RequestString 
        { 
            get
            {
                return _serializeAdPart(this.RequestType, this.RequestBundles);
            }
        }

        private static string _serializeAdPart(TransactionElementType elementType, IEnumerable<MarketAdBundle> bundles)
        {
            switch (elementType) {
                case TransactionElementType.Bid : return "*";
                case TransactionElementType.Nothing : return "free";
                case TransactionElementType.Specific :
                {
                    if (bundles.Count() == 0) return ""; // Shouldn't happen normally

                    var alternatives = new List<string>();

                    foreach (var bundle in bundles)
                    {
                        var components = new List<string>();
                        if (bundle.Credits > 0)
                        {
                            components.Add(string.Format("{0}cr", bundle.Credits));
                        }
                        if (bundle.Items != null)
                        {
                            foreach(var bundleItem in bundle.Items)
                            {
                                if (bundleItem.Quantity > 0)
                                {
                                    if (bundleItem.Quantity == 1) 
                                    {
                                        components.Add(bundleItem.Item.Name);
                                    } else {
                                        components.Add(String.Format("{0} x{1}", bundleItem.Item.Name, bundleItem.Quantity));
                                    }
                                }
                            }
                        }
                        alternatives.Add(String.Join(" and ", components));
                    }
                    return String.Join(" or ", alternatives);
                }
            }
            return "";
        }
    }
}