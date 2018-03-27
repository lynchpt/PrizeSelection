using System;
using System.Collections.Generic;
using System.Text;

namespace PrizeSelection.Model
{
    public class PrizeSelectionRow
    {
        public int PrizeIndex { get; set; } //starting from 1
        public double PrizeProbabilityLowerBound { get; set; }
        public string PrizeCategoryName { get; set; }
        public string PrizeName { get; set; }
    }
}
