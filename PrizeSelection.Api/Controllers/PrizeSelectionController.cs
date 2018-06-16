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
        /// This helper method takes submitted metadata and transforms them into a PrizeCategorySpecification object. This method 
        /// is not required; it is merely for convenience.
        /// 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="prizeCategoryName"></param>
        /// <param name="probabilityExtentForEntireCategory"></param>
        /// <param name="prizesInPrizeCategoryCount"></param>
        /// <response code="200">
        ///     <see>IList&lt;PrizeCategorySpecification&gt;</see>
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


        /// <summary>
        /// This helper method takes submitted metadata and transforms them into a PrizeCategorySpecification object. This method 
        /// is not required; it is merely for convenience.
        /// 
        /// </summary>
        /// <remarks>
        /// This method is for use when you don't want to bother supplying names for the Prizes; this method will generate default 
        /// names for you.
        /// </remarks>
        /// <param name="prizeCategoryName"></param>
        /// <param name="probabilityExtentForEntireCategory"></param>
        /// <param name="prizesInPrizeCategoryCount"></param>
        /// <response code="200">
        ///     <see>IList&lt;PrizeCategorySpecification&gt;</see>
        /// </response>
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

        /// <summary>
        /// This helper method takes submitted PrizeCategorySpecifications and transforms them into a fleshed out set of
        /// PrizeSelectionRows (i.e. makes the PrizeSelection table).
        /// </summary>
        /// <remarks>
        /// The passed in data in the PrizeCategorySpecification contains the name of the category, the list of Prizes (by name) 
        /// in the category, and how much of the probability table the Prizes cover. Note that each Prize takes an equal pro rata
        /// share of the total ProbabilityExtentForEntireCategory. For example, if you provide ten Prizes and a ProbabilityExtentForEntireCategory
        /// value of 0.20, the resulting set of PrizeSelectionRows from this method will have ten members (one for each Prize), each 
        /// of which is assigned a 0.02 span in the Probablity table, which equates to a 2% chance of being chosen in each selection.
        /// <br /> 
        /// Note that each prize in a PrizeCategorySpecification will have the same chance of being chosen. If you need to simulate a 
        /// PrizeSelection table where some Prizes have different selection chances than others, you will need to pass to this 
        /// method multiple PrizeCategorySpecification.
        /// </remarks>
        /// <param name="prizeCategorySpecifications">the list of PrizeCategorySpecification objects that describe what Prizes 
        /// should be on the resulting PrizeSelection table and how much probablity space they each share equally.</param>
        /// <response code="200">
        ///     <see>IList&lt;PrizeSelectionRow&gt;</see>
        /// </response>
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
        /// <br /> 
        /// If contructing a PrizeSelectionTable seems daunting, look into the helper method \PrizeSelectionTable that can help you create one.
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
        /// Performs the specified number of Prize Selection operation against the provided SelectionDomains, which specify how 
        /// many prizes are selected and what rates each individual prize has of being selected from the provided PrizeSelectionTables.
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

        /// <summary>
        /// This method calculates what the chance is that: for the specified number of Prize Selection operations against the specified 
        /// SelectionDomains (see the documentation for PrizeResults/{selectionCount} for details on SelectionDomains), the combined results 
        /// of all selections will meet or exceed the SuccessCriteria you specify. In this case, SuccessCriteria can include a SubsetCount value
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// This method differs from /SuccessChance in one respect:
        /// rather than every single non-zero Prize count you set needing to be matched for a PrizeSelection operation to be a success, you
        /// additionally specify a smaller number, and if even this smaller number of Prizes meet their count criteria, the 
        /// PrizeSelection operation will count as a success. For example, you can specify three Prizes with non zero counts, but also 
        /// specify a subset value of two, meaning that if if any two Prizes out of the three are selected, the PrizeSelection operation 
        /// counts as a success.
        /// <br /> 
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
        /// that are used to determine if the Prizes selected in any of the Prize Selection operations constitute success. The input 
        /// also contains a SubsetCount value
        /// </param>
        /// <response code="200">
        ///     <see>double</see>
        /// </response>
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

        /// <summary>
        /// This method provides statistical information about how many Prize Selection operations will need to be performed before the 
        /// success criteria you provide will be met or exceeded. Numerous trials are performed to lend greater accuracy to the results.
        /// 
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// The information includes:
        /// <br /> 
        /// - TrialsConducted
        /// - MinSelectionsRequired
        /// - MaxSelectionsRequired
        /// - MedianSelectionsRequired
        /// - ModeSelectionsRequired
        /// - MeanSelectionsRequired
        ///
        /// </remarks>
        /// <param name="successCalculationInput">the required input object that contains the SelectionDomains making up each 
        /// Prize Selection operation, how many Prize Selection operations to perform before checking for success, and the SuccessCriteria 
        /// that are used to determine if the Prizes selected in any of the Prize Selection operations constitute success.
        /// </param>
        /// <response code="200">
        ///     <see>PrizeSelectionsForSuccessInfo</see>
        /// </response>
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

        /// <summary>
        /// This method provides statistical information about how many Prize Selection operations will need to be performed before the 
        /// success criteria you provide will be met or exceeded. Numerous trials are performed to lend greater accuracy to the results.
        /// 
        /// 
        /// </summary>
        /// <remarks>
        /// This method differs from /SelectionsUntilSuccess in one respect:
        /// rather than every single non-zero Prize count you set needing to be matched for a PrizeSelection operation to be a success, you
        /// additionally specify a smaller number, and if even this smaller number of Prizes meet their count criteria, the 
        /// PrizeSelection operation will count as a success. For example, you can specify three Prizes with non zero counts, but also 
        /// specify a subset value of two, meaning that if if any two Prizes out of the three are selected, the PrizeSelection operation 
        /// counts as a success.
        /// 
        /// The information includes:
        /// <br /> 
        /// - TrialsConducted
        /// - MinSelectionsRequired
        /// - MaxSelectionsRequired
        /// - MedianSelectionsRequired
        /// - ModeSelectionsRequired
        /// - MeanSelectionsRequired
        ///
        /// </remarks>
        /// <param name="successCalculationInput">the required input object that contains the SelectionDomains making up each 
        /// Prize Selection operation, how many Prize Selection operations to perform before checking for success, and the SuccessCriteria 
        /// that are used to determine if the Prizes selected in any of the Prize Selection operations constitute success. The input 
        /// also contains a SubsetCount value
        /// </param>
        /// <response code="200">
        ///     <see>PrizeSelectionsForSuccessInfo</see>
        /// </response>
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
