using System.ComponentModel;
using System.Globalization;
using System.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TauManager.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [DefaultValue(true)]
        public bool Active { get; set; }
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal Level { get; set; }
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal Strength { get; set; }
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal Stamina { get; set; }
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal Agility { get; set; }
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal Intelligence { get; set; }
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal Social { get; set; }
        public decimal Wallet { get; set; }
        [DisplayFormat(DataFormatString = "{0:###,###,##0.00}")]
        public decimal Bank { get; set; }
        public string BankString
        {
            get
            {
                return Bank.ToString("N2",new CultureInfo("en-US"));
            }
        }
        public int Bonds { get; set; }
        public DateTime LastUpdate { get; set; }
        public string LastUpdateString
        {
            get
            {
                var diff = DateTime.Now - LastUpdate;
                if (diff.TotalDays < 1) return "Today";
                if (diff.TotalDays < 2) return "Yesterday";
                if (diff.TotalDays < 7) return "Last week";
                return LastUpdate.ToString("yyyy-MM-dd");
            }
        }
        public virtual ICollection<PlayerHistory> History { get; set; }

        public void Update(PlayerHistory p)
        {
            LastUpdate = p.RecordedAt;
            Level = p.Level;
            Strength = p.Strength;
            Stamina = p.Stamina;
            Agility = p.Agility;
            Intelligence = p.Intelligence;
            Social = p.Social;
            Wallet = p.Wallet;
            Bank = p.Bank;
            Bonds = p.Bonds;
        }

        public int Tier 
        { 
            get
            {
                return 1 + (int)Math.Floor((Level-1) / 5);
            } 
        }

        [DisplayFormat(DataFormatString = "{0:N3}")]
        public decimal StatTotal 
        {
            get
            {
                return Strength + Stamina + Agility;
            }
        }

        public virtual IEnumerable<CampaignAttendance> Attendance { get; set; }
    }
}