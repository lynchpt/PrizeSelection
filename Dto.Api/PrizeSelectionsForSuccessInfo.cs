using System;

namespace PrizeSelection.Dto.Api
{
    public class PrizeSelectionsForSuccessInfo
    {
        public int TrialsConducted { get; set; }
        public int MinSelectionsRequired { get; set; }
        public int MaxSelectionsRequired { get; set; }
        public double MedianSelectionsRequired { get; set; }
        public int ModeSelectionsRequired { get; set; }
        public double MeanSelectionsRequired { get; set; }

        public override string ToString()
        {
            return $"TrialsConducted: {TrialsConducted}{Environment.NewLine}" +
                   $"MinSelectionsRequired: {MinSelectionsRequired}{Environment.NewLine}" +
                   $"MaxSelectionsRequired: {MaxSelectionsRequired}{Environment.NewLine}" +
                   $"MedianSelectionsRequired: {MedianSelectionsRequired}{Environment.NewLine}" +
                   $"ModeSelectionsRequired: {ModeSelectionsRequired}{Environment.NewLine}" +
                   $"MeanSelectionsRequired: {MeanSelectionsRequired}{Environment.NewLine}";
        }
    }
}
