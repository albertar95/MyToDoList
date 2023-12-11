using System;
using System.Collections.Generic;

namespace ToDoListWebApi.Models
{
    public partial class User
    {
        public User()
        {
            Goals = new HashSet<Goal>();
            NoteGroups = new HashSet<NoteGroup>();
            Progresses = new HashSet<Progress>();
            Routines = new HashSet<Routine>();
            Tasks = new HashSet<Task>();
            Accounts = new HashSet<Account>();
            Transactions = new HashSet<Transaction>();
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
        public virtual ICollection<NoteGroup> NoteGroups { get; set; }
        public virtual ICollection<Progress> Progresses { get; set; }
        public virtual ICollection<Routine> Routines { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<Shield> Shields { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
