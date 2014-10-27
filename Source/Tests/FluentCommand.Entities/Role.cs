using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCommand.Entities
{
    public class Role
    {
        public Role()
        {
            Users = new List<User>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Byte[] RowVersion { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}