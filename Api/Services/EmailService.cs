using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Tavstal.MesterMC.Api.Models;

namespace Tavstal.MesterMC.Api.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly Settings _settings;
    
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, Settings settings)
    {
        _configuration = configuration;
        _logger = logger;
        _settings = settings;
    }
    
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