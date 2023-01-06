using AuthenticationOAuth2Google.Domain.Interfaces;
using AuthenticationOAuth2Google.Domain.Models;
using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AuthenticationOAuth2Google.Controllers
{

    public record SignUpDTO([Required] string AuthType);

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Post()
        {
            try
            {
                var user = await _authenticationService.GetLoggedUser();
                if (user != null)
                {
                    return Ok(user);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

            return BadRequest();
        }
    }
}
