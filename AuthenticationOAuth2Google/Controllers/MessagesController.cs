using AuthenticationOAuth2Google.Domain.Interfaces;
using AuthenticationOAuth2Google.Domain.Models;
using AuthenticationOAuth2Google.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AuthenticationOAuth2Google.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessagesService _messagesService;
        private readonly IChatHubService _chatHubService;
        private readonly IHubContext<ChatHub, IChatClient> _hubContext;

        public MessagesController(IMessagesService messagesService, IChatHubService chatHubService,IHubContext<ChatHub, IChatClient> hubContext)
        {
            _messagesService = messagesService;
            _chatHubService = chatHubService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<List<ChatMessage>>> Get([FromQuery] string with) 
        {
            try
            {
                return Ok((await _messagesService.GetChatMessages(with)).ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("Seen")]
        public async Task<ActionResult> Seen(Friend friend) 
        {
            try
            {
                var markAsSeenTask = _messagesService.MarkAsSeen(friend);
                var userConnectionsTask = _chatHubService.GetConnectionsForLoggedUser();

                await Task.WhenAll(markAsSeenTask, userConnectionsTask);

                if (userConnectionsTask.IsCompletedSuccessfully && userConnectionsTask.Result != null && userConnectionsTask.Result.Count() > 0)
                {
                    await _hubContext.Clients.Clients(userConnectionsTask.Result.Select(x => x.ConnectionId)).MarkAsSeen(friend.UserId);
                }
                // TODO: Fix return
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
