using System;
using System.ComponentModel.DataAnnotations;

namespace TauManager.Models
{
    public class SyndicateHistory
    {
        public long Id { get; set; }
        public int SyndicateId { get; set; }
        public virtual Syndicate Syndicate { get; set; }
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal Level { get; set; }
        public int Bonds { get; set; }
        public decimal Credits { get; set; }
        public int MembersCount { get; set; }
        public DateTime RecordedAt { get; set; }

        public SyndicateHistory() { }
        public SyndicateHistory(ViewModels.SyndicateInfoViewModel model)
        {
            this.Level = model.Level;
            this.Bonds = model.Bonds;
            this.Credits = model.Credits;
            this.MembersCount = model.MembersCount;
        }
    }
}