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
    private const int MaxRetries = 2;
    private const int RetryDelayMs = 1000;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var email = _configuration.GetSection("Email");
        _smtpHost = email["Host"] ?? "smtp.gmail.com";
        _smtpPort = int.TryParse(email["Port"], out var port) ? port : 587;
        _useSsl = bool.TryParse(email["EnableSsl"], out var ssl) ? ssl : true;
        _senderName = email["SenderName"] ?? "Milki Drug Store";
        _senderEmail = email["SenderEmail"] ?? "noreply@milki.com";
        _username = email["Username"] ?? string.Empty;
        _password = email["Password"] ?? string.Empty;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
    {
        if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_password))
        {
            _logger.LogWarning("SMTP credentials not configured. Email to {To}: {Subject}", toEmail, subject);
            throw new InvalidOperationException("SMTP credentials are not configured. Please set Email:Username and Email:Password in configuration.");
        }

        int attempt = 0;
        while (attempt <= MaxRetries)
        {
            try
            {
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
                return;
            }
            catch (SmtpException ex) when (attempt < MaxRetries && IsTransientSmtpError(ex))
            {
                attempt++;
                _logger.LogWarning(ex, "Transient SMTP error while sending email to {To}: {Subject}. Retrying in {Delay}ms...", toEmail, subject, RetryDelayMs);
                await Task.Delay(RetryDelayMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}: {Subject}", toEmail, subject);
                throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
            }
        }
    }

    private static bool IsTransientSmtpError(SmtpException ex)
    {
        var status = ex.StatusCode;
        return status == SmtpStatusCode.GeneralFailure ||
               status == SmtpStatusCode.MailboxBusy ||
               status == SmtpStatusCode.MailboxUnavailable ||
               status == SmtpStatusCode.TransactionFailed ||
               ex.InnerException is IOException ||
               ex.InnerException is TimeoutException;
    }

    public async Task SendApprovalEmailAsync(string toEmail, string userName)
    {
        var subject = "Registration Approval Required";
        var body = $"A new user '{userName}' has registered and requires admin approval.";
        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
    {
        var subject = "Password Reset Request - Milki Drug Store";
        var body = $@"
            <html>
            <body style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; color: #333;"">
                <div style=""background: linear-gradient(135deg, #059669, #0d9488); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
                    <h2 style=""color: white; margin: 0;"">Milki Drug Store</h2>
                </div>
                <div style=""background: #f9fafb; padding: 30px; border: 1px solid #e5e7eb; border-top: none;"">
                    <p style=""font-size: 16px; margin-top: 0;"">Hello {userName},</p>
                    <p style=""font-size: 16px;"">We received a request to reset your password. Click the button below to set a new password:</p>
                    <div style=""text-align: center; margin: 30px 0;"">
                        <a href=""{resetLink}"" style=""background: linear-gradient(135deg, #059669, #0d9488); color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;"">Reset Password</a>
                    </div>
                    <p style=""font-size: 14px; color: #6b7280;"">This link will expire in <strong>1 hour</strong>. If you did not request a password reset, please ignore this email.</p>
                    <p style=""font-size: 14px; color: #6b7280;"">If the button above doesn't work, copy and paste this URL into your browser:<br><a href=""{resetLink}"" style=""color: #059669; word-break: break-all;"">{resetLink}</a></p>
                </div>
                <div style=""background: #f3f4f6; padding: 20px; border-radius: 0 0 10px 10px; text-align: center; font-size: 12px; color: #9ca3af;"">
                    Milki Drug Store Management System
                </div>
            </body>
            </html>";
        await SendEmailAsync(toEmail, subject, body, isHtml: true);
    }
}
