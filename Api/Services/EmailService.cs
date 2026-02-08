using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Tavstal.MesterMC.Api.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_configuration.GetValue<string>("Email:Address")));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = body };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_configuration.GetValue<string>("Email:Provider"),  _configuration.GetValue<int>("Email:Port"), SecureSocketOptions.None);
        await smtp.AuthenticateAsync(_configuration.GetValue<string>("Email:Address"), _configuration.GetValue<string>("Email:Password"));
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}