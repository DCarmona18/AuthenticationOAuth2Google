using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationOAuth2Google.Infrastructure.Context.Entities
{
    public class FriendEntity 
    { 
        public string UserId { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
