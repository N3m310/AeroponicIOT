using AeroponicIOT.DTOs;

namespace AeroponicIOT.Services.Sensors;

public interface ISensorIngestionService
{
    Task ProcessSensorDataAsync(SensorDataDto sensorData, CancellationToken cancellationToken = default);
}

