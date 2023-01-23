using AuthenticationOAuth2Google.Domain.Interfaces;
using AuthenticationOAuth2Google.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationOAuth2Google.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessagesService _messagesService;

        public MessagesController(IMessagesService messagesService)
        {
            _messagesService = messagesService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ChatMessage>>> Get([FromQuery] string with) 
        {
            try
            {
                return Ok(await _messagesService.GetChatMessages(with));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
