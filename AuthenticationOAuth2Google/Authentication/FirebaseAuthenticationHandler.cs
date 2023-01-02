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

        public FirebaseAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            FirebaseApp firebaseApp,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            _firebaseApp = firebaseApp;
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

                return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new List<ClaimsIdentity>()
            {
                new ClaimsIdentity(ToClaims(firebaseToken.Claims), nameof(FirebaseAuthenticationHandler))
            }), JwtBearerDefaults.AuthenticationScheme));
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex);
            }
        }

        private IEnumerable<Claim>? ToClaims(IReadOnlyDictionary<string, object> claims)
        {
            return new List<Claim> 
            { 
                new Claim("id", claims["user_id"].ToString()!),
                new Claim("email", claims["email"].ToString()!),
                new Claim("name", claims["name"].ToString()!),
            };
        }
    }
}
