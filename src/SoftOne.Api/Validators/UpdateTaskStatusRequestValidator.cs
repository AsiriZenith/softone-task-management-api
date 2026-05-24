using FluentValidation;
using SoftOne.Api.Data.Enums;
using SoftOne.Api.DTOs.Requests;

namespace SoftOne.Api.Validators;

public class UpdateTaskStatusRequestValidator : AbstractValidator<UpdateTaskStatusRequest>
{
    public UpdateTaskStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .Must(status => Enum.IsDefined(typeof(Status), status))
            .WithMessage("Status must be a valid value (Todo, InProgress, Waiting, Completed, or Rejected).");
    }
}
