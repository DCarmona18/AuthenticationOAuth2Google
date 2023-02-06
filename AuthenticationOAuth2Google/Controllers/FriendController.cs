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
    public class FriendController : ControllerBase
    {
        private readonly IFriendService _friendService;

        public FriendController(IFriendService friendService)
        {
            _friendService = friendService;
        }

        /// <summary>
        /// Gets all friends based on the logged user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<Friend>>> Get() 
        {
            try
            {
                return Ok(await _friendService.GetFriendsAndUnseenMessages());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Adds a friend request to the logged user
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Post(Friend friend) 
        {
            try
            {
                return Ok(await _friendService.AddFriendRequest(friend));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("requests")]
        public async Task<ActionResult> GetFriendRequest() 
        {
            try
            {
                return Ok(await _friendService.GetFriendRequests());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPut]
        public async Task<ActionResult<Friend>> AcceptFriendRequest([FromBody] FriendRequest friendRequest) 
        {
            try
            {
                return Ok(await _friendService.AcceptFriendRequest(friendRequest));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
