using System;

namespace PrizeSelection.Dto.Api
{
    public class PrizeSelectionsForSuccessInfo
    {
        public int TrialsConducted { get; set; }
        public int MinPullsRequired { get; set; }
        public int MaxPullsRequired { get; set; }
        public double MedianPullsRequired { get; set; }
        public int ModePullsRequired { get; set; }
        public double MeanPullsRequired { get; set; }

        public override string ToString()
        {
            return $"TrialsConducted: {TrialsConducted}{Environment.NewLine}" +
                   $"MinPullsRequired: {MinPullsRequired}{Environment.NewLine}" +
                   $"MaxPullsRequired: {MaxPullsRequired}{Environment.NewLine}" +
                   $"MedianPullsRequired: {MedianPullsRequired}{Environment.NewLine}" +
                   $"ModePullsRequired: {ModePullsRequired}{Environment.NewLine}" +
                   $"MeanPullsRequired: {MeanPullsRequired}{Environment.NewLine}";
        }
    }
}
