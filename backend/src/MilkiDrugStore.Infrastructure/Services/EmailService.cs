using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;

namespace MilkiDrugStore.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
    {
        _logger.LogInformation("Email to {To}: {Subject}", toEmail, subject);
        await Task.CompletedTask;
    }

    public async Task SendApprovalEmailAsync(string toEmail, string userName)
    {
        var subject = "Registration Approval Required";
        var body = $"A new user '{userName}' has registered and requires admin approval.";
        await SendEmailAsync(toEmail, subject, body);
    }
}
