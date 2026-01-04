using Microsoft.AspNetCore.Mvc;
using CommunicationService.Models;
using CommunicationService.Services;

namespace CommunicationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunicationController : ControllerBase
    {
        private readonly ICommunicationService _communicationService;
        private readonly ILogger<CommunicationController> _logger;

        public CommunicationController(
            ICommunicationService communicationService,
            ILogger<CommunicationController> logger)
        {
            _communicationService = communicationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetAllMessages()
        {
            var messages = await _communicationService.GetAllMessagesAsync();
            return Ok(messages);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetMessage(int id)
        {
            var message = await _communicationService.GetMessageByIdAsync(id);
            if (message == null)
                return NotFound(new { message = "Message not found" });

            return Ok(message);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessagesByUser(int userId)
        {
            var messages = await _communicationService.GetMessagesByUserIdAsync(userId);
            return Ok(messages);
        }

        [HttpGet("group/{groupId}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessagesByGroup(int groupId)
        {
            var messages = await _communicationService.GetMessagesByGroupIdAsync(groupId);
            return Ok(messages);
        }

        [HttpPost]
        public async Task<ActionResult<Message>> CreateMessage([FromBody] Message message)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdMessage = await _communicationService.CreateMessageAsync(message);
            return CreatedAtAction(nameof(GetMessage), new { id = createdMessage.MessageId }, createdMessage);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Message>> UpdateMessage(int id, [FromBody] Message message)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedMessage = await _communicationService.UpdateMessageAsync(id, message);
            if (updatedMessage == null)
                return NotFound(new { message = "Message not found" });

            return Ok(updatedMessage);
        }

        [HttpPatch("{id}/read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            var result = await _communicationService.MarkMessageAsReadAsync(id);
            if (!result)
                return NotFound(new { message = "Message not found" });

            return Ok(new { message = "Message marked as read" });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var result = await _communicationService.DeleteMessageAsync(id);
            if (!result)
                return NotFound(new { message = "Message not found" });

            return Ok(new { message = "Message deleted successfully" });
        }
    }
}
