namespace PrizeSelection.Dto.Api
{
    public class PrizeResultRow
    {
        public int PrizeIndex { get; set; } //starting from 1
        public string PrizeCategoryName { get; set; }
        public string PrizeName { get; set; }
        public int PrizeSelectedCount { get; set; }

        public override string ToString()
        {
            return $"{PrizeIndex}\t\t{PrizeSelectedCount}\t\t{PrizeCategoryName}\t\t{PrizeName}";
        }
    }
}
