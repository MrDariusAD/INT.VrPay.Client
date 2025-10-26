using FluentAssertions;
using INT.VrPay.Client.Models;
using INT.VrPay.Client.Services;

namespace INT.VrPay.Client.Tests.Services;

public class ResultCodeAnalyzerTests
{
    [Theory]
    [InlineData("000.000.000", true)]
    [InlineData("000.100.110", true)]
    [InlineData("000.100.111", true)]
    [InlineData("000.100.112", true)]
    [InlineData("000.300.000", true)]
    [InlineData("000.310.100", true)]
    [InlineData("000.600.000", true)]
    [InlineData("000.400.120", true)]
    [InlineData("800.100.153", false)]
    [InlineData("800.700.100", false)]
    [InlineData("100.100.100", false)]
    public void IsSuccess_ShouldReturnCorrectResult(string resultCode, bool expectedResult)
    {
        // Act
        var result = ResultCodeAnalyzer.IsSuccess(resultCode);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("000.400.000", true)]
    [InlineData("000.400.010", true)]
    [InlineData("000.400.020", true)]
    [InlineData("000.400.100", true)]
    [InlineData("000.400.030", false)] // Exception - ends with 3
    [InlineData("000.000.000", false)]
    [InlineData("800.100.153", false)]
    public void RequiresManualReview_ShouldReturnCorrectResult(string resultCode, bool expectedResult)
    {
        // Act
        var result = ResultCodeAnalyzer.RequiresManualReview(resultCode);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("000.000.000", TransactionStatus.Success)]
    [InlineData("000.100.110", TransactionStatus.Success)]
    [InlineData("000.400.000", TransactionStatus.SuccessManualReview)]
    [InlineData("000.400.010", TransactionStatus.SuccessManualReview)]
    [InlineData("000.200.000", TransactionStatus.Pending)]
    [InlineData("300.100.100", TransactionStatus.SoftDecline)]
    [InlineData("800.100.153", TransactionStatus.HardDecline)]
    [InlineData("800.700.100", TransactionStatus.HardDecline)]
    [InlineData("100.100.100", TransactionStatus.ValidationError)]
    [InlineData("200.100.100", TransactionStatus.ValidationError)]
    [InlineData("000.400.300", TransactionStatus.CommunicationError)]
    [InlineData("900.100.100", TransactionStatus.CommunicationError)]
    [InlineData("800.800.800", TransactionStatus.SystemError)]
    [InlineData("999.999.999", TransactionStatus.SystemError)]
    public void GetStatus_ShouldReturnCorrectStatus(string resultCode, TransactionStatus expectedStatus)
    {
        // Act
        var status = ResultCodeAnalyzer.GetStatus(resultCode);

        // Assert
        status.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData("300.100.100", true)]  // Soft decline
    [InlineData("000.400.300", true)]  // Communication error
    [InlineData("900.100.100", true)]  // Communication error
    [InlineData("800.800.800", true)]  // System error
    [InlineData("800.100.153", false)] // Hard decline
    [InlineData("000.000.000", false)] // Success
    [InlineData("100.100.100", false)] // Validation error
    public void CanRetry_ShouldReturnCorrectResult(string resultCode, bool expectedResult)
    {
        // Act
        var result = ResultCodeAnalyzer.CanRetry(resultCode);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void GetStatus_WithNullOrEmpty_ShouldReturnUnknown()
    {
        // Act & Assert
        ResultCodeAnalyzer.GetStatus(null!).Should().Be(TransactionStatus.Unknown);
        ResultCodeAnalyzer.GetStatus(string.Empty).Should().Be(TransactionStatus.Unknown);
        ResultCodeAnalyzer.GetStatus("   ").Should().Be(TransactionStatus.Unknown);
    }
}
