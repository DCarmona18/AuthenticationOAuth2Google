using AuthenticationOAuth2Google.Domain.Interfaces;
using AuthenticationOAuth2Google.Domain.Models;
using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using AuthenticationOAuth2Google.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationOAuth2Google.Domain.Services
{
    public class MessagesService : IMessagesService
    {
        private readonly IMongoDBRepository<ChatMessageEntity> _chatMessageRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IChatHubService _chatHubService;

        public MessagesService(IMongoDBRepository<ChatMessageEntity> chatMessageRepository, IAuthenticationService authenticationService, IChatHubService chatHubService)
        {
            _chatMessageRepository = chatMessageRepository;
            _authenticationService = authenticationService;
            _chatHubService = chatHubService;
        }
        public async Task<IEnumerable<ChatMessage>> GetChatMessages(string messagesWith)
        {
            var authenticatedUser = await _authenticationService.GetLoggedUser();
            return _chatMessageRepository.GetBy(x => 
            (x.From == authenticatedUser.Id && x.To == messagesWith) 
            || x.To == authenticatedUser.Id && x.From == messagesWith)
                .Select(x => new ChatMessage 
                { 
                    Id = x.Id,
                    From = x.From,
                    Message = x.Message,
                    Seen = x.Seen,
                    SentAt = x.SentAt,
                    To = x.To,
                    Type = x.Type 
                }
            ).OrderBy(x => x.SentAt);
        }

        public async Task MarkAsSeen(Friend friend)
        {
            var authenticatedUser = await _authenticationService.GetLoggedUser();
            var unseenMessages = await GetUnseenMessagesFromUserIds(new List<string> { friend.UserId }, authenticatedUser);
            var messagesId = unseenMessages.Select(x => x.Id).ToList();

            var filterDefinition = _chatMessageRepository.FilterDefinitionBuilder().Where(x => messagesId.Contains(x.Id));
            var updateDefinition = _chatMessageRepository.BuildUpdateDefinition().Set(nameof(ChatMessageEntity.Seen), true);
            var updateResult = await _chatMessageRepository.UpdateBulk(filterDefinition, updateDefinition);
        }

        public async Task<List<ChatMessage>> GetUnseenMessagesFromUserIds(List<string> userIds, User authenticatedUser)
        {
            var unseenMessages = _chatMessageRepository.GetBy(x =>
            (x.To == authenticatedUser.Id && userIds.Contains(x.From)) && !x.Seen).ToList();
            if(unseenMessages != null && unseenMessages.Count > 0)
            {
                return unseenMessages.Select(x => new ChatMessage 
                { 
                    Id = x.Id,
                    From = x.From,
                    To = x.To,
                    Seen = x.Seen,
                    Message = x.Message,
                    Type = x.Type,
                    SentAt = x.SentAt
                }).ToList();
            }
            return new List<ChatMessage>();
        }
    }
}
