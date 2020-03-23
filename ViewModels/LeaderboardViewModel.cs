using System.Collections.Generic;
using TauManager.Models;

namespace TauManager.ViewModels
{
    /*
        Syntactically this class' members are very similar to AttendanceViewModel,
        but semantically the dictionaries here have different meaning:
        each dictionary has keys representing positions in respective list and
        the values are player ID's.
     */
     [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class LeaderboardViewModel 
    {
        public Dictionary<int /* Id */,Player> AllPlayers { get; set; }
        public AttendanceViewModel Attendance { get; set; } // for display
        public Dictionary<int /* Position */,int /* PlayerId */> TotalLeaderboardPositions { get; set; }
        public Dictionary<int,int> T5HardLeaderboardPositions { get; set; }
        public Dictionary<int,int> Last10T5HardLeaderboardPositions { get; set; }
        public Player PlayerToCompare { get; set; }
    }
}