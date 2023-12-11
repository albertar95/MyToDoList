using System;
using System.Collections.Generic;

namespace ToDoListWebApi.Models
{
    public partial class Account
    {
        public Guid NidAccount { get; set; }
        public string Title { get; set; } = null!;
        public DateTime CreateDate { get; set; }
        public DateTime LastModified { get; set; }
        public decimal Amount { get; set; }
        public decimal LendAmount { get; set; }
        public bool IsActive { get; set; }
        public Guid UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
