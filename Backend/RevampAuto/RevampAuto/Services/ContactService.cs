using Microsoft.EntityFrameworkCore;
using RevampAuto.Data;
using RevampAuto.DTOs;
using RevampAuto.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RevampAuto.Services
{
    public class ContactService : IContactService
    {
        private readonly ApplicationDbContext _context;

        public ContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateContactMessageAsync(ContactMessageDto dto)
        {
            var message = new ContactMessage
            {
                Name = dto.Name,
                Email = dto.Email,
                Message = dto.Message,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ContactMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ContactMessageDto>> GetAllMessagesAsync()
        {
            return await _context.ContactMessages
                .OrderByDescending(m => m.SentAt)
                .Select(m => new ContactMessageDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Email = m.Email,
                    Message = m.Message,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ContactMessageDto>> GetUnreadMessagesAsync()
        {
            return await _context.ContactMessages
                .Where(m => !m.IsRead)
                .OrderByDescending(m => m.SentAt)
                .Select(m => new ContactMessageDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Email = m.Email,
                    Message = m.Message,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead
                })
                .ToListAsync();
        }

        public async Task<bool> MarkMessageAsReadAsync(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message == null)
                return false;

            message.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}