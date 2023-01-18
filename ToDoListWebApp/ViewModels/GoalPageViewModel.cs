using ToDoListWebApp.Models;

namespace ToDoListWebApp.ViewModels
{
    public class GoalPageViewModel
    {
        public Goal Goal { get; set; } = null!;
        public IEnumerable<Models.Task> Tasks { get; set; } = null!;
        public IEnumerable<Progress> Progresses { get; set; } = null!;
    }
}
