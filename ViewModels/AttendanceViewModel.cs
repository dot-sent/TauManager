using System.Collections.Generic;

namespace TauManager.ViewModels
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class AttendanceViewModel
    {
        public Dictionary<int, int> TotalAttendance { get; set; }
        public Dictionary<int, int> Last10T5HardAttendance { get; set; }
        public Dictionary<int, int> T5HardAttendance { get; set; }
    }
}