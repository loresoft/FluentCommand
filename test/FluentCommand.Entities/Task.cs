using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCommand.Entities
{
    public class Task
    {
        public Task()
        {
            Audits = new List<Audit>();
        }

        public int Id { get; set; }
        public int StatusId { get; set; }
        public int? PriorityId { get; set; }
        public int CreatedId { get; set; }
        public string Summary { get; set; }
        public string Details { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompleteDate { get; set; }
        public int? AssignedId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Byte[] RowVersion { get; set; }
        public string LastModifiedBy { get; set; }

        public virtual ICollection<Audit> Audits { get; set; }
        public virtual Priority Priority { get; set; }
        public virtual Status Status { get; set; }
        public virtual User AssignedUser { get; set; }
        public virtual User CreatedUser { get; set; }
        public virtual TaskExtended TaskExtended { get; set; }
    }
}