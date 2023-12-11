using System;
using System.Collections.Generic;

namespace ToDoListWebApi.Models
{
    public partial class Note
    {
        public Guid NidNote { get; set; }
        public Guid GroupId { get; set; }
        public string Title { get; set; } = null!;
        public string NoteContent { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual NoteGroup Group { get; set; } = null!;
    }
}
