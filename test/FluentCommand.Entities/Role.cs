using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCommand.Entities
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string UpdatedBy { get; set; }
        public byte[] RowVersion { get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}