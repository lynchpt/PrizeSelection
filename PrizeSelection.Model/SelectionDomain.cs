using System;
using System.Collections.Generic;
using System.Text;

namespace PrizeSelection.Model
{
    public class SelectionDomain
    {
        public string SelectionDomainName { get; set; }
        public int PrizesToSelectFromDomainCount { get; set; }
        public IList<PrizeSelectionRow> PrizeSelectionTable { get; set; }
    }
}
