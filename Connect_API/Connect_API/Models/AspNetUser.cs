using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Connect_API.Models
{
    public class AspNetUser
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public DateTime CreationDate { get; set; }
    }

}