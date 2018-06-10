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
        IActionResult GetPrizeSelectionSingle(IList<D.SelectionDomain> selectionDomains);
        IActionResult GetPrizeSelectionMulti(IList<D.SelectionDomain> selectionDomains, int selectionCount);
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
        #endregion

        #region Constructors

        public PrizeSelectionController(IPrizeSelectionTableHelper prizeSelectionTableHelper, ISelectionEngine selectionEngine,
            IMapper mapper, ILogger<PrizeSelectionController> logger)
        {
            _prizeSelectionTableHelper = prizeSelectionTableHelper;
            _selectionEngine = selectionEngine;
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
            IList<PrizeCategorySpecification> prizeCategorySpecifications = new List<PrizeCategorySpecification>();

            if (prizeNames != null && prizeNames.Any())
            {
                PrizeCategorySpecification spec = _prizeSelectionTableHelper.CreatePrizeCategorySpecification(
                    prizeCategoryName, probabilityExtentForEntireCategory, prizeNames);

                prizeCategorySpecifications = new List<PrizeCategorySpecification>(){ spec };
            }

            return new ObjectResult(prizeCategorySpecifications);
        }

        [HttpPost]
        [Route(RouteConstants.PrizeSelectionTable)]
        [SwaggerOperation(nameof(GetPrizeSelectionTable))]
        [ProducesResponseType(typeof(IEnumerable<D.PrizeSelectionRow>), (int)HttpStatusCode.OK)]
        public IActionResult GetPrizeSelectionTable([FromBody]IList<D.PrizeCategorySpecification> prizeCategorySpecifications)
        {
            IList<PrizeSelectionRow> prizeSelectionTable = new List<PrizeSelectionRow>();

            if (prizeCategorySpecifications != null && prizeCategorySpecifications.Any())
            {
                IList<PrizeCategorySpecification> model = _mapper.Map<IList<PrizeCategorySpecification>>(prizeCategorySpecifications);

                prizeSelectionTable = _prizeSelectionTableHelper.GetPrizeSelectionTable(model);
            }

            return new ObjectResult(prizeSelectionTable);
        }

        [HttpPost]
        [Route(RouteConstants.GetSelectPrizesSingle)]
        [SwaggerOperation(nameof(GetPrizeSelectionSingle))]
        [ProducesResponseType(typeof(IEnumerable<D.PrizeResultRow>), (int)HttpStatusCode.OK)]
        public IActionResult GetPrizeSelectionSingle([FromBody]IList<D.SelectionDomain> selectionDomains)
        {
            IList<PrizeResultRow> prizeResultsTable = new List<PrizeResultRow>();

            if (selectionDomains != null && selectionDomains.Any())
            {
                //this is the convenience method where we just assume one pull is the desired number
                int selectionCount = 1;

                IList<SelectionDomain> selectionDomainModels = _mapper.Map<IList<SelectionDomain>>(selectionDomains);

                prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomainModels, selectionCount);
            }

            return new ObjectResult(prizeResultsTable);
        }

        [HttpPost]
        [Route(RouteConstants.GetSelectPrizesMulti)]
        [SwaggerOperation(nameof(GetPrizeSelectionMulti))]
        [ProducesResponseType(typeof(IEnumerable<D.PrizeResultRow>), (int)HttpStatusCode.OK)]
        public IActionResult GetPrizeSelectionMulti([FromBody]IList<D.SelectionDomain> selectionDomains, int selectionCount)
        {
            IList<PrizeResultRow> prizeResultsTable = new List<PrizeResultRow>();

            if (selectionDomains != null && selectionDomains.Any())
            {

                IList<SelectionDomain> selectionDomainModels = _mapper.Map<IList<SelectionDomain>>(selectionDomains);

                prizeResultsTable = _selectionEngine.SelectPrizes(selectionDomainModels, selectionCount);
            }

            return new ObjectResult(prizeResultsTable);
        }
        #endregion
    }
}
