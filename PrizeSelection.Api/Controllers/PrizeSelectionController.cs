using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PrizeSelection.Api.Constants;
using PrizeSelection.Logic;
using PrizeSelection.Model;
using Swashbuckle.AspNetCore.SwaggerGen;
using D = PrizeSelection.Dto.Api;

namespace PrizeSelection.Api.Controllers
{
    public interface IPrizeSelectionController
    {
        IActionResult GetPrizeCategorySpecification(string prizeCategoryName, double probabilityExtentForEntireCategory, IList<string> prizeNames);
        IActionResult GetPrizeCategorySpecificationNoNames(string prizeCategoryName, double probabilityExtentForEntireCategory, int prizesInPrizeCategoryCount);
        IActionResult GetPrizeSelectionTable(IList<D.PrizeCategorySpecification> prizeCategorySpecifications);

        IActionResult GetPrizeSelectionSingle(IList<D.SelectionDomain> selectionDomains);
        IActionResult GetPrizeSelectionMulti(IList<D.SelectionDomain> selectionDomains, int selectionCount);

        IActionResult GetChanceToMeetSuccessCriteriaForFixedSelectionCount(D.SuccessCalculationInput successCalculationInput);
        IActionResult GetChanceToMeetSuccessCriteriaSubsetForFixedSelectionCount(D.SuccessCalculationInput successCalculationInput);

        IActionResult GetResultsForPullsUntilSuccess(D.SuccessCalculationInput successCalculationInput);
        IActionResult GetResultsForPullsUntilSuccessSubset(D.SuccessCalculationInput successCalculationInput);
    }

    [Produces(RouteConstants.ContentType_ApplicationJson)]
    [Route(RouteConstants.BaseRoute)]
    public class PrizeSelectionController : Controller, IPrizeSelectionController
    {

        #region Class Variables

        private readonly IMapper _mapper;
        private readonly ILogger<PrizeSelectionController> _logger;
        private readonly IPrizeSelectionTableHelper _prizeSelectionTableHelper;
        private readonly ISelectionEngine _selectionEngine;
        private readonly ISelectionSuccessCalculator _selectionSuccessCalculator;
        #endregion

        #region Constructors

        public PrizeSelectionController(IPrizeSelectionTableHelper prizeSelectionTableHelper, ISelectionEngine selectionEngine,
            ISelectionSuccessCalculator selectionSuccessCalculator, IMapper mapper, ILogger<PrizeSelectionController> logger)
        {
            _prizeSelectionTableHelper = prizeSelectionTableHelper;
            _selectionEngine = selectionEngine;
            _selectionSuccessCalculator = selectionSuccessCalculator;
            _mapper = mapper;
            _logger = logger;
        }
        #endregion


        #region IPrizeSelectionController Implementation

        [HttpPost]
        [Route(RouteConstants.PrizeCategorySpecification)]
        [SwaggerOperation(nameof(GetPrizeCategorySpecification))]
        [ProducesResponseType(typeof(IEnumerable<D.PrizeCategorySpecification>), (int)HttpStatusCode.OK)]
        public IActionResult GetPrizeCategorySpecification(string prizeCategoryName, double probabilityExtentForEntireCategory,
            [FromBody]IList<string> prizeNames)
        {
            IList<D.PrizeCategorySpecification> prizeCategorySpecifications = new List<D.PrizeCategorySpecification>();

            if (prizeNames != null && prizeNames.Any())
            {
                PrizeCategorySpecification spec = _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                    prizeCategoryName, probabilityExtentForEntireCategory, prizeNames);

                IList<PrizeCategorySpecification> prizeCategorySpecificationsModel = new List<PrizeCategorySpecification>(){ spec };

                prizeCategorySpecifications = _mapper.Map<IList<D.PrizeCategorySpecification>>(prizeCategorySpecificationsModel);
            }

            return new ObjectResult(prizeCategorySpecifications);
        }

        [HttpGet]
        [Route(RouteConstants.PrizeCategorySpecificationNoNames)]
        [SwaggerOperation(nameof(GetPrizeCategorySpecificationNoNames))]
        [ProducesResponseType(typeof(IEnumerable<D.PrizeCategorySpecification>), (int)HttpStatusCode.OK)]
        public IActionResult GetPrizeCategorySpecificationNoNames(string prizeCategoryName, double probabilityExtentForEntireCategory,
            int prizesInPrizeCategoryCount)
        {
            IList<D.PrizeCategorySpecification> prizeCategorySpecifications = new List<D.PrizeCategorySpecification>();

            if (prizesInPrizeCategoryCount > 0)
            {
                PrizeCategorySpecification spec = _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                    prizeCategoryName, probabilityExtentForEntireCategory, prizesInPrizeCategoryCount);

                IList<PrizeCategorySpecification> prizeCategorySpecificationsModel = new List<PrizeCategorySpecification>() { spec };

                prizeCategorySpecifications = _mapper.Map<IList<D.PrizeCategorySpecification>>(prizeCategorySpecificationsModel);
            }

            return new ObjectResult(prizeCategorySpecifications);
        }

        [HttpPost]
        [Route(RouteConstants.PrizeSelectionTable)]
        [SwaggerOperation(nameof(GetPrizeSelectionTable))]
        [ProducesResponseType(typeof(IEnumerable<D.PrizeSelectionRow>), (int)HttpStatusCode.OK)]
        public IActionResult GetPrizeSelectionTable([FromBody]IList<D.PrizeCategorySpecification> prizeCategorySpecifications)
        {
            IList<D.PrizeSelectionRow> prizeSelectionTable = new List<D.PrizeSelectionRow>();

            if (prizeCategorySpecifications != null && prizeCategorySpecifications.Any())
            {
                IList<PrizeCategorySpecification> model = _mapper.Map<IList<PrizeCategorySpecification>>(prizeCategorySpecifications);

                IList<PrizeSelectionRow> prizeSelectionTableModel = _prizeSelectionTableHelper.GetPrizeSelectionTable(model);

                prizeSelectionTable = _mapper.Map<IList<D.PrizeSelectionRow>>(prizeSelectionTableModel);
            }

            return new ObjectResult(prizeSelectionTable);
        }

        [HttpPost]
        [Route(RouteConstants.SelectPrizesSingle)]
        [SwaggerOperation(nameof(GetPrizeSelectionSingle))]
        [ProducesResponseType(typeof(IEnumerable<D.PrizeResultRow>), (int)HttpStatusCode.OK)]
        public IActionResult GetPrizeSelectionSingle([FromBody]IList<D.SelectionDomain> selectionDomains)
        {
            IList<D.PrizeResultRow> prizeResultsTable = new List<D.PrizeResultRow>();

            if (selectionDomains != null && selectionDomains.Any())
            {
                //this is the convenience method where we just assume one pull is the desired number
                int selectionCount = 1;

                IList<SelectionDomain> selectionDomainModels = _mapper.Map<IList<SelectionDomain>>(selectionDomains);

                IList<PrizeResultRow> prizeResultsTableModel = _selectionEngine.SelectPrizes(selectionDomainModels, selectionCount);

                prizeResultsTable = _mapper.Map<IList<D.PrizeResultRow>>(prizeResultsTableModel);
            }

            return new ObjectResult(prizeResultsTable);
        }

        [HttpPost]
        [Route(RouteConstants.SelectPrizesMulti)]
        [SwaggerOperation(nameof(GetPrizeSelectionMulti))]
        [ProducesResponseType(typeof(IEnumerable<D.PrizeResultRow>), (int)HttpStatusCode.OK)]
        public IActionResult GetPrizeSelectionMulti([FromBody]IList<D.SelectionDomain> selectionDomains, int selectionCount)
        {
            IList<D.PrizeResultRow> prizeResultsTable = new List<D.PrizeResultRow>();

            if (selectionDomains != null && selectionDomains.Any())
            {
                IList<SelectionDomain> selectionDomainModels = _mapper.Map<IList<SelectionDomain>>(selectionDomains);

                IList<PrizeResultRow> prizeResultsTableModel = _selectionEngine.SelectPrizes(selectionDomainModels, selectionCount);

                prizeResultsTable = _mapper.Map<IList<D.PrizeResultRow>>(prizeResultsTableModel);
            }

            return new ObjectResult(prizeResultsTable);
        }

        [HttpPost]
        [Route(RouteConstants.SuccessChance)]
        [SwaggerOperation(nameof(GetChanceToMeetSuccessCriteriaForFixedSelectionCount))]
        [ProducesResponseType(typeof(double), (int)HttpStatusCode.OK)]
        public IActionResult GetChanceToMeetSuccessCriteriaForFixedSelectionCount([FromBody]D.SuccessCalculationInput successCalculationInput)
        {
            double successChance = 0;

            if (successCalculationInput.SuccessCriteria != null && successCalculationInput.SuccessCriteria.Any() &&
                successCalculationInput.SelectionDomains != null && successCalculationInput.SelectionDomains.Any() &&
                successCalculationInput.SelectionCount > 0)
            {
                IList<SelectionDomain> selectionDomainModels = _mapper.Map<IList<SelectionDomain>>(successCalculationInput.SelectionDomains);

                successChance = _selectionSuccessCalculator.GetChanceToMeetSuccessCriteriaForFixedSelectionCount(
                    successCalculationInput.SuccessCriteria, selectionDomainModels, successCalculationInput.SelectionCount);
            }

            return new ObjectResult(successChance);
        }

        [HttpPost]
        [Route(RouteConstants.SuccessChanceSubset)]
        [SwaggerOperation(nameof(GetChanceToMeetSuccessCriteriaSubsetForFixedSelectionCount))]
        [ProducesResponseType(typeof(double), (int)HttpStatusCode.OK)]
        public IActionResult GetChanceToMeetSuccessCriteriaSubsetForFixedSelectionCount([FromBody]D.SuccessCalculationInput successCalculationInput)
        {
            double successChance = 0;

            if (successCalculationInput.SuccessCriteria != null && successCalculationInput.SuccessCriteria.Any() &&
                successCalculationInput.SelectionDomains != null && successCalculationInput.SelectionDomains.Any() &&
                successCalculationInput.SelectionCount > 0 && successCalculationInput.SubsetSize.HasValue)
            {
                IList<SelectionDomain> selectionDomainModels = _mapper.Map<IList<SelectionDomain>>(successCalculationInput.SelectionDomains);

                successChance = _selectionSuccessCalculator.GetChanceToMeetSuccessCriteriaSubsetForFixedSelectionCount(
                    successCalculationInput.SuccessCriteria, selectionDomainModels, successCalculationInput.SelectionCount, successCalculationInput.SubsetSize.Value);
            }

            return new ObjectResult(successChance);
        }


        [HttpPost]
        [Route(RouteConstants.SelectionsUntilSuccess)]
        [SwaggerOperation(nameof(GetResultsForPullsUntilSuccess))]
        [ProducesResponseType(typeof(D.PrizeSelectionsForSuccessInfo), (int)HttpStatusCode.OK)]
        public IActionResult GetResultsForPullsUntilSuccess([FromBody]D.SuccessCalculationInput successCalculationInput)
        {
            D.PrizeSelectionsForSuccessInfo result = new D.PrizeSelectionsForSuccessInfo();

            if (successCalculationInput.SuccessCriteria != null && successCalculationInput.SuccessCriteria.Any() &&
                successCalculationInput.SelectionDomains != null && successCalculationInput.SelectionDomains.Any())
            {
                IList<SelectionDomain> selectionDomainModels = _mapper.Map<IList<SelectionDomain>>(successCalculationInput.SelectionDomains);

                PrizeSelectionsForSuccessInfo successModel = _selectionSuccessCalculator.GetResultsForPullsUntilSuccess(successCalculationInput.SuccessCriteria, selectionDomainModels);

                result = _mapper.Map<D.PrizeSelectionsForSuccessInfo>(successModel);
            }

            return new ObjectResult(result);
        }

        [HttpPost]
        [Route(RouteConstants.SelectionsUntilSuccessSubset)]
        [SwaggerOperation(nameof(GetResultsForPullsUntilSuccessSubset))]
        [ProducesResponseType(typeof(D.PrizeSelectionsForSuccessInfo), (int)HttpStatusCode.OK)]
        public IActionResult GetResultsForPullsUntilSuccessSubset([FromBody]D.SuccessCalculationInput successCalculationInput)
        {
            D.PrizeSelectionsForSuccessInfo result = new D.PrizeSelectionsForSuccessInfo();

            if (successCalculationInput.SuccessCriteria != null && successCalculationInput.SuccessCriteria.Any() &&
                successCalculationInput.SelectionDomains != null && successCalculationInput.SelectionDomains.Any() &&
                successCalculationInput.SubsetSize.HasValue)
            {
                IList<SelectionDomain> selectionDomainModels = _mapper.Map<IList<SelectionDomain>>(successCalculationInput.SelectionDomains);

                PrizeSelectionsForSuccessInfo successModel = _selectionSuccessCalculator.GetResultsForPullsUntilSuccessSubset(
                    successCalculationInput.SuccessCriteria, selectionDomainModels, successCalculationInput.SubsetSize.Value);

                result = _mapper.Map<D.PrizeSelectionsForSuccessInfo>(successModel);
            }

            return new ObjectResult(result);
        }
        #endregion
    }
}
