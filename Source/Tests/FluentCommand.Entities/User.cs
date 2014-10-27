using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCommand.Entities
{
    public class User
    {
        public User()
        {
            Audits = new List<Audit>();
            AssignedTasks = new List<Task>();
            CreatedTasks = new List<Task>();
            Roles = new List<Role>();
        }

        public int Id { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Byte[] Avatar { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Byte[] RowVersion { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string Comment { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime LastActivityDate { get; set; }
        public DateTime? LastPasswordChangeDate { get; set; }
        public string AvatarType { get; set; }

        public virtual ICollection<Audit> Audits { get; set; }
        public virtual ICollection<Task> AssignedTasks { get; set; }
        public virtual ICollection<Task> CreatedTasks { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
    }
}