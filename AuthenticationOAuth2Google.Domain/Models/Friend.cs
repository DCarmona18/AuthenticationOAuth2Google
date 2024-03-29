﻿using System.ComponentModel.DataAnnotations;

namespace AuthenticationOAuth2Google.Domain.Models
{
    public class Friend
    {
        public string UserId { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public string FullName { get; set; }
        public int UnseenMessages { get; set; } = 0;
        public ChatMessage LatestMessage { get; set; } = new ChatMessage();
    }
}
