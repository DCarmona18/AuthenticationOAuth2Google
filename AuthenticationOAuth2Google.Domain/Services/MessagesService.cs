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

        public MessagesService(IMongoDBRepository<ChatMessageEntity> chatMessageRepository, IAuthenticationService authenticationService)
        {
            _chatMessageRepository = chatMessageRepository;
            _authenticationService = authenticationService;
        }
        public async Task<List<ChatMessage>> GetChatMessages(string messagesWith)
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
            ).OrderBy(x => x.SentAt).ToList();
        }

        public async Task<List<string>> MarkAsSeen(Friend friend)
        {
            var authenticatedUser = await _authenticationService.GetLoggedUser();
            var unseenMessagesId =_chatMessageRepository.GetBy(x =>
            ((x.From == authenticatedUser.Id && x.To == friend.UserId)
            || (x.To == authenticatedUser.Id && x.From == friend.UserId)) && !x.Seen).Select(x => x.Id).ToList();

            var filterDefinition = _chatMessageRepository.FilterDefinitionBuilder().Where(x => unseenMessagesId.Contains(x.Id));
            var updateDefinition = _chatMessageRepository.BuildUpdateDefinition().Set(nameof(ChatMessageEntity.Seen), true);
            await _chatMessageRepository.UpdateBulk(filterDefinition, updateDefinition);
            return unseenMessagesId;
        }
    }
}
