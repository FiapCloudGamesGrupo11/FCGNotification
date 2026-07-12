using MassTransit;
using NotificationsAPI.Events;
using NotificationsAPI.Services;

namespace NotificationsAPI.Consumers;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly NotificationService _notificationService;

    public PaymentProcessedConsumer(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var payment = context.Message;

        if (payment.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
        {
            _notificationService.SendPurchaseConfirmation(payment.UserId.ToString());
        }
        else
        {
            _notificationService.SendPurchaseRejection(payment.UserId.ToString(), payment.Status);
        }

        return Task.CompletedTask;
    }
}
