using System;
using System.Collections.Generic;

namespace ToDoListWebApi.Models
{
    public partial class User
    {
        public User()
        {
            Goals = new HashSet<Goal>();
            Progresses = new HashSet<Progress>();
            Tasks = new HashSet<Task>();
        }

        public Guid NidUser { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool IsAdmin { get; set; }
        public byte[]? ProfilePic { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsDisabled { get; set; }

        public virtual ICollection<Goal> Goals { get; set; }
        public virtual ICollection<Progress> Progresses { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }
    }
}
