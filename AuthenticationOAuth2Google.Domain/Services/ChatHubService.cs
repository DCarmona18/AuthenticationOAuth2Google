using AuthenticationOAuth2Google.Domain.Interfaces;
using AuthenticationOAuth2Google.Domain.Models;
using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using AuthenticationOAuth2Google.Infrastructure.Interfaces;

namespace AuthenticationOAuth2Google.Domain.Services
{
    public class ChatHubService : IChatHubService
    {
        private readonly IMongoDBRepository<ConnectedUserEntity> _connectedUserRepository;
        private readonly IAuthenticationService _authenticationService;

        public ChatHubService(IMongoDBRepository<ConnectedUserEntity> connectedUserRepository, IAuthenticationService authenticationService)
        {
            _connectedUserRepository = connectedUserRepository;
            _authenticationService = authenticationService;
        }
        public async Task<IEnumerable<ConnectedUser>> GetConnectedUsers()
        {
            var loggedUser = await _authenticationService.GetLoggedUser();
            if (loggedUser == null)
                throw new Exception("Failed to get logged user");

            // Get the connected users from hub
            return _connectedUserRepository.GetBy(x => x.UserId != loggedUser.Id)
            .Select(x => new ConnectedUser 
            { 
                UserId = x.UserId,
                AvatarUrl = x.AvatarUrl,
                ConnectionId = x.ConnectionId,
                Email = x.Email,
                FullName = x.FullName,
                UserAgent = x.UserAgent,
                ConnectedAt = x.ConnectedAt
            }).OrderByDescending(x => x.ConnectedAt)
            .ToList();
        }
    }
}
