using AuthenticationOAuth2Google.Domain.Constants;
using AuthenticationOAuth2Google.Domain.Interfaces;
using AuthenticationOAuth2Google.Domain.Models;
using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using AuthenticationOAuth2Google.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver.Linq;
using System.Security.Claims;

namespace AuthenticationOAuth2Google.Domain.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IMongoDBRepository<UserEntity> _mongoDBRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationService(IMongoDBRepository<UserEntity> mongoDBRepository, IHttpContextAccessor httpContextAccessor )
        {
            _mongoDBRepository = mongoDBRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                var claimsUser = _httpContextAccessor.HttpContext.User;
                if (await _mongoDBRepository.GetByIdAsync(claimsUser.Claims.First(x => x.Type == "internal_id").Value) == null)
                    return new();

                var userEntity = new UserEntity()
                {
                    AuthType = user.AuthType,
                    CreationDate = DateTime.Now,
                    Enabled = true,
                    Email = claimsUser.Claims.First(x => x.Type == ClaimTypes.Email).Value,
                    FullName = claimsUser.Claims.First(x => x.Type == ClaimTypes.GivenName).Value,
                    Role = ClaimsConstants.DEFAULT_ROLE
                };
                var result = await _mongoDBRepository.CreateAsync(userEntity);
                return new User { 
                    AuthType= result.AuthType,
                    CreationDate = result.CreationDate,
                    Email = result.Email,
                    AvatarUrl = result.AvatarUrl,
                    FullName = result.FullName,
                    Role = result.Role,
                    Id= result.Id,
                    Username = result.Username,
                    Password = string.Empty
                };
            }
            catch
            {
                return new();
            }
        }

        public async Task<User?> GetLoggedUser()
        {
            var email = _httpContextAccessor.HttpContext.User.Claims.First(x => x.Type == ClaimTypes.Email).Value;
            var internalId = _httpContextAccessor.HttpContext.User.Claims.First(x => x.Type == "internal_id").Value;
            var loggedUser = await _mongoDBRepository.GetByIdAsync(internalId);
            if (loggedUser != null)
            {
                return new User { 
                    Id = loggedUser.Id,
                    AuthType = loggedUser.AuthType,
                    AvatarUrl = loggedUser.AvatarUrl,
                    FullName = loggedUser.FullName,
                    Role = loggedUser.Role,
                    CreationDate = loggedUser.CreationDate,
                    Email = loggedUser.Email,
                    Enabled = loggedUser.Enabled,
                    Username = loggedUser.Username
                };
            }

            return null;
        }
    }
}
