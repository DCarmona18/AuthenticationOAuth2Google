using AuthenticationOAuth2Google.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationOAuth2Google.Domain.Interfaces
{
    public interface IFriendService
    {
        Task<Friend> AddFriend(Friend friend);
        Task<List<Friend>> GetFriends();
    }
}
