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
        public async Task<ActionResult> Post([FromBody] SignUpDTO signUpDTO)
        {
            try
            {
                AUTH_TYPE parsedAuthType;
                if (Enum.TryParse<AUTH_TYPE>(signUpDTO.AuthType, true, out parsedAuthType))
                {
                    var data = new User
                    {
                        AuthType = parsedAuthType
                    };

                    var user = await _authenticationService.CreateUserAsync(data);
                    if (user != null)
                    {
                        return Ok(user);
                    }
                }

                return BadRequest($"{nameof(AUTH_TYPE)} does not match to registered types.");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

            return BadRequest();
            
        }
    }
}
