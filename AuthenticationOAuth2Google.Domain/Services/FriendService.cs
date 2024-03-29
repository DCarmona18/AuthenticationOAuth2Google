﻿using AuthenticationOAuth2Google.Domain.Interfaces;
using AuthenticationOAuth2Google.Domain.Models;
using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using AuthenticationOAuth2Google.Infrastructure.Enums;
using AuthenticationOAuth2Google.Infrastructure.Interfaces;
using MongoDB.Driver;

namespace AuthenticationOAuth2Google.Domain.Services
{
    public class FriendService : IFriendService
    {
        private readonly IMongoDBRepository<UserEntity> _userRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMongoDBRepository<FriendRequestEntity> _friendRequestRepository;
        private readonly IMessagesService _messagesService;

        public FriendService(IMongoDBRepository<UserEntity> userRepository,
            IAuthenticationService authenticationService,
            IMongoDBRepository<FriendRequestEntity> friendRequestRepository,
            IMessagesService messagesService)
        {
            _userRepository = userRepository;
            _authenticationService = authenticationService;
            _friendRequestRepository = friendRequestRepository;
            _messagesService = messagesService;
        }
        public async Task<FriendRequest> AddFriendRequest(Friend friend)
        {
            // Validate email address exists
            var userFriend = _userRepository.GetBy(x => x.Email == friend.Email).FirstOrDefault();
            if (userFriend == null) return new FriendRequest();

            var loggedUser = await _authenticationService.GetLoggedUser();
            var userEntity = await _userRepository.GetByIdAsync(loggedUser.Id);
            var friendRequest = _friendRequestRepository.GetBy(x => 
                        x.SenderId == loggedUser.Id &&
                        x.ReceiverId == userFriend.Id &&
                        x.Status != UserRequestStatus.Accepted &&
                        x.Status != UserRequestStatus.Rejected)
                .FirstOrDefault();
            if ((userEntity.Friends == null || !userEntity.Friends!.Any(x => x.UserId == userFriend.Id)) && friendRequest == null)
            {
                // Get UserId
                var friendRequestEntity = new FriendRequestEntity
                {
                    ReceiverId = userFriend.Id,
                    SenderId = loggedUser.Id,
                    CreatedAt = DateTime.Now,
                    Status = UserRequestStatus.Pending
                };

                // Validate if the user has added the friend already if not add it to collection
                await _friendRequestRepository.CreateAsync(friendRequestEntity);
                return new FriendRequest 
                { 
                    Id = friendRequestEntity.Id,
                    AvatarUrl = loggedUser.AvatarUrl,
                    Name = loggedUser.FullName,
                    Status = friendRequestEntity.Status
                };
            }

            return new FriendRequest();
        }

        public async Task<List<Friend>> GetFriendsAndUnseenMessages()
        {
            var result = new List<Friend>();
            var loggedUser = await _authenticationService.GetLoggedUser();
            var userIdFriends = (await _userRepository.GetByIdAsync(loggedUser.Id))
                            .Friends.Select(x => x.UserId).ToList();
            var unseenMessages = await _messagesService.GetUnseenMessagesFromUserIds(userIdFriends, loggedUser);
            var friends = _userRepository
                .GetBy(x => userIdFriends.Contains(x.Id)).ToList();
            foreach (var friend in friends)
            {
                var messages = unseenMessages.Where(unseenMessage => unseenMessage.From == friend.Id).Count();
                var latestMessage = await GetLatestMessage(friend.Id);
                result.Add(
                    new Friend {
                    AvatarUrl = friend.AvatarUrl,
                    Email = friend.Email,
                    FullName = friend.FullName,
                    UserId = friend.Id,
                    UnseenMessages = messages,
                    LatestMessage = latestMessage
                });
            }

            return result;
        }

        public async Task<List<FriendRequest>> GetFriendRequests() 
        {
            var loggedUser = await _authenticationService.GetLoggedUser();
            var friendRequests = (_friendRequestRepository.GetBy(x => x.ReceiverId == loggedUser.Id && x.Status == UserRequestStatus.Pending)).ToList();
            var friendRequestsResponse = new List<FriendRequest>();
            if (friendRequests.Count == 0) return friendRequestsResponse;

            var senders = friendRequests.Select(x => x.SenderId).ToList();
            var friendRequestUsers = _userRepository.GetBy(x => senders.Contains(x.Id)).ToList();
            foreach (var friendRequestUser in friendRequestUsers)
            {
                var tmpFriendRequest = friendRequests.First(fr => friendRequestUser.Id == fr.SenderId);
                friendRequestsResponse.Add(new FriendRequest
                {
                    Id = tmpFriendRequest.Id,
                    AvatarUrl = friendRequestUser.AvatarUrl,
                    Name = friendRequestUser.FullName,
                    Status = tmpFriendRequest.Status
                });
            }

            return friendRequestsResponse;
        }

        public async Task<Friend> AcceptFriendRequest(FriendRequest friendRequest) 
        {
            var friendRequestEntity = await _friendRequestRepository.GetByIdAsync(friendRequest.Id);
            var friend = new Friend();
            if (friendRequestEntity != null) 
            {
                friendRequestEntity.Status = friendRequest.Status;
                // Create Friend Entity and Add to Both collections sender and receiver
                var senderFriendEntity = new FriendEntity
                {
                    UserId = friendRequestEntity.SenderId,
                    CreationDate = DateTime.Now,
                };

                var receiverFriendEntity = new FriendEntity
                {
                    UserId = friendRequestEntity.ReceiverId,
                    CreationDate = DateTime.Now,
                };

                if (friendRequestEntity.Status == UserRequestStatus.Accepted) 
                {
                    // Validate if the user has added the friend already if not add it to collection
                    var addFriend1 = _userRepository.AddToCollectionAsync(friendRequestEntity.ReceiverId, senderFriendEntity, nameof(UserEntity.Friends));
                    var addFriend2 = _userRepository.AddToCollectionAsync(friendRequestEntity.SenderId, receiverFriendEntity, nameof(UserEntity.Friends));
                    await Task.WhenAll(addFriend1, addFriend2);
                    var userFriend = await _userRepository.GetByIdAsync(friendRequestEntity.SenderId);

                    friend = new Friend 
                    { 
                        FullName = userFriend.FullName,
                        AvatarUrl = userFriend.AvatarUrl,
                        Email = userFriend.Email,
                        UserId = userFriend.Id
                    };
                }
                var updateDefinition = _friendRequestRepository.BuildUpdateDefinition().Set(nameof(FriendRequestEntity.Status), friendRequestEntity.Status);
                await _friendRequestRepository.UpdateById(friendRequestEntity.Id, updateDefinition);
            }

            return friend;
        }

        private async Task<ChatMessage> GetLatestMessage(string messageWith)
        {
            var latestMessages = (await _messagesService.GetChatMessages(messageWith)).OrderByDescending(x => x.SentAt);
            return latestMessages.Take(1).FirstOrDefault();
        }
    }
}
