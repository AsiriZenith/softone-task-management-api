using FluentAssertions;
using SoftOne.Api.Data.Enums;
using SoftOne.Api.DTOs.Requests;
using SoftOne.Api.Validators;

namespace SoftOne.Api.Tests.Validators;

public class CreateTaskRequestValidatorTests
{
    private readonly CreateTaskRequestValidator _validator = new();

    [Fact]
    public void Validate_MissingTitle_ReturnsValidationError()
    {
        var request = new CreateTaskRequest { Title = string.Empty };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateTaskRequest.Title)
            && e.ErrorMessage == "Title is required.");
    }

    [Fact]
    public void Validate_TitleTooLong_ReturnsValidationError()
    {
        var request = new CreateTaskRequest { Title = new string('a', 101) };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateTaskRequest.Title)
            && e.ErrorMessage == "Title must not exceed 100 characters.");
    }

    [Fact]
    public void Validate_DescriptionTooLong_ReturnsValidationError()
    {
        var request = new CreateTaskRequest
        {
            Title = "Valid title",
            Description = new string('a', 501)
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateTaskRequest.Description)
            && e.ErrorMessage == "Description must not exceed 500 characters.");
    }

    [Fact]
    public void Validate_DueDateInPast_ReturnsValidationError()
    {
        var request = new CreateTaskRequest
        {
            Title = "Valid title",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1)
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateTaskRequest.DueDate)
            && e.ErrorMessage == "Due date cannot be in the past.");
    }

    [Fact]
    public void Validate_InvalidPriority_ReturnsValidationError()
    {
        var request = new CreateTaskRequest
        {
            Title = "Valid title",
            Priority = (TaskPriority)99
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == nameof(CreateTaskRequest.Priority)
            && e.ErrorMessage == "Priority must be a valid value (Low, Medium, or High).");
    }

    [Fact]
    public void Validate_ValidRequest_ReturnsNoErrors()
    {
        var request = new CreateTaskRequest
        {
            Title = "Complete assignment",
            Description = "Finish Phase 6 validation",
            Priority = TaskPriority.High,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7)
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
