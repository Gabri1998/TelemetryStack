// Namespace defines the logical group this class belongs to.
// This helps organize large applications.
namespace ApiGateway.Models;

// "public" means this class can be used anywhere in the project.
public class Device
{
    // Property representing the device unique identifier.
    // "string" is used for simplicity now (later we may use GUID).
    public required string Id { get; set; }

    // Property representing the device name.
    // Example: "Truck-1", "Sensor-42", etc.
    public required string Name { get; set; }
}