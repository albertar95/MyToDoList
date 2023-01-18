using System;
using System.Collections.Generic;

namespace ToDoListWebApi.Models
{
    public partial class Progress
    {
        public Guid NidProgress { get; set; }
        public Guid GoalId { get; set; }
        public Guid TaskId { get; set; }
        public short ProgressTime { get; set; }
        public DateTime CreateDate { get; set; }
        public string? Description { get; set; }
        public Guid UserId { get; set; }

        public virtual Goal Goal { get; set; } = null!;
        public virtual Task Task { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
