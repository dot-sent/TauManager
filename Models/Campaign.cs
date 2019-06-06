using System.Linq;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace TauManager.Models
{
    public class Campaign
    {
        public enum CampaignStatus : byte { Unknown, Assigned, Planned, InProgress, Abandoned, Completed, Failed, Skipped };
        public enum CampaignDifficulty : byte { Easy, Normal, Hard, Extreme };
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? UTCDateTime { get; set; }
        public string UTCDateString
        {
            get
            {
                return UTCDateTime.HasValue ? UTCDateTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Not set";
            }
        } 
        public string GCTDateString
        {
            get
            {
                return UTCDateTime.HasValue ? 
                    (UTCDateTime.Value.Subtract(new DateTime(1964, 01, 22)).Days/100.0).ToString("0.00'/'") +
                    (UTCDateTime.Value.TimeOfDay.TotalDays*100).ToString("00.000").Replace('.', ':') : "Not set";
            }
        }
        public int? ManagerId { get; set; }
        public virtual Player Manager { get; set; }
        [DefaultValue(CampaignStatus.Unknown)]
        public CampaignStatus Status { get; set; }
        public string Station { get; set; }
        public CampaignDifficulty? Difficulty { get; set; }
        public int? Tiers { get; set; }
        public string TiersString 
        { 
            get
            {
                if (!Tiers.HasValue)
                {
                    return "Not set";
                }
                return String.Join(',', TiersList.Select(t => t.ToString()));
            } 
        }
        public List<int> TiersList 
        { 
            get
            {
                var result = new List<int>();
                if (Tiers.HasValue)
                {
                    var tiers_temp = Tiers.Value;
                    for (var tier = 5; tier > 0; tier--)
                    {
                        if (tiers_temp >= Math.Pow(2, tier-1))
                        {
                            result.Insert(0, tier);
                            tiers_temp -= (int)Math.Round(Math.Pow(2, tier-1));
                        }
                    }
                }
                return result;
            }
        }
        public string Comments { get; set; }
        public virtual IEnumerable<CampaignSignup> Signups { get; set; }
        public Dictionary<int, int> SignupsDict 
        {
             get
             {
                 if (Signups == null) return new Dictionary<int, int>();
                 return Signups.ToDictionary(s => s.PlayerId, s => 1);
             }
        }
        public virtual IEnumerable<CampaignAttendance> Attendance { get; set; }
        public Dictionary<int, int> AttendanceDict 
        {
             get
             {
                 if (Attendance == null) return new Dictionary<int, int>();
                 return Attendance.ToDictionary(s => s.PlayerId, s => 1);
             }
        }
        public virtual IEnumerable<CampaignLoot> Loot { get; set; }
    }
}