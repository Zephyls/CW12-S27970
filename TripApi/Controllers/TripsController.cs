using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TripApi.DTOs;
using TripApi.Services;

namespace TripApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripsController(ITripService tripService)
        {
            _tripService = tripService;
        }
        

        [HttpGet]
        public async Task<IActionResult> GetTrips([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            int pageNum = page ?? 1;
            int ps = pageSize ?? 10;

            var result = await _tripService.GetTripsAsync(pageNum, ps);

            var response = new
            {
                pageNum = result.PageNum,
                pageSize = result.PageSize,
                allPages = result.AllPages,
                trips = result.Items
            };

            return Ok(response);
        }
        

        [HttpPost("{idTrip}/clients")]
        public async Task<IActionResult> AddClientToTrip(int idTrip, [FromBody] CreateClientForTripDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var errorMessage = await _tripService.AddClientToTripAsync(idTrip, dto);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return BadRequest(new { message = errorMessage });
            }

            return Ok(new { message = "Client registered successfully for the trip." });
        }
    }
}
