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
        private readonly IMongoDBRepository<ConnectedUserEntity> _mongoDBRepository;

        public ChatHub(IMongoDBRepository<ConnectedUserEntity> mongoDBRepository)
        {
            _mongoDBRepository = mongoDBRepository;
        }

        public async Task SendMessage(ChatMessage message)
        {
            var userSender = _mongoDBRepository.GetBy(x => x.UserId == message.UserIdSender).FirstOrDefault();
            if (userSender == null) return;

            var user = GetConnectedUserFromContext();
            var connectedUsersEntity = _mongoDBRepository.GetBy(x => x.UserId == user.UserId).ToList();
            
            var messageReceived = new ChatMessage 
            { 
                Message = message.Message,
                UserIdSender = message.UserIdSender,
                AvatarUrl = userSender.AvatarUrl,
                SentAt = DateTime.Now
            };

            var messageSent = new ChatMessage
            {
                Message = message.Message,
                UserIdSender = user.UserId,
                AvatarUrl = user.AvatarUrl,
                SentAt = DateTime.Now
            };

            await Clients.AllExcept(connectedUsersEntity.Select(x => x.ConnectionId)).ReceiveMessage(messageReceived);
            // TODO: Send individual user
            // await Clients.Client(userReceiver.Email).ReceiveMessage(messageReceived);
            await Clients.Caller.SentMessage(messageSent);
        }

        public async override Task OnConnectedAsync()
        {
            var user = GetConnectedUserFromContext();

            if (user == null)
                return;

            var connectedUsersEntity = _mongoDBRepository.GetBy(x => x.UserId == user.UserId).ToList();
            // This allows the user to have only one connection Active
            foreach (var connectedUser in connectedUsersEntity)
            {
                await _mongoDBRepository.DeleteBulkAsync(x => x.ConnectionId == connectedUser.ConnectionId);
            }
            
            await _mongoDBRepository.CreateAsync(new ConnectedUserEntity
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
            await _mongoDBRepository.DeleteBulkAsync(x => x.ConnectionId == Context.ConnectionId);
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
