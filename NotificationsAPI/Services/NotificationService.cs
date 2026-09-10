namespace NotificationsAPI.Services;

public class NotificationService
{
    public Task SendWelcomeEmailAsync(string email, string userId)
    {
        Console.WriteLine($"[EMAIL] Bem-vindo enviado para usuário {userId} e email {email}");

        return Task.CompletedTask;
    }

    public Task SendPurchaseConfirmationAsync(string userId)
    {
        Console.WriteLine($"[EMAIL] Compra confirmada para usuário {userId}");

        return Task.CompletedTask;
    }

    public Task SendPurchaseRejectionAsync(string userId, string status, string? reason)
    {
        Console.WriteLine(
            $"[EMAIL] Compra rejeitada para usuário {userId} com status {status}. Motivo: {reason ?? "Não informado"}");

        return Task.CompletedTask;
    }
}
