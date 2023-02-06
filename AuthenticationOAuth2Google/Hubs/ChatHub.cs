using AuthenticationOAuth2Google.Domain.Interfaces;
using AuthenticationOAuth2Google.Domain.Models;
using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using AuthenticationOAuth2Google.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text;

namespace AuthenticationOAuth2Google.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IMongoDBRepository<ConnectedUserEntity> _connectedUsersRepository;
        private readonly IMongoDBRepository<UserEntity> _userRepository;
        private readonly IMongoDBRepository<ChatMessageEntity> _chatMessageRepository;
        private readonly IMongoDBRepository<FriendRequestEntity> _friendRequestRepository;

        public ChatHub(
            IMongoDBRepository<ConnectedUserEntity> connectedUsersRepository,
            IMongoDBRepository<UserEntity> userRepository,
            IMongoDBRepository<ChatMessageEntity> chatMessageRepository,
            IMongoDBRepository<FriendRequestEntity> friendRequestRepository
            )
        {
            _connectedUsersRepository = connectedUsersRepository;
            _userRepository = userRepository;
            _chatMessageRepository = chatMessageRepository;
            _friendRequestRepository = friendRequestRepository;
        }

        public async Task SendMessage(ChatMessage message)
        {
            // TODO: do not get children by default
            var toUser = await _userRepository.GetByIdAsync(message.To);
            if (toUser == null)
            {
                // TODO: Notify error to hub user not found
                return;
            }
            
            var user = GetConnectedUserFromContext();
            var connectedUsersEntity = _connectedUsersRepository.GetBy(x => x.UserId == user.UserId || x.UserId == toUser.Id).ToList();

            var messageEntity = new ChatMessageEntity 
            { 
                From = user.UserId,
                To = toUser.Id,
                Message = message.Message,
                Seen = false,
                SentAt = DateTime.Now,
                Type = message.Type.ToString()
            }; 

            // Store message
            await _chatMessageRepository.CreateAsync(messageEntity);

            var messageResponse = new ChatMessage 
            { 
                Id = messageEntity.Id,
                From = messageEntity.From,
                To = messageEntity.To,
                Message = messageEntity.Message,
                Seen = messageEntity.Seen,
                SentAt = DateTime.Now,
                Type = message.Type
            };

            await Clients.Clients(connectedUsersEntity.Select(x => x.ConnectionId)).ReceiveMessage(messageResponse);
            await Clients.Clients(connectedUsersEntity.Select(x => x.ConnectionId)).NewMessage(messageResponse);
        }

        public async Task FriendRequestSent(FriendRequest friendRequest) 
        {
            var friendRequestEntity = await _friendRequestRepository.GetByIdAsync(friendRequest.Id);
            if (friendRequestEntity == null) return;

            var userReceiverConnections = _connectedUsersRepository.GetBy(x => x.UserId == friendRequestEntity.ReceiverId).ToList();
            if (userReceiverConnections != null && userReceiverConnections.Count > 0)
            {
                await Clients.Clients(userReceiverConnections.Select(x => x.ConnectionId)).NewFriendRequest(friendRequest);
            }
        }

        public async Task FriendRequestAccepted(Friend friend)
        {
            var userReceiverConnections = _connectedUsersRepository.GetBy(x => x.UserId == friend.UserId).ToList();
            if (userReceiverConnections != null && userReceiverConnections.Count > 0)
            {
                var user = GetConnectedUserFromContext();
                await Clients.Clients(userReceiverConnections.Select(x => x.ConnectionId)).FriendRequestAccepted(new Friend 
                { 
                    UserId = user.UserId,
                    AvatarUrl = user.AvatarUrl,
                    Email = user.Email,
                    FullName = user.FullName 
                });
            }
        }

        public async override Task OnConnectedAsync()
        {
            var user = GetConnectedUserFromContext();

            if (user == null)
                return;

            var connectedUsersEntity = _connectedUsersRepository.GetBy(x => x.UserId == user.UserId).ToList();
            // This allows the user to have only one connection Active
            foreach (var connectedUser in connectedUsersEntity)
            {
                await _connectedUsersRepository.DeleteBulkAsync(x => x.ConnectionId == connectedUser.ConnectionId);
            }
            
            await _connectedUsersRepository.CreateAsync(new ConnectedUserEntity
            {
                ConnectionId = Context.ConnectionId,
                UserId = user.UserId,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Email = user.Email,
                UserAgent = Context.GetHttpContext()!.Request.Headers["User-Agent"].First() ?? "Unknown",
                ConnectedAt = DateTime.UtcNow
            });

            // Tells the users a new one got connected to the hub
            await Clients.Others.ConnectedToHub(user);
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            var user = GetConnectedUserFromContext();

            if (user == null)
                return;

            Console.WriteLine(String.Format("Client {0} closed the connection.", Context.ConnectionId));
            await _connectedUsersRepository.DeleteBulkAsync(x => x.ConnectionId == Context.ConnectionId);
            await Clients.Others.DisconnectedFromHub(user);
            await base.OnDisconnectedAsync(exception);
        }

        private ConnectedUser GetConnectedUserFromContext() 
        {
            ConnectedUser user = new();
            if (Context.User != null && Context.User.Claims.Count() > 0)
            {
                var userId = Context.User.Claims.First(x => x.Type == "internal_id").Value;
                var connectionId = Context.ConnectionId;
                var email = Context.User.Claims.First(x => x.Type == ClaimTypes.Name).Value;
                var fullName = Context.User.Claims.First(x => x.Type == ClaimTypes.GivenName).Value;
                var avatarUrl = Context.User.Claims.First(x => x.Type == "avatar_url").Value;
                user = new ConnectedUser
                {
                    UserId = userId,
                    ConnectionId = connectionId,
                    FullName = fullName,
                    AvatarUrl = avatarUrl,
                    Email = email,
                    UserAgent = Context.GetHttpContext()!.Request.Headers["User-Agent"].First() ?? "Unknown"
                };
            }

            return user;
        }
    }
}
