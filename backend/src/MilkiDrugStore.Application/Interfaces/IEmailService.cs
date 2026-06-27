namespace MilkiDrugStore.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);
    Task SendApprovalEmailAsync(string toEmail, string userName);
}
