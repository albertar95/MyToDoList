using System;
using System.Collections.Generic;

namespace ToDoListWebApp.Models
{
    public partial class Goal
    {
        public Goal()
        {
            Progresses = new HashSet<Progress>();
            Tasks = new HashSet<Task>();
        }

        public Guid NidGoal { get; set; }
        public string Title { get; set; } = null!;
        public short EstimateTime { get; set; }
        public byte DurationType { get; set; }
        public byte GoalStatus { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public Guid UserId { get; set; }
        public byte GoalType { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<Progress> Progresses { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }
    }
}
