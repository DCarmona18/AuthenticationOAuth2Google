using AuthenticationOAuth2Google.Domain.Interfaces;
using AuthenticationOAuth2Google.Domain.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationOAuth2Google.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatHubController : ControllerBase
    {
        private readonly IChatHubService _chatHubService;

        public ChatHubController(IChatHubService chatHubService)
        {
            _chatHubService = chatHubService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConnectedUser>>> GetConnectedUsersAsync() 
        {
            try
            {
                return Ok(await _chatHubService.GetConnectedUsers());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
