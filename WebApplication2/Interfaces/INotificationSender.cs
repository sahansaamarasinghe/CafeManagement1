namespace WebApplication2.Interfaces
{
    public interface INotificationSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
