using System.Collections.Generic;

namespace PrizeSelection.Dto.Api
{
    public class SelectionDomain
    {
        public string SelectionDomainName { get; set; }
        public int PrizesToSelectFromDomainCount { get; set; }
        public IList<PrizeSelectionRow> PrizeSelectionTable { get; set; }
    }
}
