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
        /// <summary>
        /// Gets one Ability by its unique id
        /// </summary>
        /// <remarks>
        /// Sample Use Case - You want to find out data about the Ability called "Firaja"
        /// - You first call /api/v1.0/IdLists/Ability to get the proper IdList
        /// - Then you look up the integer Key associated with the Value of "Firaja" in the IdList (the id is 4 in this case)
        /// - Finally you call this api: api/v1.0/Abilities/4
        /// <br /> 
        /// Example - http://ffrkapi.azurewebsites.net/api/v1.0/Abilities/4 (or use Try It Out to see data in this page)
        /// </remarks>
        /// <param name="abilityId">the integer id for the desired Ability; it can be found in the Ability IdList</param>
        /// <response code="200">
        ///     <see>IEnumerable&lt;Ability&gt;</see>
        /// </response>
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
        [ProducesResponseType(typeof(IList<D.PrizeSelectionRow>), (int)HttpStatusCode.OK)]
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

        /// <summary>
        /// Performs one Prize Selection operation against the provided SelectionDomains, which specify how many prizes are selected and 
        /// what rates each individual prize has of being selected from the provided PrizeSelectionTables.
        /// 
        /// </summary>
        /// <remarks>
        /// Each provided SelectionDomain represents one stage of an overall Prize Selection operation; in many simple cases, only one 
        /// SelectionDomain should be provided. An example of this would be a simple lottery where one person (which would count as a 
        /// Prize in this scenario) has bought 11 tickets, while 9 other people each bought one. One SelectionDomain would be passed in, and it
        /// would specify how many prizes to select (one) and each Prize's (each person's) chance of being selected via the PrizeSelectionTable 
        /// object. A PrizeSelectionTable is basically a probability table with some extra metadata included; in our simple scenario, 
        /// the person Prize would have a 55% chance of being selected, and each other person would have a 5% chance.
        /// <br /> 
        /// From above, you see that different Prizes in one PrizeSelectionTable (which is contained by a SelectionDomain) can have different 
        /// probabilities of being selected. In cases where you want the same Prize to have two different probabilities for selection in 
        /// different stages of the overall Prize Selection operation, you will need multiple SelectionDomains. Let's consider an example.
        /// <br /> 
        /// If you wanted to model one guaranteed selection of one Prize out of a group of ten possible Prizes (always getting one Prize 
        /// in this selection stage), but then to also have five more selections made against the same set of ten possible Prizes where 
        /// this time each has only a 1% chance of being picked (i.e. on average, 90% of each of these five selections will pick no 
        /// Prize at all), you would need to use two SelectionDomains. The first would specify one selection against a PrizeSelectionTable 
        /// where each Prize had a 10% chance of being picked, and five selections against a PrizeSelectionTable where each Prize 
        /// had a 1% chance of being picked.
        /// <br /> 
        /// In all cases, the returned result is a set of PrizeResultRows, which functions as a table telling you for each possible prize 
        /// how many of them you picked in the Prize Selection operation. Note that any prize you specified in the SelectionDomain will 
        /// show up in the PrizeResultRow set, even if no instances of that Prize was selected.
        /// 
        /// </remarks>
        /// <param name="selectionDomains">the specification objects for how to conduct the Prize Selection operation</param>
        /// <response code="200">
        ///     <see>IList&lt;PrizeResultRow&gt;</see>
        /// </response>
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

        /// <summary>
        /// Performs one Prize Selection operation against the provided SelectionDomains, which specify how many prizes are selected and 
        /// what rates each individual prize has of being selected from the provided PrizeSelectionTables.
        /// 
        /// </summary>
        /// <remarks>
        /// Each provided SelectionDomain represents one stage of an overall Prize Selection operation; in many simple cases, only one 
        /// SelectionDomain should be provided. An example of this would be a simple lottery where one person (which would count as a 
        /// Prize in this scenario) has bought 11 tickets, while 9 other people each bought one. One SelectionDomain would be passed in, and it
        /// would specify how many prizes to select (one) and each Prize's (each person's) chance of being selected via the PrizeSelectionTable 
        /// object. A PrizeSelectionTable is basically a probability table with some extra metadata included; in our simple scenario, 
        /// the person Prize would have a 55% chance of being selected, and each other person would have a 5% chance.
        /// <br /> 
        /// From above, you see that different Prizes in one PrizeSelectionTable (which is contained by a SelectionDomain) can have different 
        /// probabilities of being selected. In cases where you want the same Prize to have two different probabilities for selection in 
        /// different stages of the overall Prize Selection operation, you will need multiple SelectionDomains. Let's consider an example.
        /// <br /> 
        /// If you wanted to model one guaranteed selection of one Prize out of a group of ten possible Prizes (always getting one Prize 
        /// in this selection stage), but then to also have five more selections made against the same set of ten possible Prizes where 
        /// this time each has only a 1% chance of being picked (i.e. on average, 90% of each of these five selections will pick no 
        /// Prize at all), you would need to use two SelectionDomains. The first would specify one selection against a PrizeSelectionTable 
        /// where each Prize had a 10% chance of being picked, and five selections against a PrizeSelectionTable where each Prize 
        /// had a 1% chance of being picked.
        /// <br /> 
        /// In all cases, the returned result is a set of PrizeResultRows, which functions as a table telling you for each possible prize 
        /// how many of them you picked in the Prize Selection operation. Note that any prize you specified in the SelectionDomain will 
        /// show up in the PrizeResultRow set, even if no instances of that Prize was selected.
        /// 
        /// </remarks>
        /// <param name="selectionDomains">the specification objects for how to conduct the Prize Selection operation</param>
        /// <param name="selectionCount">how many Prize Selection operations to perform</param>
        /// <response code="200">
        ///     <see>IList&lt;PrizeResultRow&gt;</see>
        /// </response>
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

        /// <summary>
        /// This method calculates what the chance is that: for the specified number of Prize Selection operations against the specified 
        /// SelectionDomains (see the documentation for PrizeResults/{selectionCount} for details on SelectionDomains), the combined results 
        /// of all selections will meet or exceed the SuccessCriteria you specify.
        /// 
        /// </summary>
        /// <remarks>
        /// The SuccessCalculationInput you pass in is basically the same data you provide for PrizeResults/{selectionCount}, all bundled 
        /// together in one parameter object that also includes the SuccessCriteria.
        /// <br /> 
        /// The SuccessCriteria is a simple "table" of Prize keys (the 1 based index of the Prize in the Prize Selection table) correlated 
        /// to how many instances of each of those Prizes needs to be drawn within the specified number of Prize Selection operations for 
        /// the entire set of selections to be considered a success by you. The SuccessCriteria needs to include every item that appears
        /// in any of the PrizeSelectionTables in the SelectionDomains; if any of those Prizes are irrelevant to you, just set 0 
        /// as the number of instances needed for success.
        /// <br /> 
        /// To give the best estimate of the success chance; the full set of Prize Selection operations will be simulated numerous times 
        /// to smooth out randomness and give you the average chance.
        /// <br /> 
        /// The result is just a number representing the success chance, this method does not return the exact set of Prizes selected for 
        /// any given set of Prize Selection operations or for all of them together.
        ///
        /// </remarks>
        /// <param name="successCalculationInput">the required input object that contains the SelectionDomains making up each 
        /// Prize Selection operation, how many Prize Selection operations to perform before checking for success, and the SuccessCriteria 
        /// that are used to determine if the Prizes selected in any of the Prize Selection operations constitute success.
        /// </param>
        /// <response code="200">
        ///     <see>double</see>
        /// </response>
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
