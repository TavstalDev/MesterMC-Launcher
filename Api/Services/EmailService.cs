using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Tavstal.MesterMC.Api.Models;

namespace Tavstal.MesterMC.Api.Services;

/// <summary>
/// Service responsible for sending emails using SMTP.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<EmailService> _logger;
    private readonly Settings _settings;
    private string _emailBlankDoc = string.Empty;
    private string _emailActionDoc = string.Empty;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class.
    /// </summary>
    /// <param name="environment">The web host environment used to access application-specific paths.</param>
    /// <param name="logger">The logger instance for logging messages.</param>
    /// <param name="settings">The settings containing email configuration details.</param>
    public EmailService(IWebHostEnvironment environment, ILogger<EmailService> logger, Settings settings)
    {
        _environment = environment;
        _logger = logger;
        _settings = settings;
        // Async initialization
        Task.Run(async () => await InitAsync());
    }

    private async Task InitAsync()
    {
        string templateDir = Path.Combine(_environment.WebRootPath, "templates");
        if (!Directory.Exists(templateDir))
        {
            _logger.LogError("Email template directory not found at path: {Path}", templateDir);
            return;
        }
        
        string templatePath = Path.Combine(templateDir, "emailBlank.html");
        if (!File.Exists(templatePath))
        {
            _logger.LogError("Email document not found at path: {Path}", templatePath);
            return;
        }
        _emailBlankDoc = await File.ReadAllTextAsync(templatePath);
        
        templatePath = Path.Combine(templateDir, "emailAction.html");
        if (!File.Exists(templatePath))
        {
            _logger.LogError("Email document not found at path: {Path}", templatePath);
            return;
        }
        _emailActionDoc = await File.ReadAllTextAsync(templatePath);
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

    public async Task SendEmailAsync(string to, string username, string subject, string body)
    {
        string finalBody = _emailBlankDoc.Replace("{{TITLE}}", subject)
            .Replace("{{MESSAGE_BODY}}", body)
            .Replace("{{USERNAME}}", username);
        await SendEmailAsync(to, subject, finalBody);
    }
    
    public async Task SendEmailAsync(string to, string username, string subject, string body, string actionUrl,
        string buttonText)
    {
        string finalBody = _emailActionDoc.Replace("{{TITLE}}", subject)
            .Replace("{{MESSAGE_BODY}}", body)
            .Replace("{{USERNAME}}", username)
            .Replace("{{ACTION_URL}}", actionUrl)
            .Replace("{{BUTTON_TEXT}}", buttonText);
        await SendEmailAsync(to, subject, finalBody);
    }
}