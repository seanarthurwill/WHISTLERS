using Microsoft.EntityFrameworkCore;
using CommunicationService.Data;
using CommunicationService.Models;

namespace CommunicationService.Services
{
    public interface ICommunicationService
    {
        Task<IEnumerable<Message>> GetAllMessagesAsync();
        Task<Message?> GetMessageByIdAsync(int messageId);
        Task<IEnumerable<Message>> GetMessagesByUserIdAsync(int userId);
        Task<IEnumerable<Message>> GetMessagesByGroupIdAsync(int groupId);
        Task<Message> CreateMessageAsync(Message message);
        Task<Message?> UpdateMessageAsync(int messageId, Message message);
        Task<bool> DeleteMessageAsync(int messageId);
        Task<bool> MarkMessageAsReadAsync(int messageId);
    }

    public class CommunicationService : ICommunicationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommunicationService> _logger;

        public CommunicationService(ApplicationDbContext context, ILogger<CommunicationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Message>> GetAllMessagesAsync()
        {
            return await _context.Messages
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<Message?> GetMessageByIdAsync(int messageId)
        {
            return await _context.Messages.FindAsync(messageId);
        }

        public async Task<IEnumerable<Message>> GetMessagesByUserIdAsync(int userId)
        {
            return await _context.Messages
                .Where(m => m.RecipientUserId == userId || m.SenderUserId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetMessagesByGroupIdAsync(int groupId)
        {
            return await _context.Messages
                .Where(m => m.RecipientGroupId == groupId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<Message> CreateMessageAsync(Message message)
        {
            message.SentAt = DateTime.UtcNow;
            message.IsRead = false;
            
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Message {message.MessageId} created by user {message.SenderUserId}");
            return message;
        }

        public async Task<Message?> UpdateMessageAsync(int messageId, Message message)
        {
            var existingMessage = await _context.Messages.FindAsync(messageId);
            if (existingMessage == null)
                return null;

            existingMessage.Subject = message.Subject;
            existingMessage.MessageBody = message.MessageBody;
            existingMessage.IsRead = message.IsRead;
            existingMessage.ReadAt = message.ReadAt;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Message {messageId} updated");
            return existingMessage;
        }

        public async Task<bool> DeleteMessageAsync(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
                return false;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Message {messageId} deleted");
            return true;
        }

        public async Task<bool> MarkMessageAsReadAsync(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
                return false;

            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Message {messageId} marked as read");
            return true;
        }
    }
}
