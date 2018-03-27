using System.Collections.Generic;

namespace PrizeSelection.Dto.Api
{
    public class PrizeCategorySpecification
    {
        public string PrizeCategoryName { get; set; } //eg Guaranteed

        //whatever probability number is assigned here will be evenly divided among all {PrizeCount} prizes.
        //For example, if the ProbabilityExtentForEntireCategory is 0.4 and PrizeCount is 8,
        //the final prize selection table will have (among others, perhaps) one row for each of the 8 prizes,
        //and each one will have a 0.05 probability range, or chance of being sleected
        public double ProbabilityExtentForEntireCategory { get; set; }

        //only set one of the below; but one MUST be set

        //should not be set simultaneously with PrizeNames; set one and let the other be derived
        public int PrizeCount { get; set; } //eg 14

        //should not be set simultaneously with PrizeCategoryName; set one and let the other be derived
        //Whether assigned or derived, each resulting prize name for each prize catgory will end up as a row in the PrizeSelectionRow
        public IList<string> PrizeNames { get; set; }
    }
}
