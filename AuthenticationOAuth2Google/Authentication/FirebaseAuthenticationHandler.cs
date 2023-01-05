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

            string token = bearerToken.Substring(BEARER_PREFIX.Length);

            try
            {
                FirebaseToken firebaseToken = await FirebaseAuth.GetAuth(_firebaseApp).VerifyIdTokenAsync(token);

                // TODO: Check user against the database
                var user = (await _mongoDBRepository.GetAsync()).FirstOrDefault(x => x.Email == firebaseToken.Claims["email"].ToString());
                if (user == null) 
                {
                    var userEntity = new UserEntity()
                    {
                        CreationDate = DateTime.Now,
                        Enabled = true,
                        Email = firebaseToken.Claims["email"].ToString()!,
                        Fullname = firebaseToken.Claims["name"].ToString()!,
                        Role = ClaimsConstants.DEFAULT_ROLE,
                        AvatarUrl = firebaseToken.Claims["picture"].ToString()!,
                        OAuthId = firebaseToken.Claims["user_id"].ToString()!
                    };
                    user = await _mongoDBRepository.CreateAsync(userEntity);
                }

                return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new List<ClaimsIdentity>()
            {
                new ClaimsIdentity(ToClaims(firebaseToken.Claims, user), nameof(FirebaseAuthenticationHandler))
            }), JwtBearerDefaults.AuthenticationScheme));
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
                new Claim("id", user.OAuthId!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.Fullname!),
                new Claim(ClaimTypes.Role, user.Role)
            };
        }
    }
}
