using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AuthenticationOAuth2Google.Controllers
{

    public record GoogleAuthDTO([Required] string GoogleToken);

    [Route("api/[controller]")]
    [ApiController]
    public class GoogleAuthController : ControllerBase
    {
        public GoogleAuthController()
        {

        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Post()
        {
            try
            {
                var data = HttpContext.User;
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
            
        }
    }
}
