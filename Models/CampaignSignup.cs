using System;
using System.ComponentModel.DataAnnotations;
namespace TauManager.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class CampaignSignup
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }
        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
        public bool? Attending { get; set; }
    }
}