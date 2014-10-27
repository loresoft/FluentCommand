using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCommand.Entities
{
    public class Audit
    {
        public Audit()
        {
        }

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int? UserId { get; set; }
        public int? TaskId { get; set; }
        public string Content { get; set; }
        public string Username { get; set; }
        public DateTime CreatedDate { get; set; }
        public Byte[] RowVersion { get; set; }

        public virtual Task Task { get; set; }
        public virtual User User { get; set; }
    }
}