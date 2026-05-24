using SoftOne.Api.Data.Enums;

namespace SoftOne.Api.DTOs.Requests;

public class UpdateTaskStatusRequest
{
    public Status Status { get; set; }
}
