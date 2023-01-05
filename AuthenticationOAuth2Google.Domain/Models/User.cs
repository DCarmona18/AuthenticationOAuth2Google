using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationOAuth2Google.Domain.Models
{
    public class User
    {
        public string? Id { get; set; }

        public string Email { get; set; }

        public string Fullname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AvatarUrl { get; set; }
        public string Role { get; set; }
        public bool Enabled { get; set; }
        public DateTime CreationDate { get; set; }
        public AUTH_TYPE AuthType { get; set; }
    }
}
