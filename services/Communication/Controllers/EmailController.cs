using Microsoft.AspNetCore.Mvc;
using CommunicationService.Services;

namespace CommunicationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailController> _logger;

        public EmailController(IEmailService emailService, ILogger<EmailController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Send a single email
        /// </summary>
        [HttpPost("send")]
        public async Task<ActionResult> SendEmail([FromBody] SendEmailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.To))
                return BadRequest(new { message = "Recipient email is required" });

            if (string.IsNullOrWhiteSpace(request.Subject))
                return BadRequest(new { message = "Subject is required" });

            if (string.IsNullOrWhiteSpace(request.Body))
                return BadRequest(new { message = "Email body is required" });

            var result = await _emailService.SendEmailAsync(
                request.To, 
                request.Subject, 
                request.Body, 
                request.IsHtml ?? true
            );

            if (!result.Success)
            {
                _logger.LogWarning($"Failed to send email to {request.To}: {result.ErrorMessage}");
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "Failed to send email", 
                    error = result.ErrorMessage 
                });
            }

            return Ok(new 
            { 
                success = true,
                message = "Email sent successfully", 
                messageId = result.MessageId 
            });
        }

        /// <summary>
        /// Send email to multiple recipients
        /// </summary>
        [HttpPost("send-multiple")]
        public async Task<ActionResult> SendEmailToMultiple([FromBody] SendMultipleEmailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.Recipients == null || !request.Recipients.Any())
                return BadRequest(new { message = "At least one recipient email is required" });

            if (string.IsNullOrWhiteSpace(request.Subject))
                return BadRequest(new { message = "Subject is required" });

            if (string.IsNullOrWhiteSpace(request.Body))
                return BadRequest(new { message = "Email body is required" });

            var result = await _emailService.SendEmailToMultipleAsync(
                request.Recipients, 
                request.Subject, 
                request.Body, 
                request.IsHtml ?? true
            );

            if (!result.Success)
            {
                _logger.LogWarning($"Failed to send email to {request.Recipients.Count} recipients: {result.ErrorMessage}");
                return StatusCode(500, new 
                { 
                    success = false,
                    message = "Failed to send email", 
                    error = result.ErrorMessage 
                });
            }

            return Ok(new 
            { 
                success = true,
                message = $"Email sent successfully to {request.Recipients.Count} recipients", 
                messageId = result.MessageId 
            });
        }
    }

    // Request Models
    public class SendEmailRequest
    {
        public string To { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public bool? IsHtml { get; set; } = true;
    }

    public class SendMultipleEmailRequest
    {
        public List<string> Recipients { get; set; } = new();
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public bool? IsHtml { get; set; } = true;
    }
}
