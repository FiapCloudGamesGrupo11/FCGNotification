using MassTransit;
using NotificationsAPI.Consumers;
using NotificationsAPI.Events;
using NotificationsAPI.Services;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<NotificationService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserCreatedConsumer>();
    x.AddConsumer<PaymentProcessedConsumer>();

    x.UsingRabbitMq(
        (context, cfg) =>
        {
            var rabbitMqSettings = context
                .GetRequiredService<IConfiguration>()
                .GetSection("RabbitMQ");

            var host = rabbitMqSettings["Host"];
            var username = rabbitMqSettings["Username"];
            var password = rabbitMqSettings["Password"];
            var userCreatedQueue = rabbitMqSettings["UserCreatedQueueName"];
            var paymentProcessedQueue = rabbitMqSettings["PaymentProcessedQueueName"];

            var logger = context
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("NotificationAPI.RabbitMQ");

            cfg.Host(
                host,
                "/",
                h =>
                {
                    h.Username(username);
                    h.Password(password);
                }
            );

            cfg.ReceiveEndpoint(
                userCreatedQueue,
                e =>
                {
                    e.UseRawJsonDeserializer(isDefault: true);
                    e.ConfigureConsumer<UserCreatedConsumer>(context);
                }
            );

            cfg.ReceiveEndpoint(
                paymentProcessedQueue,
                e =>
                {
                    e.UseRawJsonDeserializer(isDefault: true);

                    e.Bind(
                        "payment.exchange",
                        x =>
                        {
                            x.ExchangeType = ExchangeType.Fanout;
                        }
                    );

                    e.ConfigureConsumer<PaymentProcessedConsumer>(context);
                }
            );
        }
    );
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.MapGet(
    "/publish-user",
    async (IPublishEndpoint publish) =>
    {
        await publish.Publish(
            new UserCreatedEvent
            {
                UserId = Guid.NewGuid(),
                Name = "Esther",
                Email = "esther@teste.com",
            }
        );

        return Results.Ok("Evento UserCreated enviado!");
    }
);

app.MapGet(
    "/publish-payment",
    async (IPublishEndpoint publish) =>
    {
        await publish.Publish(
            new PaymentProcessedEvent
            {
                UserId = Guid.NewGuid(),
                GameId = Guid.NewGuid(),
                Price = 100,
                Status = "Approved",
            }
        );

        return Results.Ok("Evento PaymentProcessed enviado!");
    }
);

app.Run();
