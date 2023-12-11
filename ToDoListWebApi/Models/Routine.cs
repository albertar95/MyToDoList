using System;
using System.Collections.Generic;

namespace ToDoListWebApi.Models
{
    public partial class Routine
    {
        public Routine()
        {
            RoutineProgresses = new HashSet<RoutineProgress>();
        }

        public Guid NidRoutine { get; set; }
        public string Title { get; set; } = null!;
        public byte RepeatType { get; set; }
        public string RepeatDays { get; set; } = null!;
        public DateTime FromDate { get; set; }
        public DateTime Todate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool Status { get; set; }
        public Guid UserId { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<RoutineProgress> RoutineProgresses { get; set; }
    }
}
