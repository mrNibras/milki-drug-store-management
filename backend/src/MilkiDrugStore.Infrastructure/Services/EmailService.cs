using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MilkiDrugStore.Application.Interfaces;
using System.Net;
using System.Net.Mail;

namespace MilkiDrugStore.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly bool _useSsl;
    private readonly string _senderName;
    private readonly string _senderEmail;
    private readonly string _username;
    private readonly string _password;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var smtp = _configuration.GetSection("Smtp");
        _smtpHost = smtp["Host"] ?? "smtp.gmail.com";
        _smtpPort = int.TryParse(smtp["Port"], out var port) ? port : 587;
        _useSsl = bool.TryParse(smtp["UseSsl"], out var ssl) ? ssl : true;
        _senderName = smtp["SenderName"] ?? "Milki Drug Store";
        _senderEmail = smtp["SenderEmail"] ?? "noreply@milki.com";
        _username = smtp["Username"] ?? string.Empty;
        _password = smtp["Password"] ?? string.Empty;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password))
            {
                _logger.LogWarning("SMTP credentials not configured. Email to {To}: {Subject}", toEmail, subject);
                return;
            }

            using var message = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = _useSsl,
                Credentials = new NetworkCredential(_username, _password)
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {To}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}: {Subject}", toEmail, subject);
            throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
        }
    }

    public async Task SendApprovalEmailAsync(string toEmail, string userName)
    {
        var subject = "Registration Approval Required";
        var body = $"A new user '{userName}' has registered and requires admin approval.";
        await SendEmailAsync(toEmail, subject, body);
    }
}
