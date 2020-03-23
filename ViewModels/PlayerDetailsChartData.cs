using System.Collections.Generic;

namespace TauManager.ViewModels
{
    public class PlayerDetailsChartData: List<PlayerDetailsChartDataPoint>
    {
        public enum Interval: byte { Week, Month1, Month3, Month6, Year, Max }
        public enum DataKind: byte { StatsTotal, Credits, Bonds, XP }
    }
}