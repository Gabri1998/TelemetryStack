using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;
using System.Text.Json;
using TelemetryService.Models;

namespace TelemetryService.Services;

public class MqttWorker : BackgroundService
{
   private readonly TelemetryProcessor _telemetryProcessor;

public MqttWorker(TelemetryProcessor telemetryProcessor)
{
    _telemetryProcessor = telemetryProcessor;
}

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttFactory();
        var mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithClientId("telemetry-service")
            .WithTcpServer("localhost", 1883)
            .Build();
            var jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

            Telemetry? telemetry;

            try
            {
                telemetry = JsonSerializer.Deserialize<Telemetry>(
                    payload,
                    jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" JSON error: {ex.Message}");
                return;
             }
            if (telemetry == null)
            {
                Console.WriteLine(" Invalid telemetry payload");
                return;
            }

            Console.WriteLine($" Received telemetry from {telemetry.DeviceId}");

            await _telemetryProcessor.ProcessTelemetryAsync(telemetry);
        };

        Console.WriteLine(" Connecting to MQTT...");
        await mqttClient.ConnectAsync(options, stoppingToken);

        Console.WriteLine(" Connected");

        await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
            .WithTopic("devices/telemetry")
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
            .Build(), stoppingToken);

        Console.WriteLine(" Subscribed");

        // Keep running until app stops
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}