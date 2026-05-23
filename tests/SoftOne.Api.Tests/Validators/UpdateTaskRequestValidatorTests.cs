using FluentAssertions;
using SoftOne.Api.Data.Enums;
using SoftOne.Api.DTOs.Requests;
using SoftOne.Api.Validators;

namespace SoftOne.Api.Tests.Validators;

public class UpdateTaskRequestValidatorTests
{
    private readonly UpdateTaskRequestValidator _validator = new();

    [Fact]
    public void Validate_MissingTitle_ReturnsValidationError()
    {
        var request = new UpdateTaskRequest { Title = string.Empty };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateTaskRequest.Title));
    }

    [Fact]
    public void Validate_InvalidPriority_ReturnsValidationError()
    {
        var request = new UpdateTaskRequest
        {
            Title = "Updated title",
            Priority = (TaskPriority)99
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateTaskRequest.Priority));
    }

    [Fact]
    public void Validate_ValidRequest_ReturnsNoErrors()
    {
        var request = new UpdateTaskRequest
        {
            Title = "Updated title",
            Description = "Updated description",
            Priority = TaskPriority.Medium
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
