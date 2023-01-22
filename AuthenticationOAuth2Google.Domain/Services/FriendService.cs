using AuthenticationOAuth2Google.Domain.Interfaces;
using AuthenticationOAuth2Google.Domain.Models;
using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using AuthenticationOAuth2Google.Infrastructure.Interfaces;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationOAuth2Google.Domain.Services
{
    public class FriendService : IFriendService
    {
        private readonly IMongoDBRepository<UserEntity> _userRepository;
        private readonly IAuthenticationService _authenticationService;

        public FriendService(IMongoDBRepository<UserEntity> userRepository, IAuthenticationService authenticationService)
        {
            _userRepository = userRepository;
            _authenticationService = authenticationService;
        }
        public async Task<Friend> AddFriend(Friend friend)
        {
            // Validate email address exists
            var userFriend = _userRepository.GetBy(x => x.Email == friend.Email).FirstOrDefault();
            if (userFriend == null) return new Friend();

            var loggedUser = await _authenticationService.GetLoggedUser();
            var userEntity = await _userRepository.GetByIdAsync(loggedUser.Id);

            if (userEntity.Friends == null || !userEntity.Friends.Any(x => x.UserId == userFriend.Id))
            {
                // Get UserId
                var friendEntity = new FriendEntity
                {
                    UserId = userFriend.Id!,
                    CreationDate = DateTime.Now,
                };

                // Validate if the user has added the friend already if not add it to collection
                await _userRepository.AddToCollectionAsync(userEntity.Id, friendEntity, nameof(UserEntity.Friends));
                return new Friend 
                { 
                    Id = userFriend.Id!,
                    AvatarUrl = userFriend.AvatarUrl,
                    Email = userFriend.Email,
                    FullName = userFriend.FullName,
                    UserId = userFriend.Id
                };
            }

            return new Friend();
        }

        public async Task<List<Friend>> GetFriends()
        {
            var friendList = new List<Friend>();
            var loggedUser = await _authenticationService.GetLoggedUser();
            var userIdFriends = (await _userRepository.GetByIdAsync(loggedUser.Id))
                            .Friends.Select(x => x.UserId).ToList();

            return _userRepository
                .GetBy(x => userIdFriends.Contains(x.Id))
                .Select(x => new Friend {
                AvatarUrl = x.AvatarUrl,
                Email = x.Email,
                FullName = x.FullName,
                UserId = x.Id
            }).ToList();
        }
    }
}
