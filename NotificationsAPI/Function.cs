using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using NotificationsAPI.Events;
using NotificationsAPI.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace NotificationsAPI;

public class Function
{
    private const string DefaultUserCreatedQueue = "user-created";
    private const string DefaultPaymentProcessedQueue = "notification-payment-processed";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly NotificationService _notificationService;

    public Function()
        : this(new NotificationService())
    {
    }

    public Function(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<SQSBatchResponse> FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        ArgumentNullException.ThrowIfNull(sqsEvent);

        var failures = new List<SQSBatchResponse.BatchItemFailure>();

        foreach (var message in sqsEvent.Records)
        {
            try
            {
                await ProcessMessageAsync(message, context);
            }
            catch (Exception exception)
            {
                context.Logger.LogError(
                    $"Falha ao processar a mensagem {message.MessageId}: {exception}");

                failures.Add(new SQSBatchResponse.BatchItemFailure
                {
                    ItemIdentifier = message.MessageId
                });
            }
        }

        return new SQSBatchResponse(failures);
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        if (string.IsNullOrWhiteSpace(message.Body))
        {
            throw new InvalidOperationException("A mensagem SQS não possui conteúdo.");
        }

        var queueName = GetQueueName(message.EventSourceArn);
        var userCreatedQueue = GetQueueSetting("USER_CREATED_QUEUE", DefaultUserCreatedQueue);
        var paymentProcessedQueue = GetQueueSetting(
            "PAYMENT_PROCESSED_QUEUE",
            DefaultPaymentProcessedQueue);

        if (queueName.Equals(userCreatedQueue, StringComparison.OrdinalIgnoreCase))
        {
            var userCreated = Deserialize<UserCreatedEvent>(message.Body);

            await _notificationService.SendWelcomeEmailAsync(
                userCreated.Email,
                userCreated.UserId.ToString());

            context.Logger.LogInformation(
                $"UserCreatedEvent processado. MessageId: {message.MessageId}");

            return;
        }

        if (queueName.Equals(paymentProcessedQueue, StringComparison.OrdinalIgnoreCase))
        {
            var paymentProcessed = Deserialize<PaymentProcessedEvent>(message.Body);

            if (paymentProcessed.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                await _notificationService.SendPurchaseConfirmationAsync(paymentProcessed.UserId);
            }
            else
            {
                await _notificationService.SendPurchaseRejectionAsync(
                    paymentProcessed.UserId,
                    paymentProcessed.Status,
                    paymentProcessed.Reason);
            }

            context.Logger.LogInformation(
                $"PaymentProcessedEvent processado. MessageId: {message.MessageId}");

            return;
        }

        throw new InvalidOperationException(
            $"A fila '{queueName}' não está configurada para esta função.");
    }

    private static T Deserialize<T>(string body)
    {
        return JsonSerializer.Deserialize<T>(body, SerializerOptions)
            ?? throw new JsonException($"Não foi possível desserializar {typeof(T).Name}.");
    }

    private static string GetQueueName(string? eventSourceArn)
    {
        if (string.IsNullOrWhiteSpace(eventSourceArn))
        {
            throw new InvalidOperationException("O ARN da fila de origem não foi informado.");
        }

        return eventSourceArn.Split(':', StringSplitOptions.RemoveEmptyEntries).Last();
    }

    private static string GetQueueSetting(string variableName, string defaultValue)
    {
        return Environment.GetEnvironmentVariable(variableName) is { Length: > 0 } value
            ? value
            : defaultValue;
    }
}
