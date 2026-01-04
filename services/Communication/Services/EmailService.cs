using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace CommunicationService.Services
{
    public interface IEmailService
    {
        Task<EmailResult> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<EmailResult> SendEmailToMultipleAsync(List<string> recipients, string subject, string body, bool isHtml = true);
    }

    public class EmailResult
    {
        public bool Success { get; set; }
        public string? MessageId { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class EmailService : IEmailService
    {
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IAmazonSimpleEmailService sesClient, 
            IConfiguration configuration, 
            ILogger<EmailService> logger)
        {
            _sesClient = sesClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<EmailResult> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var sender = _configuration["Email:SenderEmail"];
                
                if (string.IsNullOrWhiteSpace(sender))
                {
                    _logger.LogError("Sender email not configured in appsettings.json");
                    return new EmailResult 
                    { 
                        Success = false, 
                        ErrorMessage = "Sender email not configured" 
                    };
                }

                var sendRequest = new SendEmailRequest
                {
                    Source = sender,
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { to }
                    },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Html = isHtml ? new Content
                            {
                                Charset = "UTF-8",
                                Data = body
                            } : null,
                            Text = !isHtml ? new Content
                            {
                                Charset = "UTF-8",
                                Data = body
                            } : null
                        }
                    }
                };

                var response = await _sesClient.SendEmailAsync(sendRequest);
                _logger.LogInformation($"Email sent successfully to {to}. MessageId: {response.MessageId}");
                
                return new EmailResult 
                { 
                    Success = true, 
                    MessageId = response.MessageId 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {to}: {ex.Message}");
                return new EmailResult 
                { 
                    Success = false, 
                    ErrorMessage = ex.Message 
                };
            }
        }

        public async Task<EmailResult> SendEmailToMultipleAsync(List<string> recipients, string subject, string body, bool isHtml = true)
        {
            try
            {
                var sender = _configuration["Email:SenderEmail"];
                
                if (string.IsNullOrWhiteSpace(sender))
                {
                    _logger.LogError("Sender email not configured in appsettings.json");
                    return new EmailResult 
                    { 
                        Success = false, 
                        ErrorMessage = "Sender email not configured" 
                    };
                }

                var sendRequest = new SendEmailRequest
                {
                    Source = sender,
                    Destination = new Destination
                    {
                        ToAddresses = recipients
                    },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Html = isHtml ? new Content
                            {
                                Charset = "UTF-8",
                                Data = body
                            } : null,
                            Text = !isHtml ? new Content
                            {
                                Charset = "UTF-8",
                                Data = body
                            } : null
                        }
                    }
                };

                var response = await _sesClient.SendEmailAsync(sendRequest);
                _logger.LogInformation($"Email sent successfully to {recipients.Count} recipients. MessageId: {response.MessageId}");
                
                return new EmailResult 
                { 
                    Success = true, 
                    MessageId = response.MessageId 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to multiple recipients: {ex.Message}");
                return new EmailResult 
                { 
                    Success = false, 
                    ErrorMessage = ex.Message 
                };
            }
        }
    }
}
