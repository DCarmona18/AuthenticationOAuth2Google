using AuthenticationOAuth2Google.Controllers;
using AuthenticationOAuth2Google.Domain.Constants;
using AuthenticationOAuth2Google.Domain.Models;
using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using AuthenticationOAuth2Google.Infrastructure.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace AuthenticationOAuth2Google.Authentication
{
    public class FirebaseAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string BEARER_PREFIX = "Bearer ";
        private readonly FirebaseApp _firebaseApp;
        private readonly IMongoDBRepository<UserEntity> _mongoDBRepository;
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public FirebaseAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            FirebaseApp firebaseApp,
            IMongoDBRepository<UserEntity> mongoDBRepository,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            _firebaseApp = firebaseApp;
            _mongoDBRepository = mongoDBRepository;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Context.Request.Headers.ContainsKey("Authorization")) 
            {
                return AuthenticateResult.NoResult();
            }

            string bearerToken = Context.Request.Headers["Authorization"]!;

            if( bearerToken == null || !bearerToken.StartsWith(BEARER_PREFIX)) 
            {
                return AuthenticateResult.Fail("Invalid scheme");
            }

            string authType = string.Empty;
            if (Context.Request.Headers.ContainsKey("x-auth-type")) 
            {
                authType = Context.Request.Headers["x-auth-type"]!;
            }

            string token = bearerToken.Substring(BEARER_PREFIX.Length);

            try
            {
                FirebaseToken firebaseToken = await FirebaseAuth.GetAuth(_firebaseApp).VerifyIdTokenAsync(token);
                //Instantiate a Singleton of the Semaphore with a value of 1. This means that only 1 thread can be granted access at a time.
                //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
                // Only lock the thread when the user is trying to login
                if (authType != "") 
                    await semaphoreSlim.WaitAsync();
                
                try
                {
                    var user = (await _mongoDBRepository.GetAsync()).FirstOrDefault(x => x.Email == firebaseToken.Claims["email"].ToString());
                    if (user == null)
                    {
                        AUTH_TYPE parsedAuthType;
                        if (!Enum.TryParse(authType, true, out parsedAuthType))
                        {
                            return AuthenticateResult.Fail("Authentication type not valid");
                        }

                        var userEntity = new UserEntity()
                        {
                            CreationDate = DateTime.Now,
                            Enabled = true,
                            Email = firebaseToken.Claims["email"].ToString()!,
                            FullName = firebaseToken.Claims["name"].ToString()!,
                            Role = ClaimsConstants.DEFAULT_ROLE,
                            AvatarUrl = firebaseToken.Claims["picture"].ToString()!,
                            OAuthId = firebaseToken.Claims["user_id"].ToString()!,
                            AuthType = parsedAuthType
                        };
                        
                        user = await _mongoDBRepository.CreateAsync(userEntity);
                    }

                    return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new List<ClaimsIdentity>()
                    {
                        new ClaimsIdentity(ToClaims(firebaseToken.Claims, user), nameof(FirebaseAuthenticationHandler))
                    }), JwtBearerDefaults.AuthenticationScheme));
                }
                finally
                {
                    //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                    //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                    semaphoreSlim.Release();
                }

            
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex);
            }
        }

        private IEnumerable<Claim>? ToClaims(IReadOnlyDictionary<string, object> claims, UserEntity user)
        {
            return new List<Claim> 
            { 
                new Claim("OAuthId", user.OAuthId!),
                new Claim("internal_id", user.Id!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.FullName!),
                new Claim(ClaimTypes.Role, user.Role)
            };
        }
    }
}
