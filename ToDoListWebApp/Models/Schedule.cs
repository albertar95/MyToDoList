using System;
using System.Collections.Generic;

namespace ToDoListWebApp.Models
{
    public partial class Schedule
    {
        public Schedule()
        {
            Progresses = new HashSet<Progress>();
        }

        public Guid NidSchedule { get; set; }
        public Guid TaskId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual Task Task { get; set; } = null!;
        public virtual ICollection<Progress> Progresses { get; set; }
    }
}
