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

        //  Handle incoming messages
        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

            try
            {
                var telemetry = JsonSerializer.Deserialize<Telemetry>(payload, jsonOptions);

                if (telemetry == null)
                {
                    Console.WriteLine(" Invalid telemetry payload");
                    return;
                }

                Console.WriteLine($" Received telemetry from {telemetry.DeviceId}");

                await _telemetryProcessor.ProcessTelemetryAsync(telemetry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" JSON error: {ex.Message}");
            }
        };

        //  Handle disconnect + reconnect loop
        mqttClient.DisconnectedAsync += async e =>
{
    if (stoppingToken.IsCancellationRequested)
        return;

    Console.WriteLine(" MQTT disconnected");

    while (!stoppingToken.IsCancellationRequested)
    {
        try
        {
            await Task.Delay(2000, stoppingToken);

            // 🔥 CRITICAL GUARD
            if (mqttClient.IsConnected)
                return;

            await mqttClient.ConnectAsync(options, stoppingToken);
            Console.WriteLine(" Reconnected");

            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic("devices/telemetry")
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build(), stoppingToken);

            Console.WriteLine(" Re-subscribed");

            return; //  EXIT completely
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Reconnect failed: {ex.Message}");
        }
    }
};

        //  Initial connect
        Console.WriteLine(" Connecting to MQTT...");
        await mqttClient.ConnectAsync(options, stoppingToken);
        Console.WriteLine(" Connected");

        //  Initial subscribe
        await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
            .WithTopic("devices/telemetry")
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build(), stoppingToken);

        Console.WriteLine(" Subscribed");

        //  Keep worker alive
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}