using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PrizeSelection.Api.Constants;
using PrizeSelection.Model;
using Swashbuckle.AspNetCore.SwaggerGen;
using D = PrizeSelection.Dto.Api;

namespace PrizeSelection.Api.Controllers
{

    public interface IPrizeSelectionTableController
    {
        IActionResult CreatePrizeSelectionTable(IList<D.PrizeCategorySpecification> specs);
    }

    [Produces(RouteConstants.ContentType_ApplicationJson)]
    [Route(RouteConstants.BaseRoute)]
    public class PrizeSelectionTableController : IPrizeSelectionTableController
    {
        #region Class Variables

        private readonly IMapper _mapper;
        private readonly ILogger<PrizeSelectionTableController> _logger;
        #endregion


        #region Constructors

        public PrizeSelectionTableController(IMapper mapper, ILogger<PrizeSelectionTableController> logger)
        {
            _mapper = mapper;
            _logger = logger;
        }
        #endregion

        [HttpPost]
        [Route(RouteConstants.PrizeSelectionTable)]
        [SwaggerOperation(nameof(CreatePrizeSelectionTable))]
        [ProducesResponseType(typeof(IEnumerable<D.PrizeSelectionRow>), (int)HttpStatusCode.OK)]
        public IActionResult CreatePrizeSelectionTable(IList<D.PrizeCategorySpecification> specs)
        {
            _logger.LogInformation($"Controller Method invoked: {nameof(CreatePrizeSelectionTable)}");

            IList<PrizeCategorySpecification> prizeCategorySpecifications = _mapper.Map<IList<PrizeCategorySpecification>>(specs);

            IEnumerable<PrizeSelectionRow> model = null;

            IEnumerable<D.PrizeSelectionRow> result = _mapper.Map<IEnumerable<D.PrizeSelectionRow>>(model);

            return new ObjectResult(result);
        }
    }
}
