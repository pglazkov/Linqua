using System;

namespace Linqua.Api.Models
{
    public class User
    {
        public User()
        {
        }

        public User(Guid id, string auth0Id, string email)
        {
            Id = id;
            Auth0Id = auth0Id;
            Email = email;
        }

        public Guid Id { get; set; }

        public string Auth0Id { get; set; }

        public string Email { get; set; }
    }
}