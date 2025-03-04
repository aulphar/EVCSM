using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EVCSMBackend.Models;
using EVCSMBackend.Services;
using System;
using System.Threading.Tasks;

namespace EVCSMBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargingSessionAPI : ControllerBase
    {

            private readonly MongoDbService _mongoDbService;

            public ChargingSessionAPI(MongoDbService mongoDbService)
            {
                _mongoDbService = mongoDbService;
            }

            [HttpPost("start")]
            public async Task<IActionResult> StartCharging()
            {
                var session = new ChargingSession
                {
                    Status = "Charging",
                    StartTime = DateTime.UtcNow,
                    EnergyConsumed = 0
                };

                await _mongoDbService.StoreChargingSession(session);
                return Ok(session);
            }

            [HttpPost("stop")]
            public async Task<IActionResult> StopCharging()
            {
                var session = await _mongoDbService.GetActiveSession();
                if (session == null)
                    return NotFound("No active session found.");

                session.Status = "Stopped";
                session.EndTime = DateTime.UtcNow;
                await _mongoDbService.UpdateChargingSession(session);
                return Ok(session);
            }

            [HttpGet("status")]
            public async Task<IActionResult> GetStatus()
            {
                var session = await _mongoDbService.GetLatestSession();
                return Ok(session);
            }
        }
    }



