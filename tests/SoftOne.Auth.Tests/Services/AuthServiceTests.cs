using FluentAssertions;
using SoftOne.Auth;
using SoftOne.Auth.Services;

namespace SoftOne.Auth.Tests.Services;

public class AuthServiceTests
{
    private const string ValidPassword = "Admin@123";
    private readonly AuthService _sut = new();

    [Fact]
    public void ValidateCredentials_WithValidCredentials_ReturnsAuthenticatedResult()
    {
        var result = _sut.ValidateCredentials(AuthCredentials.Username, ValidPassword);

        result.IsAuthenticated.Should().BeTrue();
        result.Username.Should().Be(AuthCredentials.Username);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ValidateCredentials_WithInvalidUsername_ReturnsFailure()
    {
        var result = _sut.ValidateCredentials("wronguser", ValidPassword);

        result.IsAuthenticated.Should().BeFalse();
        result.Username.Should().BeNull();
        result.ErrorMessage.Should().Be("Invalid username or password.");
    }

    [Fact]
    public void ValidateCredentials_WithInvalidPassword_ReturnsFailure()
    {
        var result = _sut.ValidateCredentials(AuthCredentials.Username, "WrongPassword");

        result.IsAuthenticated.Should().BeFalse();
        result.Username.Should().BeNull();
        result.ErrorMessage.Should().Be("Invalid username or password.");
    }

    [Fact]
    public void ValidateCredentials_WithEmptyUsername_ReturnsFailure()
    {
        var result = _sut.ValidateCredentials(string.Empty, ValidPassword);

        result.IsAuthenticated.Should().BeFalse();
        result.ErrorMessage.Should().Be("Username is required.");
    }

    [Fact]
    public void ValidateCredentials_WithEmptyPassword_ReturnsFailure()
    {
        var result = _sut.ValidateCredentials(AuthCredentials.Username, string.Empty);

        result.IsAuthenticated.Should().BeFalse();
        result.ErrorMessage.Should().Be("Password is required.");
    }

    [Fact]
    public void ValidateCredentials_WithWhitespaceUsername_ReturnsFailure()
    {
        var result = _sut.ValidateCredentials("   ", ValidPassword);

        result.IsAuthenticated.Should().BeFalse();
        result.ErrorMessage.Should().Be("Username is required.");
    }
}
