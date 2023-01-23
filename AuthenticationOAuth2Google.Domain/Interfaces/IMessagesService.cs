﻿using AuthenticationOAuth2Google.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationOAuth2Google.Domain.Interfaces
{
    public interface IMessagesService
    {
        Task<List<ChatMessage>> GetChatMessages(string messagesWith);
    }
}
