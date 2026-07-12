namespace NotificationsAPI.Services;

public class NotificationService
{
    public void SendWelcomeEmail(string email, string userId)
    {
        Console.WriteLine($"[EMAIL] Bem-vindo enviado para usuário {userId} e email {email}");
    }

    public void SendPurchaseConfirmation(string userId)
    {
        Console.WriteLine($"[EMAIL] Compra confirmada para usuário {userId}");
    }

    public void SendPurchaseRejection(string userId, string status)
    {
        Console.WriteLine($"[EMAIL] Compra rejeitada para usuário {userId} com status {status}");
    }
}   