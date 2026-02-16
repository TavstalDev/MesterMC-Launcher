using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Tavstal.MesterMC.Api.Models;

namespace Tavstal.MesterMC.Api.Services;

/// <summary>
/// Service responsible for sending emails using SMTP.
/// </summary>
public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly Settings _settings;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration instance.</param>
    /// <param name="logger">The logger instance for logging messages.</param>
    /// <param name="settings">The settings containing email configuration details.</param>
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, Settings settings)
    {
        _configuration = configuration;
        _logger = logger;
        _settings = settings;
    }
    
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_settings.EmailAddress));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = body };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.EmailProvider,  _settings.EmailPort, SecureSocketOptions.None);
        try
        {
            await smtp.AuthenticateAsync(_settings.EmailAddress, _settings.EmailPassword);
        }
        catch (Exception)
        {
            // ignored, the smtp server might not require authentication, so we can ignore authentication failures
        }

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}