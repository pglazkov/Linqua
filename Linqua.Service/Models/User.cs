using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Linqua.Service.Models
{
    public class User
    {
        public User()
        {
        }

        public User(Guid id, string microsoftAccountId, string email)
        {
            Id = id;
            MicrosoftAccountId = microsoftAccountId;
            Email = email;
        }

        [Key]
        public Guid Id { get; set; }

        [MaxLength(256)]
        [Index(IsUnique = true)]
        public string MicrosoftAccountId { get; set; }

        [MaxLength(512)]
        public string Email { get; set; }
    }
}