using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidayController : ControllerBase
    {   
        private readonly HolidayService _holidayService;

        private readonly HolidayPendingService _holidayPendingService;

        public HolidayController(HolidayService holidayService, HolidayPendingService holidayPendingService)
        {
            _holidayService = holidayService;
            _holidayPendingService = holidayPendingService;
        }

        
        // POST: api/Holiday
        [HttpPost]
        public async Task<ActionResult<HolidayDTO>> PostHoliday(HolidayDTO holidayDTO)
        {
            List<string> errorMessages = new List<string>(); // Declare errorMessages here to capture new errors for each call

            HolidayDTO holidayResultDTO = await _holidayPendingService.Add(holidayDTO, errorMessages);
            if (holidayResultDTO != null)
            {
                return Ok(holidayResultDTO);
            }
            else
            {
                return BadRequest(errorMessages);
            }
        }
    }
}
