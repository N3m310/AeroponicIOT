using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Diagnostics;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System.Text;

namespace HydroponicIOT.Services.Mqtt;

/// <summary>
/// MQTT broker service for device communication
/// Handles device connections, subscriptions, and message publishing
/// </summary>
public class MqttService : IMqttService, IDisposable
{
    private readonly ILogger<MqttService> _logger;
    private readonly IConfiguration _configuration;
    private MqttServer? _mqttServer;
    private bool _isRunning;

    public bool IsRunning => _isRunning;

    public MqttService(ILogger<MqttService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Start the MQTT broker on the configured port
    /// </summary>
    public async Task StartAsync()
    {
        try
        {
            var port = int.Parse(_configuration.GetSection("MqttSettings")["Port"] ?? "1883");
            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithDefaultEndpointPort(port)
                .WithDefaultEndpointBoundIPAddress(System.Net.IPAddress.Any);

            var options = optionsBuilder.Build();

            var factory = new MqttFactory();
            _mqttServer = factory.CreateMqttServer(options);

            // Handle client connected
            _mqttServer.ClientConnectedAsync += async e =>
            {
                _logger.LogInformation("MQTT Client connected: {ClientId}", e.ClientId);
                await Task.CompletedTask;
            };

            // Handle client disconnected
            _mqttServer.ClientDisconnectedAsync += async e =>
            {
                _logger.LogInformation("MQTT Client disconnected: {ClientId}", e.ClientId);
                await Task.CompletedTask;
            };

            await _mqttServer.StartAsync();
            _isRunning = true;

            _logger.LogInformation("MQTT Broker started on port {Port}", port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting MQTT broker");
            throw;
        }
    }

    /// <summary>
    /// Stop the MQTT broker
    /// </summary>
    public async Task StopAsync()
    {
        try
        {
            if (_mqttServer != null)
            {
                await _mqttServer.StopAsync();
                _isRunning = false;
                _logger.LogInformation("MQTT Broker stopped");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping MQTT broker");
            throw;
        }
    }

    /// <summary>
    /// Publish a message to a specific topic
    /// </summary>
    public async Task PublishAsync(string topic, string payload, bool retainFlag = false)
    {
        try
        {
            if (_mqttServer == null || !_isRunning)
            {
                _logger.LogWarning("MQTT Broker not running, cannot publish to {Topic}", topic);
                return;
            }

            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(retainFlag)
                .Build();

            await _mqttServer.InjectApplicationMessage(
                new InjectedMqttApplicationMessage(applicationMessage)
                {
                    SenderClientId = "ServerPublisher"
                });

            _logger.LogDebug("Published message to topic {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to {Topic}", topic);
        }
    }

    public void Dispose()
    {
        _mqttServer?.Dispose();
    }
}
