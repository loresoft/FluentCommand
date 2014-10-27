using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCommand.Entities
{
    public class Status
    {
        public Status()
        {
            Tasks = new List<Task>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Byte[] RowVersion { get; set; }

        public virtual ICollection<Task> Tasks { get; set; }
    }
}