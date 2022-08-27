using Company.Product.Models.Responses;
using Company.Product.Services.Interfaces;
using Company.Product.WebApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Company.Product.WebApi.Controllers
{

    [ApiController]
    [ApiVersion("1.0")]
    [Route("/v{version:apiVersion}/calculations")]
    public class CalculationsController : ControllerBase
    {
        private readonly ILogger<CalculationsController> _logger;
        private readonly ICapacityService _capacityService;

        public CalculationsController(ILogger<CalculationsController> logger, ICapacityService capacityService)
        {
            _logger = logger;
            _capacityService = capacityService;
        }

        [Authorize]
        [HttpPost("dams/capacities", Name = nameof(CalculateDamCapacity))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpErrorResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(HttpErrorResponse))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(HttpErrorResponse))]
        public IActionResult CalculateDamCapacity([RequiredIntegerArray] int[] elevation)
        {
            return Ok(_capacityService.CalculateDamCapacityFromElevation(elevation));
        }
    }
}
