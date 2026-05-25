using FluentValidation;
using SoftOne.Api.DTOs.Requests;

namespace SoftOne.Api.Validators;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DueDate)
            .Must(dueDate => dueDate!.Value >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Due date cannot be in the past.")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Priority must be a valid value (Low, Medium, or High).")
            .When(x => x.Priority.HasValue);
    }
}
