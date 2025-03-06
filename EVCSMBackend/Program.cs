using EVCSMBackend.Services;
using MQTTnet;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var options = new MqttClientOptionsBuilder()
    .WithWebSocketServer(o => o.WithUri("ws://127.0.0.1:9001"))
    .WithCleanSession()
    .Build();

IMqttClient mqttclient = new MqttClientFactory().CreateMqttClient();

mqttclient.ConnectAsync(options);

builder.Services.AddSingleton<IMqttClient>(mqttclient);

builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<MqttSubscriberService>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<MqttSubscriberService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();


