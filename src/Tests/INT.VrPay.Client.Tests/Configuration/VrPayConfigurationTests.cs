using FluentAssertions;
using INT.VrPay.Client.Configuration;
using INT.VrPay.Client.Models;

namespace INT.VrPay.Client.Tests.Configuration;

public class VrPayConfigurationTests
{
    [Fact]
    public void Validate_WithValidConfiguration_ShouldNotThrow()
    {
        // Arrange
        var config = new VrPayConfiguration
        {
            BaseUrl = "https://test.vr-pay-ecommerce.de/",
            EntityId = "test-entity-id",
            AccessToken = "Bearer test-token",
            TimeoutSeconds = 30
        };

        // Act
        var act = () => config.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("", "test-entity", "Bearer token")]
    [InlineData("   ", "test-entity", "Bearer token")]
    public void Validate_WithMissingBaseUrl_ShouldThrow(string baseUrl, string entityId, string accessToken)
    {
        // Arrange
        var config = new VrPayConfiguration
        {
            BaseUrl = baseUrl,
            EntityId = entityId,
            AccessToken = accessToken
        };

        // Act
        var act = () => config.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("BaseUrl is required.");
    }

    [Theory]
    [InlineData("https://test.vr-pay-ecommerce.de/", "", "Bearer token")]
    [InlineData("https://test.vr-pay-ecommerce.de/", "   ", "Bearer token")]
    public void Validate_WithMissingEntityId_ShouldThrow(string baseUrl, string entityId, string accessToken)
    {
        // Arrange
        var config = new VrPayConfiguration
        {
            BaseUrl = baseUrl,
            EntityId = entityId,
            AccessToken = accessToken
        };

        // Act
        var act = () => config.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("EntityId is required.");
    }

    [Theory]
    [InlineData("https://test.vr-pay-ecommerce.de/", "test-entity", "")]
    [InlineData("https://test.vr-pay-ecommerce.de/", "test-entity", "   ")]
    public void Validate_WithMissingAccessToken_ShouldThrow(string baseUrl, string entityId, string accessToken)
    {
        // Arrange
        var config = new VrPayConfiguration
        {
            BaseUrl = baseUrl,
            EntityId = entityId,
            AccessToken = accessToken
        };

        // Act
        var act = () => config.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("AccessToken is required.");
    }

    [Fact]
    public void Validate_WithInvalidBaseUrl_ShouldThrow()
    {
        // Arrange
        var config = new VrPayConfiguration
        {
            BaseUrl = "not-a-valid-url",
            EntityId = "test-entity",
            AccessToken = "Bearer token"
        };

        // Act
        var act = () => config.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("BaseUrl must be a valid absolute URL.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_WithInvalidTimeout_ShouldThrow(int timeoutSeconds)
    {
        // Arrange
        var config = new VrPayConfiguration
        {
            BaseUrl = "https://test.vr-pay-ecommerce.de/",
            EntityId = "test-entity",
            AccessToken = "Bearer token",
            TimeoutSeconds = timeoutSeconds
        };

        // Act
        var act = () => config.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("TimeoutSeconds must be greater than 0.");
    }

    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var config = new VrPayConfiguration();

        // Assert
        config.BaseUrl.Should().Be("https://test.vr-pay-ecommerce.de/");
        config.EntityId.Should().BeEmpty();
        config.AccessToken.Should().BeEmpty();
        config.TimeoutSeconds.Should().Be(30);
        config.UseTestMode.Should().BeTrue();
        config.TestModeValue.Should().Be(TestMode.External);
    }
}
