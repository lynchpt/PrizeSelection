using System;
using System.Collections.Generic;
using System.Text;

namespace PrizeSelection.Dto.Api
{
    public class SuccessChanceInput
    {
        public IDictionary<int, int> SuccessCriteria { get; set; }
        public IList<SelectionDomain> SelectionDomains { get; set; }
        public int SelectionCount { get; set; }
    }
}
