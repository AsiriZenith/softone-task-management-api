using FluentAssertions;
using SoftOne.Auth;
using SoftOne.Auth.Helpers;

namespace SoftOne.Auth.Tests.Helpers;

public class PasswordHasherTests
{
    private const string Password = "Admin@123";

    [Fact]
    public void HashPassword_ReturnsNonEmptyHash()
    {
        var hash = PasswordHasher.HashPassword(Password);

        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().StartWith("$2");
    }

    [Fact]
    public void HashPassword_ProducesDifferentHashesForSamePassword()
    {
        var hash1 = PasswordHasher.HashPassword(Password);
        var hash2 = PasswordHasher.HashPassword(Password);

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        var hash = PasswordHasher.HashPassword(Password);

        PasswordHasher.VerifyPassword(Password, hash).Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        var hash = PasswordHasher.HashPassword(Password);

        PasswordHasher.VerifyPassword("WrongPassword", hash).Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithStoredAssessmentHash_ReturnsTrue()
    {
        PasswordHasher.VerifyPassword(Password, AuthCredentials.PasswordHash).Should().BeTrue();
    }
}
