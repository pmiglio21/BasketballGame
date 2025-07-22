using Constants;

namespace Entities
{
    public class SkillStats
    {
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
