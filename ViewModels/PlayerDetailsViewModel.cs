using System.Collections.Generic;
using TauManager.Models;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class PlayerDetailsViewModel
    {
        public Player Player { get; set; }
        public IEnumerable<PlayerHistory> History { get; set; }
        public int MoreRows { get; set; }
        public long LastActivityId { get; set; }
        public AttendanceViewModel Attendance { get; set; }
    }
}