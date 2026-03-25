using DeviceService.Models;
using DeviceService.Repositories;

namespace DeviceService.Services;

public class DeviceManager
{
    private readonly DeviceRepository _repository;

    public DeviceManager(DeviceRepository repository)
    {
        _repository = repository;
    }

    public List<Device> GetDevices()
    {
        return _repository.GetDevices();
    }

    public void AddDevice(string name)
    {
        var device = new Device
        {
            Id = Guid.NewGuid().ToString(),
            Name = name
        };

        _repository.AddDevice(device);
    }
}