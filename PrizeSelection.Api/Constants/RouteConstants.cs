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

        public const string PrizeSelectionTable = "PrizeSelectionTable"; //POST IList<PrizeCategorySpecification>

        public const string GetSelectPrizesSingle = "PrizeResults";
        public const string GetSelectPrizesMulti = "PrizeResults/{selectionCount}";

        //public const string PrizeSelectionTable = ""; //POST IList<PrizeCategorySpecification>

    }
}
