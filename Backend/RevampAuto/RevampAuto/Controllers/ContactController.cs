using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RevampAuto.DTOs;
using RevampAuto.Services;
using System.Threading.Tasks;

namespace RevampAuto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost]
        public async Task<IActionResult> SendContactMessage([FromBody] ContactMessageDto dto)
        {
            await _contactService.CreateContactMessageAsync(dto);
            return Ok(new { message = "Your message has been sent successfully" });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetAllMessages()
        {
            var messages = await _contactService.GetAllMessagesAsync();
            return Ok(messages);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadMessages()
        {
            var messages = await _contactService.GetUnreadMessagesAsync();
            return Ok(messages);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkMessageAsRead(int id)
        {
            var result = await _contactService.MarkMessageAsReadAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}