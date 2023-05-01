using System;
using System.Collections.Generic;

namespace ToDoListWebApi.Models
{
    public partial class Task
    {
        public Task()
        {
            Schedules = new HashSet<Schedule>();
        }

        public Guid NidTask { get; set; }
        public Guid GoalId { get; set; }
        public string Title { get; set; } = null!;
        public bool TaskStatus { get; set; }
        public byte TaskWeight { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public DateTime? ClosureDate { get; set; }
        public Guid UserId { get; set; }

        public virtual Goal Goal { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Schedule> Schedules { get; set; }
    }
}
