using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Globalization;
using System.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TauManager.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Player
    {
        [Flags]
        public enum NotificationFlags: int {
            None = 0,
            GauleVisa = 1,
            University = 2,
            NewCampaign = 4,
            CampaignUpdatedIfSignedUp = 8,
            CampaignUpdatedAll = 16,
            CampaignSoonIfSignedUp = 32,
            CampaignSoonAll = 64,
            NewMarketAd = 128,
            All = (GauleVisa | University | NewCampaign |
                CampaignUpdatedIfSignedUp | CampaignUpdatedAll |
                CampaignSoonIfSignedUp | CampaignSoonAll |
                NewMarketAd )
        }

        public static bool IsValidNotificationFlag(int flag) => (flag & (int)NotificationFlags.All) == flag;

        public int Id { get; set; }
        public string Name { get; set; }
        public int? SyndicateId { get; set; }
        public virtual Syndicate Syndicate { get; set; }
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
        public DateTime? UniCourseDate { get; set; }
        public bool UniCourseActive
        { 
            get
            {
                return UniCourseDate.HasValue ?
                    UniCourseDate.Value.Date >= DateTime.Today :
                    false;
            }
        }

        public bool UniversityComplete {
            get
            {
                return this.PlayerSkills.Sum(ps => ps.SkillLevel) >= Constants.MaxUniversityCourses;
            }
        }
        public string UniCourseDateString { 
            get
            {
                if (!UniCourseDate.HasValue) return "No data";
                var diff = DateTime.Today - UniCourseDate.Value;
                if (diff.Days >= 7) return UniCourseDate.Value.ToString("yyyy-MM-dd");
                if (diff.Days > 1 && diff.Days < 7) return "Last week";
                if (diff.Days == 1) return "Yesterday";
                if (diff.Days == 0) return "Due today";
                if (diff.Days == -1) return "Due tomorrow";
                return String.Format("Due in {0} days", 0-diff.Days);
            }
        }
        public DateTime? GauleVisaExpiry { get; set; }
        public bool GauleVisaExpired { 
            get
            {
                return GauleVisaExpiry.HasValue ? 
                    GauleVisaExpiry < DateTime.Now :
                    true;
            }
        }
        public bool GauleVisaExpiring {
            get
            {
                return GauleVisaExpiry.HasValue ? 
                    GauleVisaExpiry <= DateTime.Now.AddDays(3) :
                    false;
            }
        }
        public string GauleVisaExpiryString {
            get
            {
                if (!GauleVisaExpiry.HasValue) return "Player never had a visa";
                var diff = DateTime.Today - GauleVisaExpiry.Value;
                if (diff.Days >= 7) return GauleVisaExpiry.Value.ToString("yyyy-MM-dd");
                if (diff.Days > 1 && diff.Days < 7) return "last week";
                if (diff.Days == 1) return "yesterday";
                if (diff.Days == 0) return "expires today";
                if (diff.Days == -1) return "expires tomorrow";
                return String.Format("expires in {0} days", 0-diff.Days);
            }
        }

        public string DiscordLogin { get; set; }
        public string DiscordAuthCode { get; set; }
        [DefaultValue(false)]
        public bool DiscordAuthConfirmed { get; set; }
        public NotificationFlags NotificationSettings { get; set; }
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

        public string LevelString 
        { 
            get
            {
                var intPart = (int)Math.Floor(Level);
                var fracPart = Level - intPart;
                return intPart.ToString() + "@" + fracPart.ToString("#0.0%");
            }
        }

        public virtual IEnumerable<CampaignAttendance> Attendance { get; set; }
        public virtual IEnumerable<PlayerSkill> PlayerSkills { get; set; }
        [InverseProperty(nameof(LootRequest.RequestedFor))]
        public virtual IEnumerable<LootRequest> LootRequests { get; set; }
        // [InverseProperty("RequestedById")]
        // public virtual IEnumerable<LootRequest> LootRequestsForOthers { get; set; }
        public virtual IEnumerable<CampaignLoot> HeldCampaignLoot { get; set; }
    }
}