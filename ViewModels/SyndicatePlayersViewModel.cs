using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TauManager.Models;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class SyndicatePlayersViewModel
    {
        public class TierStatistics
        {
            public int Tier { get; set; }
            public int PlayerCount { get; set; }
            [DisplayFormat(DataFormatString = "{0:N3}")]
            public decimal Strength { get; set; }
            [DisplayFormat(DataFormatString = "{0:N3}")]
            public decimal Stamina { get; set; }
            [DisplayFormat(DataFormatString = "{0:N3}")]
            public decimal Agility { get; set; }

            [DisplayFormat(DataFormatString = "{0:N3}")]
            public decimal StatTotal 
            {
                get
                {
                    return Strength + Stamina + Agility;
                }
            }
            [DisplayFormat(DataFormatString = "{0:N3}")]
            public double StatTotalMedian { get; set; }
            [DisplayFormat(DataFormatString = "{0:N3}")]
            public double StatTotalStdDev { get; set; }
        }
        public class LastPlayerActivity
        {
            public int DaysAgo { get; set; }
            public string DaysAgoString 
            {
                get
                {
                    if (DaysAgo < 0) return "No data";
                    if (DaysAgo < 1) return "Today";
                    if (DaysAgo < 2) return "Yesterday";
                    if (DaysAgo < 14) return DaysAgo + "d ago";
                    return ">2w ago";
                }
            }
            public bool Active { get; set; }
            public int DaysClass 
            { 
                get
                {
                    if (DaysAgo < 0) return 0;
                    if (DaysAgo < 2) return 1;
                    if (DaysAgo < 14) return 2;
                    return Active ? 3 : 0;
                }
            }
        }
        public Dictionary<int, List<Player>> Players { get; set; }
        public Dictionary<int, TierStatistics> PlayerStats { get; set; }
        public Dictionary<KeyValuePair<int, int>, int> PlayerCountByStatTotal { get; set; }
        public int MaxTier { get; set; }
        public Dictionary<int, LastPlayerActivity> LastActivity { get; set; }
        public AttendanceViewModel Attendance { get; set; }
        public Player PlayerToCompare { get; set; }
        public bool IncludeInactive { get; set; }
    }
}