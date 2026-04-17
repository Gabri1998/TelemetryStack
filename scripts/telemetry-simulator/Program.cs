using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;
using System.Text.Json;

var factory = new MqttFactory();
var client = factory.CreateMqttClient();

var options = new MqttClientOptionsBuilder()
    .WithClientId("simulator")
    .WithTcpServer("localhost", 1883)
    .Build();

Console.WriteLine("Connecting to MQTT...");
await client.ConnectAsync(options, CancellationToken.None);
Console.WriteLine("Connected");

var random = new Random();

// use a real device ID
var deviceId = "07864ccf-c89e-4024-a918-00cc3f78517e";

while (true)
{
    var payload = new
    {
        deviceId = deviceId,
        temperature = random.Next(20, 30),
        speed = random.Next(0, 120),
        battery = random.Next(50, 100),
        timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
    };

    var json = JsonSerializer.Serialize(payload);

    var message = new MqttApplicationMessageBuilder()
        .WithTopic("devices/telemetry")
        .WithPayload(Encoding.UTF8.GetBytes(json))
        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
        .Build();

    await client.PublishAsync(message, CancellationToken.None);

    Console.WriteLine($"Sent: {json}");

    await Task.Delay(1000);
}