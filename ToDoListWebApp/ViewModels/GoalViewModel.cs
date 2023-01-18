using ToDoListWebApp.Models;

namespace ToDoListWebApp.ViewModels
{
    public class GoalViewModel
    {
        public IEnumerable<Goal> Goals { get; set; } = null!;
        public IEnumerable<Models.Task> Tasks { get; set; } = null!;
        public IEnumerable<Progress> Progresses { get; set; } = null!;
        public Progress Progress { get; set; } = null!;
        public string[] bgColor { get; set; } = new string[] { "aquamarine", "burlywood", "lemonchiffon", "azure", "cadetblue", "chartreuse", "lightcoral", "lightsteelblue", "plum", "lightseagreen", "peru", "cornflowerblue", "darkgray", "darkkhaki", "lightblue", "bisque", "violet", "mediumseagreen", "palegreen", "paleturquoise", "tan", "hotpink", "cyan", "thistle", "goldenrod", "darksalmon" };
    }
}
