using MassTransit;
using NotificationsAPI.Events;
using NotificationsAPI.Services;

namespace NotificationsAPI.Consumers;

public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly NotificationService _notificationService;

    public UserCreatedConsumer(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var message = context.Message;

        _notificationService.SendWelcomeEmail(message.Email, message.UserId.ToString());

        return Task.CompletedTask;
    }
}
