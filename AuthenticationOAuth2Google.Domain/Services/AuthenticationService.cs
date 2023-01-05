using AuthenticationOAuth2Google.Domain.Constants;
using AuthenticationOAuth2Google.Domain.Interfaces;
using AuthenticationOAuth2Google.Domain.Models;
using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using AuthenticationOAuth2Google.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

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
                if ((await _mongoDBRepository.GetAsync()).Any(x => x.Email == claimsUser.Claims.First(x => x.Type == ClaimsConstants.EMAIL).Value))
                    return new();

                var userEntity = new UserEntity()
                {
                    AuthType = user.AuthType,
                    CreationDate = DateTime.Now,
                    Enabled = true,
                    Email = claimsUser.Claims.First(x => x.Type == ClaimsConstants.EMAIL).Value,
                    Fullname = claimsUser.Claims.First(x => x.Type == ClaimsConstants.NAME).Value,
                    Role = ClaimsConstants.DEFAULT_ROLE
                };
                var result = await _mongoDBRepository.CreateAsync(userEntity);
                return new User { 
                    AuthType= result.AuthType,
                    CreationDate = result.CreationDate,
                    Email = result.Email,
                    AvatarUrl = result.AvatarUrl,
                    Fullname = result.Fullname,
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
    }
}
