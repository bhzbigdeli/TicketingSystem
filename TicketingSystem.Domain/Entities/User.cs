using System;
using System.Collections.Generic;
using System.Text;
using TicketingSystem.Domain.Enums;

namespace TicketingSystem.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }

        private User() { } // For EF Core

        public User(string fullName, string email, string passwordHash, UserRole role)
        {
            FullName = fullName;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
        }

        public void ChangeRole(UserRole role)
        {
            Role = role;
            SetUpdated();
        }

        public void ChangePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            SetUpdated();
        }
    }
}
