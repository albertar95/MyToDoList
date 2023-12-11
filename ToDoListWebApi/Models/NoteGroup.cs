using System;
using System.Collections.Generic;

namespace ToDoListWebApi.Models
{
    public partial class NoteGroup
    {
        public NoteGroup()
        {
            Notes = new HashSet<Note>();
        }

        public Guid NidGroup { get; set; }
        public string Title { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid UserId { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<Note> Notes { get; set; }
    }
}
