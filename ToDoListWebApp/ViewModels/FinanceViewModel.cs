using ToDoListWebApp.Models;

namespace ToDoListWebApp.ViewModels
{
    public class FinanceViewModel
    {
        public List<Account> Accounts { get; set; }
        public List<Transaction> Transactions { get; set; }
        public bool AllTransactions { get; set; }
        public string[] bgColor { get; set; } = new string[] { "aquamarine", "burlywood", "lemonchiffon", "azure", "cadetblue", "chartreuse", "lightcoral", "lightsteelblue", "plum", "lightseagreen", "peru", "cornflowerblue", "darkgray", "darkkhaki", "lightblue", "bisque", "violet", "mediumseagreen", "palegreen", "paleturquoise", "tan", "hotpink", "cyan", "thistle", "goldenrod", "darksalmon" };
        public static List<Tuple<byte, string>> TransactionTypes { get; set; } = new List<Tuple<byte, string>>() 
        { new Tuple<byte, string>(1,"Pay"),new Tuple<byte, string>(2,"Lend"),new Tuple<byte, string>(3,"LendBack") };
    }
}
