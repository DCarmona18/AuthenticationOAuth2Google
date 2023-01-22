using System.ComponentModel.DataAnnotations;

namespace AuthenticationOAuth2Google.Domain.Models
{
    public class Friend
    {
        public string Id { get; set; }
        public string UserId { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public string FullName { get; set; }
    }
}
