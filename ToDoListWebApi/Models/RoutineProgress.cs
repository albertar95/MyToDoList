using System;
using System.Collections.Generic;

namespace ToDoListWebApi.Models
{
    public partial class RoutineProgress
    {
        public Guid NidRoutineProgress { get; set; }
        public Guid RoutineId { get; set; }
        public DateTime ProgressDate { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual Routine Routine { get; set; } = null!;
    }
}
