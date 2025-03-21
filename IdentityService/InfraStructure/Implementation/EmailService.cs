using System.Net;
using System.Net.Mail;

namespace BN.PROJECT.IdentityService;
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly SmtpClient _smtpClient;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpClient = new SmtpClient
        {
            Host = _configuration["Email:Smtp:Host"],
            Port = int.Parse(_configuration["Email:Smtp:Port"]),
            //Credentials = new NetworkCredential(
            //    _configuration["Email:Smtp:Username"],
            //    _configuration["Email:Smtp:Password"]),
            //EnableSsl = true
        };
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["Email:From"]),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);

        await _smtpClient.SendMailAsync(mailMessage);
    }
}

