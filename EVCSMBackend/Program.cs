using EVCSMBackend.Services;
using MQTTnet;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();



var mqttclient = new MqttClientFactory().CreateMqttClient();



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

app.UseDefaultFiles(); 
app.UseStaticFiles(); 

app.UseAuthorization();

app.MapControllers();


app.Run();


