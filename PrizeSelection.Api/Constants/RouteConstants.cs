using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrizeSelection.Api.Constants
{
    public class RouteConstants
    {
        public const string ContentType_ApplicationJson = "application/json";

        public const string BaseRoute = "api/v1.0/[controller]"; //to handle versioning

        //IdList Routes

        public const string PrizeCategorySpecification = "PrizeCategorySpecification/{prizeCategoryName}/{probabilityExtentForEntireCategory}";
        public const string PrizeCategorySpecificationNoNames = "PrizeCategorySpecificationNoNames/{prizeCategoryName}/{probabilityExtentForEntireCategory}/{prizesInPrizeCategoryCount}";

        public const string PrizeSelectionTable = "PrizeSelectionTable"; //POST IList<PrizeCategorySpecification>

        public const string SelectPrizesSingle = "PrizeResults";
        public const string SelectPrizesMulti = "PrizeResults/{selectionCount}";

        public const string SuccessChance = "SuccessChance";
        public const string SuccessChanceSubset = "SuccessChanceSubset";

        public const string SelectionsUntilSuccess = "SelectionsUntilSuccess";
        public const string SelectionsUntilSuccessSubset = "SelectionsUntilSuccessSubset";

        //GetChanceToMeetSuccessCriteriaForFixedSelectionCount
    }
}
