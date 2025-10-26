using INT.VrPay.Client;
using INT.VrPay.Client.Models;
using INT.VrPay.Client.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using INT.VrPay.Client.Extensions;

// Setup
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
services.AddVrPayClient(config =>
{
    config.BaseUrl = "https://test.vr-pay-ecommerce.de/";
    config.EntityId = "8a8294174e735d0c014e78beb6b9154b";
    config.AccessToken = "Bearer OGE4Mjk0MTc0ZTczNWQwYzAxNGU3OGJlYjZjNTE1NGZ8TnE3b0UlMmEyQldKcjQlJTZYZFk=";
    config.UseTestMode = true;
    config.TestModeValue = TestMode.External;
});

var serviceProvider = services.BuildServiceProvider();
var client = serviceProvider.GetRequiredService<IVrPayClient>();

Console.WriteLine("=== VrPay Client Test ===\n");

// Test 1: Successful Pre-Authorization
Console.WriteLine("Test 1: Pre-Authorization with valid card...");
try
{
    var request = TestData.CreateSuccessfulPaymentRequest(
        amount: 92.00m,
        currency: Currency.EUR,
        paymentBrand: PaymentBrand.Visa);
    request.MerchantTransactionId = $"CONSOLE-TEST-{Guid.NewGuid():N}";

    var response = await client.PreAuthorizeAsync(request);
    
    Console.WriteLine($"✓ Success! Transaction ID: {response.Id}");
    Console.WriteLine($"  Result Code: {response.Result.Code}");
    Console.WriteLine($"  Description: {response.Result.Description}");
    Console.WriteLine($"  Status: {response.GetStatus()}");
    Console.WriteLine($"  Amount: {response.Amount} {response.Currency}");
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Failed: {ex.Message}\n");
}

// Test 2: Declined Card
Console.WriteLine("Test 2: Pre-Authorization with declined card...");
try
{
    var request = TestData.CreateDeclinedPaymentRequest(
        amount: 50.00m,
        currency: Currency.EUR,
        paymentBrand: PaymentBrand.Visa);
    request.MerchantTransactionId = $"CONSOLE-TEST-DECLINED-{Guid.NewGuid():N}";

    var response = await client.PreAuthorizeAsync(request);
    Console.WriteLine($"✗ Should have been declined but wasn't! Transaction ID: {response.Id}\n");
}
catch (INT.VrPay.Client.Exceptions.VrPayPaymentDeclinedException ex)
{
    Console.WriteLine($"✓ Correctly declined!");
    Console.WriteLine($"  Result Code: {ex.ResultCode}");
    Console.WriteLine($"  Message: {ex.Message}");
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Unexpected error: {ex.Message}\n");
}

// Test 3: Test Cards
Console.WriteLine("Test 3: Testing predefined test cards...");
Console.WriteLine($"  Visa Success Card: {TestCards.VisaSuccess.Number}");
Console.WriteLine($"  Visa Declined Card: {TestCards.VisaDeclined.Number}");
Console.WriteLine($"  Mastercard Success Card: {TestCards.MastercardSuccess.Number}");
Console.WriteLine($"  Amex Success Card: {TestCards.AmexSuccess.Number}");
Console.WriteLine();

Console.WriteLine("=== Tests Complete ===");
