namespace ApiGateway.DTOs;

// DTO used for incoming requests
public class CreateDeviceDto
{
    // Name of the device (required)
    public required string Name { get; set; }
}