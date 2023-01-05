using AuthenticationOAuth2Google.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationOAuth2Google.Domain.Interfaces
{
    public interface IAuthenticationService
    {
        Task<User> CreateUserAsync(User user);
    }
}
