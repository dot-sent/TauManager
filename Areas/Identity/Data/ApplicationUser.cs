using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace TauManager.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class ApplicationUser : IdentityUser
    {
      public Boolean IsApproved { get; set; }
      public int PlayerId { get; set; }
      public int? SyndicateOverride { get; set; }
      public string PlayerPageUploadToken { get; set; }
      public byte? MarketView { get; set; }
      public byte? MarketSort { get; set; }
      public bool? MarketIsViewPinned { get; set; }
    }
}