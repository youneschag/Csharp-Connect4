using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Connect_Web.Models
{
    public class AspNetUser
    {
        [Key]
        public Guid Id { get; set; }
        public required string username { get; set; }
        public DateTime CreationDate { get; set; }
        public required string password { get; set; }
    }
}