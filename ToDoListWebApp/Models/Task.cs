using System;
using System.Collections.Generic;

namespace ToDoListWebApp.Models
{
    public partial class Task
    {
        public Task()
        {
            Progresses = new HashSet<Progress>();
        }

        public Guid NidTask { get; set; }
        public Guid GoalId { get; set; }
        public string Title { get; set; } = null!;
        public bool TaskStatus { get; set; }
        public short EstimateTime { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public DateTime? ClosureDate { get; set; }
        public Guid UserId { get; set; }

        public virtual Goal Goal { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Progress> Progresses { get; set; }
    }
}
