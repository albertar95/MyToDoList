using System;
using System.Collections.Generic;

namespace ToDoListWebApp.Models
{
    public partial class Goal
    {
        public Goal()
        {
            Tasks = new HashSet<Task>();
        }

        public Guid NidGoal { get; set; }
        public string Title { get; set; } = null!;
        public byte GoalStatus { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public Guid UserId { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<Task> Tasks { get; set; }
    }
}
