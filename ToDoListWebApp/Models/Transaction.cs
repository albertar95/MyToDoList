using System;
using System.Collections.Generic;

namespace ToDoListWebApp.Models
{
    public partial class Transaction
    {
        public Guid NidTransaction { get; set; }
        public byte TransactionType { get; set; }
        public Guid PayerAccount { get; set; }
        public Guid RecieverAccount { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid UserId { get; set; }
        public string TransactionReason { get; set; } = null!;

        public virtual User User { get; set; } = null!;
    }
}
