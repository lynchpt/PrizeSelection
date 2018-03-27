using System;
using System.Collections.Generic;
using System.Text;

namespace PrizeSelection.Model
{
    public class PrizeSelectionsForSuccessInfo
    {
        public int TrialsConducted { get; set; }
        public int MinPullsRequired { get; set; }
        public int MaxPullsRequired { get; set; }
        public double MedianPullsRequired { get; set; }
        public int ModePullsRequired { get; set; }
        public double MeanPullsRequired { get; set; }
    }
}
