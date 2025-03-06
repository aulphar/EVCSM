using Microsoft.AspNetCore.Mvc;
using EVCSMBackend.Models;
using EVCSMBackend.Services;
using System.Text;
using MQTTnet;
using MQTTnet.Protocol;
using Newtonsoft.Json;

namespace EVCSMBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargingSessionAPI : ControllerBase
    {
           private readonly IMqttClient? _mqttClient;
           private readonly MongoDbService _mongoDbService;
           private static Timer _energyTimer;
           
            public ChargingSessionAPI(MongoDbService mongoDbService, IMqttClient mqttClient)
            {
                _mongoDbService = mongoDbService;
                _mqttClient = mqttClient;
             
            }

            [HttpPost("start")]
            public async Task<IActionResult> StartCharging()
            {
                var session = new ChargingSession();
                var sessionserialised = JsonConvert.SerializeObject(session);

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("charging/updates")
                    .WithPayload(Encoding.UTF8.GetBytes(sessionserialised))
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .Build();

                await _mongoDbService.StoreChargingSession(session);
                _mqttClient.PublishAsync(message);

                _energyTimer = new Timer(async state =>
                {
                    var activeSession = await _mongoDbService.GetActiveSession();
                    if (activeSession != null && activeSession.Status != "Stopped")
                    {
                        activeSession.EnergyConsumed += 0.5;
                        Console.WriteLine($"Energy consumed: {activeSession.EnergyConsumed}");
                        await _mongoDbService.UpdateChargingSession(activeSession);
                    }
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));


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
                var sessionserialised = JsonConvert.SerializeObject(session);

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("charging/updates")
                    .WithPayload(Encoding.UTF8.GetBytes(sessionserialised))
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .Build();
                _mqttClient.PublishAsync(message);
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



