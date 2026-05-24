using FluentAssertions;
using FluentValidation.TestHelper;
using SoftOne.Api.Data.Enums;
using SoftOne.Api.DTOs.Requests;
using SoftOne.Api.Validators;
using Xunit;

namespace SoftOne.Api.Tests.Validators;

public class UpdateTaskStatusRequestValidatorTests
{
    private readonly UpdateTaskStatusRequestValidator _validator = new();

    [Theory]
    [InlineData(Status.Todo)]
    [InlineData(Status.InProgress)]
    [InlineData(Status.Waiting)]
    [InlineData(Status.Completed)]
    [InlineData(Status.Rejected)]
    public void Validate_ValidStatus_NoErrors(Status status)
    {
        var request = new UpdateTaskStatusRequest { Status = status };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_InvalidStatus_HasError()
    {
        var request = new UpdateTaskStatusRequest { Status = (Status)99 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Status);
    }
}
