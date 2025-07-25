using Constants;
using Enums;
using System.Collections.Generic;

namespace Entities
{
    public class SkillStats
    {
        public HashSet<SkillStatType> HighSkillStatsFilled = new HashSet<SkillStatType>();

        public HashSet<SkillStatType> LowSkillStatsFilled = new HashSet<SkillStatType>();


        public HashSet<SkillStatType> AvailableSkillStatsToAlter = new HashSet<SkillStatType>()
        {
            SkillStatType.TwoPointShooting,
            SkillStatType.ThreePointShooting,
            SkillStatType.Dunking,
            SkillStatType.Rebounding,
            SkillStatType.Stealing,
            SkillStatType.Blocking,
            SkillStatType.BallHandling,
            SkillStatType.Speed
        };

        public int TwoPointShooting { get; set; } = GlobalConstants.SkillStatAverage;

        public int ThreePointShooting { get; set; } = GlobalConstants.SkillStatAverage;

        public int Dunking { get; set; } = GlobalConstants.SkillStatAverage;

        public int Rebounding { get; set; } = GlobalConstants.SkillStatAverage;

        public int Stealing { get; set; } = GlobalConstants.SkillStatAverage;

        public int Blocking { get; set; } = GlobalConstants.SkillStatAverage;

        public int BallHandling { get; set; } = GlobalConstants.SkillStatAverage;

        public int Speed { get; set; } = GlobalConstants.SkillStatAverage;
    }
}
