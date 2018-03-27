namespace PrizeSelection.Dto.Api
{
    public class PrizeSelectionRow
    {
        public int PrizeIndex { get; set; } //starting from 1
        public double PrizeProbabilityLowerBound { get; set; }
        public string PrizeCategoryName { get; set; }
        public string PrizeName { get; set; }
    }
}
