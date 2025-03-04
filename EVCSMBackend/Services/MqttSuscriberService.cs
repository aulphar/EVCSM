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
            var client = new MqttClientFactory()
                .CreateMqttClient();
            _mqttClient = client;
            _mqttClient.ConnectedAsync += e =>
            {
                Console.WriteLine("Connected to MQTT broker.");
                return _mqttClient.SubscribeAsync("charging/updates");
            };
            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Console.WriteLine($"Received message: {payload}");

                try
                {
                    var messageData = JsonConvert.DeserializeObject<dynamic>(payload);
                    if (messageData?.status == "Charging")
                    {
                        var chargingSession = new ChargingSession();
                        await _mongoDbService.StoreChargingSession(chargingSession);
                        Console.WriteLine("Stored new charging session in MongoDB.");
                        _currentSession = chargingSession;
                       
                        var sessionDetails = JsonConvert.SerializeObject(chargingSession);
                        var message = new MqttApplicationMessageBuilder()
                            .WithTopic("session/details")
                            .WithPayload(Encoding.UTF8.GetBytes(sessionDetails))
                            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                            .Build();

                        await _mqttClient.PublishAsync(message, cancellationToken);


                        _simulationTimer = new Timer(async _ =>
                        {
                            if (_currentSession != null && _currentSession.Status == "Charging")
                            {
                                _currentSession.EnergyConsumed += 0.5; 
                                Console.WriteLine($"Simulated energy: {_currentSession.EnergyConsumed} kWh");

                                var updatePayload = Newtonsoft.Json.JsonConvert.SerializeObject(new
                                {
                                    status = _currentSession.Status,
                                    energyConsumed = _currentSession.EnergyConsumed,
                                    timestamp = DateTime.UtcNow
                                });
                                if (_mqttClient != null && _mqttClient.IsConnected)
                                {
                                    var updates = new MqttApplicationMessageBuilder()
                                        .WithTopic("session/updates")
                                        .WithPayload(Encoding.UTF8.GetBytes(updatePayload))
                                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                                        .Build();

                                    await _mqttClient.PublishAsync(updates, cancellationToken);
                                }}
                        }, null, 0, 1000);
                        
                    }
                    else if (messageData?.status == "Stopped")
                    {
                        _simulationTimer?.Dispose();
                        _simulationTimer = null;
                        _currentSession.Status = "Stopped";
                        _currentSession.EndTime = DateTime.UtcNow;

                        await _mongoDbService.UpdateChargingSession(_currentSession);
                        Console.WriteLine("Updated charging session in MongoDB.");
                        if (_mqttClient != null && _mqttClient.IsConnected)
                        {
                            var lastSession = await _mongoDbService.GetLatestSession();
                            var lastSessionDetails = JsonConvert.SerializeObject(lastSession);
                            var message = new MqttApplicationMessageBuilder()
                                .WithTopic("session/details")
                                .WithPayload(Encoding.UTF8.GetBytes(lastSessionDetails))
                                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                                .Build();

                            await _mqttClient.PublishAsync(message, cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex}");
                }

            };
            var options = new MqttClientOptionsBuilder()
                .WithWebSocketServer(o => o.WithUri("ws://127.0.0.1:9001/mqtt"))
                .WithCleanSession()
                .Build();
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
                await _mqttClient.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken);
            }
        }
        public void Dispose()
        {
            _simulationTimer?.Dispose();
        }
    }
}