using System;
using System.Collections.Generic;

namespace ToDoListWebApp.Models
{
    public partial class Progress
    {
        public Guid NidProgress { get; set; }
        public Guid ScheduleId { get; set; }
        public short ProgressTime { get; set; }
        public DateTime CreateDate { get; set; }
        public string? Description { get; set; }
        public Guid UserId { get; set; }

        public virtual Schedule Schedule { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
