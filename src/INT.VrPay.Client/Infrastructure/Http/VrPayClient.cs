using System.Globalization;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using INT.VrPay.Client.Configuration;
using INT.VrPay.Client.Exceptions;
using INT.VrPay.Client.Extensions;
using INT.VrPay.Client.Models;

namespace INT.VrPay.Client;

/// <summary>
/// Implementation of the VrPay payment client.
/// </summary>
public class VrPayClient : IVrPayClient
{
    private readonly HttpClient _httpClient;
    private readonly VrPayConfiguration _configuration;
    private readonly ILogger<VrPayClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Creates a new VrPayClient.
    /// </summary>
    public VrPayClient(
        HttpClient httpClient,
        IOptions<VrPayConfiguration> configuration,
        ILogger<VrPayClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Validate configuration
        try
        {
            _configuration.Validate();
        }
        catch (InvalidOperationException ex)
        {
            throw new VrPayConfigurationException("Invalid VrPay configuration.", ex);
        }

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <inheritdoc/>
    public async Task<PaymentResponse> PreAuthorizeAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        request.PaymentType = PaymentType.PreAuthorization;
        return await ExecutePaymentAsync(request, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PaymentResponse> DebitAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        request.PaymentType = PaymentType.Debit;
        return await ExecutePaymentAsync(request, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PaymentResponse> CaptureAsync(string preAuthId, decimal amount, Currency currency, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(preAuthId))
        {
            throw new ArgumentException("Pre-authorization ID is required.", nameof(preAuthId));
        }

        var request = PaymentRequest.Create(amount, currency, PaymentType.Capture);
        return await ExecuteBackofficeOperationAsync(preAuthId, request, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PaymentResponse> RefundAsync(string captureId, decimal amount, Currency currency, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(captureId))
        {
            throw new ArgumentException("Capture ID is required.", nameof(captureId));
        }

        var request = PaymentRequest.Create(amount, currency, PaymentType.Refund);
        return await ExecuteBackofficeOperationAsync(captureId, request, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PaymentResponse> ReverseAsync(string preAuthId, decimal amount, Currency currency, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(preAuthId))
        {
            throw new ArgumentException("Pre-authorization ID is required.", nameof(preAuthId));
        }

        var request = new PaymentRequest
        {
            PaymentType = PaymentType.Reversal,
            Amount = amount.ToString("F2", CultureInfo.InvariantCulture),
            Currency = currency
        };

        return await ExecuteBackofficeOperationAsync(preAuthId, request, cancellationToken);
    }

    private async Task<PaymentResponse> ExecutePaymentAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        // Validate request
        ValidatePaymentRequest(request);

        // Set entity ID and test mode from configuration
        request.EntityId = _configuration.EntityId;
        if (_configuration.UseTestMode)
        {
            request.TestMode = _configuration.TestModeValue;
        }

        _logger.LogInformation(
            "Executing payment operation: Type={PaymentType}, Amount={Amount}, Currency={Currency}",
            request.PaymentType,
            request.Amount,
            request.Currency);

        // Build URL
        var url = $"{_configuration.BaseUrl.TrimEnd('/')}/v1/payments";

        // Send request
        var response = await SendRequestAsync(url, request, cancellationToken);

        // Log result
        _logger.LogInformation(
            "Payment operation completed: ResultCode={ResultCode}, Description={Description}",
            response.Result.Code,
            response.Result.Description);

        // Check if declined
        if (!response.IsSuccess())
        {
            var status = response.GetStatus();
            _logger.LogWarning(
                "Payment declined: Status={Status}, ResultCode={ResultCode}",
                status,
                response.Result.Code);

            throw new VrPayPaymentDeclinedException(
                $"Payment was declined: {response.Result.Description}",
                response.Result.Code,
                response);
        }

        return response;
    }

    private async Task<PaymentResponse> ExecuteBackofficeOperationAsync(
        string referenceId,
        PaymentRequest request,
        CancellationToken cancellationToken)
    {
        // Set entity ID and test mode from configuration
        request.EntityId = _configuration.EntityId;
        if (_configuration.UseTestMode)
        {
            request.TestMode = _configuration.TestModeValue;
        }

        _logger.LogInformation(
            "Executing backoffice operation: Type={PaymentType}, ReferenceId={ReferenceId}",
            request.PaymentType,
            referenceId);

        // Build URL with reference ID
        var url = $"{_configuration.BaseUrl.TrimEnd('/')}/v1/payments/{referenceId}";

        // Send request
        var response = await SendRequestAsync(url, request, cancellationToken);

        // Log result
        _logger.LogInformation(
            "Backoffice operation completed: ResultCode={ResultCode}",
            response.Result.Code);

        // Check if declined
        if (!response.IsSuccess())
        {
            throw new VrPayPaymentDeclinedException(
                $"Operation was declined: {response.Result.Description}",
                response.Result.Code,
                response);
        }

        return response;
    }

    private async Task<PaymentResponse> SendRequestAsync(
        string url,
        PaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Convert request to form data
            var formData = ConvertToFormData(request);

            // Create HTTP request
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(formData)
            };

            // Add authorization header
            httpRequest.Headers.Add("Authorization", _configuration.AccessToken);

            // Send request
            var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);

            // Read response content
            var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            // Check for HTTP errors
            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "HTTP error: StatusCode={StatusCode}, Content={Content}",
                    httpResponse.StatusCode,
                    content);

                throw new VrPayCommunicationException(
                    $"HTTP request failed with status {httpResponse.StatusCode}.",
                    (int)httpResponse.StatusCode);
            }

            // Deserialize response
            var response = JsonSerializer.Deserialize<PaymentResponse>(content, _jsonOptions);

            if (response == null)
            {
                throw new VrPayCommunicationException("Failed to deserialize response.");
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception occurred.");
            throw new VrPayCommunicationException("Failed to communicate with VrPay API.", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Request timed out.");
            throw new VrPayCommunicationException("Request to VrPay API timed out.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response.");
            throw new VrPayCommunicationException("Failed to parse response from VrPay API.", ex);
        }
    }

    private Dictionary<string, string> ConvertToFormData(PaymentRequest request)
    {
        var formData = new Dictionary<string, string>();

        // Add all properties as form fields
        if (!string.IsNullOrEmpty(request.EntityId))
            formData["entityId"] = request.EntityId;

        if (!string.IsNullOrEmpty(request.Amount))
            formData["amount"] = request.Amount;

        formData["currency"] = GetEnumMemberValue(request.Currency);
        formData["paymentType"] = GetEnumMemberValue(request.PaymentType);

        if (request.PaymentBrand.HasValue)
            formData["paymentBrand"] = GetEnumMemberValue(request.PaymentBrand.Value);

        if (!string.IsNullOrEmpty(request.MerchantTransactionId))
            formData["merchantTransactionId"] = request.MerchantTransactionId;

        if (!string.IsNullOrEmpty(request.ShopperResultUrl))
            formData["shopperResultUrl"] = request.ShopperResultUrl;

        if (request.TestMode.HasValue)
            formData["testMode"] = GetEnumMemberValue(request.TestMode.Value);

        // Add card data
        if (request.Card != null)
        {
            if (!string.IsNullOrEmpty(request.Card.Number))
                formData["card.number"] = request.Card.Number;

            if (!string.IsNullOrEmpty(request.Card.Holder))
                formData["card.holder"] = request.Card.Holder;

            if (!string.IsNullOrEmpty(request.Card.ExpiryMonth))
                formData["card.expiryMonth"] = request.Card.ExpiryMonth;

            if (!string.IsNullOrEmpty(request.Card.ExpiryYear))
                formData["card.expiryYear"] = request.Card.ExpiryYear;

            if (!string.IsNullOrEmpty(request.Card.Cvv))
                formData["card.cvv"] = request.Card.Cvv;
        }

        // Add customer data
        if (request.Customer != null)
        {
            if (!string.IsNullOrEmpty(request.Customer.GivenName))
                formData["customer.givenName"] = request.Customer.GivenName;

            if (!string.IsNullOrEmpty(request.Customer.Surname))
                formData["customer.surname"] = request.Customer.Surname;

            if (!string.IsNullOrEmpty(request.Customer.Email))
                formData["customer.email"] = request.Customer.Email;

            if (!string.IsNullOrEmpty(request.Customer.Ip))
                formData["customer.ip"] = request.Customer.Ip;

            if (!string.IsNullOrEmpty(request.Customer.MerchantCustomerId))
                formData["customer.merchantCustomerId"] = request.Customer.MerchantCustomerId;
        }

        // Add billing data
        if (request.Billing != null)
        {
            if (!string.IsNullOrEmpty(request.Billing.Street1))
                formData["billing.street1"] = request.Billing.Street1;

            if (!string.IsNullOrEmpty(request.Billing.Street2))
                formData["billing.street2"] = request.Billing.Street2;

            if (!string.IsNullOrEmpty(request.Billing.City))
                formData["billing.city"] = request.Billing.City;

            if (!string.IsNullOrEmpty(request.Billing.State))
                formData["billing.state"] = request.Billing.State;

            if (!string.IsNullOrEmpty(request.Billing.Postcode))
                formData["billing.postcode"] = request.Billing.Postcode;

            if (!string.IsNullOrEmpty(request.Billing.Country))
                formData["billing.country"] = request.Billing.Country;
        }

        return formData;
    }

    private void ValidatePaymentRequest(PaymentRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Amount))
        {
            errors.Add("Amount is required.");
        }
        else if (!decimal.TryParse(request.Amount, out var amount) || amount <= 0)
        {
            errors.Add("Amount must be a positive number.");
        }

        // Validate card data for PA and DB transactions
        if (request.PaymentType is PaymentType.PreAuthorization or PaymentType.Debit)
        {
            if (request.Card == null)
            {
                errors.Add("Card data is required for payment transactions.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.Card.Number))
                {
                    errors.Add("Card number is required.");
                }

                if (string.IsNullOrWhiteSpace(request.Card.ExpiryMonth))
                {
                    errors.Add("Card expiry month is required.");
                }

                if (string.IsNullOrWhiteSpace(request.Card.ExpiryYear))
                {
                    errors.Add("Card expiry year is required.");
                }
            }

            if (!request.PaymentBrand.HasValue)
            {
                errors.Add("Payment brand is required for payment transactions.");
            }
        }

        if (errors.Any())
        {
            _logger.LogWarning("Payment request validation failed: {Errors}", string.Join(", ", errors));
            throw new VrPayValidationException("Payment request validation failed.", errors);
        }
    }

    private static string GetEnumMemberValue<TEnum>(TEnum enumValue) where TEnum : struct, Enum
    {
        var type = typeof(TEnum);
        var memberInfo = type.GetMember(enumValue.ToString())[0];
        var enumMemberAttr = memberInfo.GetCustomAttribute<EnumMemberAttribute>();
        return enumMemberAttr?.Value ?? enumValue.ToString();
    }
}
