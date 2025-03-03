using System.Text;
using EVCSMBackend.Models;
using MQTTnet;
using MQTTnet.Protocol;
using Newtonsoft.Json;

namespace EVCSMBackend.Services
{
    public class MqttSubscriberService : IHostedService, IDisposable
    {
        private readonly MongoDbService _mongoDbService;
        private IMqttClient? _mqttClient;
        private Timer? _simulationTimer;
        private ChargingSession? _currentSession;

        public MqttSubscriberService(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithWebSocketServer(o => o.WithUri("ws://127.0.0.1:9001/mqtt"))
                .WithClientId(Guid.NewGuid().ToString())
                .WithCleanSession()
                .Build();

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Console.WriteLine($"Received message: {payload}");

                try
                {
                    var messageData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(payload);
                    if (messageData?.status == "Charging")
                    {
                        var chargingSession = new ChargingSession();
                        await _mongoDbService.StoreChargingSession(chargingSession);
                        Console.WriteLine("Stored new charging session in MongoDB.");

                        
                        _simulationTimer = new Timer(async _ =>
                            {
                                if (_currentSession != null && _currentSession.Status == "Charging")
                                {
                                    _currentSession.EnergyConsumed += 0.5; // Simulate +0.5 kWh per second
                                    Console.WriteLine($"Simulated energy: {_currentSession.EnergyConsumed} kWh");

                                    // Publish updated session data over MQTT
                                    var updatePayload = Newtonsoft.Json.JsonConvert.SerializeObject(new
                                    {
                                        status = _currentSession.Status,
                                        energyConsumed = _currentSession.EnergyConsumed,
                                        timestamp = DateTime.UtcNow
                                    });
                                    if (_mqttClient != null && _mqttClient.IsConnected)
                                    {
                                        var message = new MqttApplicationMessageBuilder()
                                            .WithTopic("charging/updates")
                                            .WithPayload(Encoding.UTF8.GetBytes(updatePayload))
                                            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                                            .WithRetainFlag()
                                            .Build();

                                        await _mqttClient.PublishAsync(message, cancellationToken);
                                    }}
                            }, null, 0, 1000);
                    }
                    else if (messageData?.status == "Stopped" && _currentSession != null)
                    {
                        _simulationTimer?.Dispose();
                        _simulationTimer = null;
                        _currentSession.Status = "Stopped";
                        _currentSession.EndTime = DateTime.UtcNow;

                        await _mongoDbService.UpdateChargingSession(_currentSession);
                        Console.WriteLine("Updated charging session in MongoDB as stopped.");

                        var finalPayload = JsonConvert.SerializeObject(new
                        {
                            status = _currentSession.Status,
                            energyConsumed = _currentSession.EnergyConsumed,
                            timestamp = DateTime.UtcNow
                        });

                        if (_mqttClient != null && _mqttClient.IsConnected)
                        {
                            var message = new MqttApplicationMessageBuilder()
                                .WithTopic("charging/updates")
                                .WithPayload(Encoding.UTF8.GetBytes(finalPayload))
                                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                                .WithRetainFlag()
                                .Build();

                            await _mqttClient.PublishAsync(message, cancellationToken);
                        }
                        _currentSession = null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                }
            };

            _mqttClient.ConnectedAsync += e =>
            {
                Console.WriteLine("Connected to MQTT broker.");
                return _mqttClient.SubscribeAsync("charging/updates");
            };

            await _mqttClient.ConnectAsync(options, cancellationToken);
        }

       
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_simulationTimer != null)
            {
                _simulationTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _simulationTimer.Dispose();
                _simulationTimer = null;
            }
            if (_mqttClient != null && _mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
            }
        }

        public void Dispose()
        {
            _simulationTimer?.Dispose();
        }
    }
}