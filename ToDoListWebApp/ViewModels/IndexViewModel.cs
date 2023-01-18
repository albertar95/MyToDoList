using ToDoListWebApp.Models;

namespace ToDoListWebApp.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<Progress> Progresses { get; set; } = null!;
        public IEnumerable<Progress> AllProgresses { get; set; } = null!;
        public IEnumerable<Models.Task> AllTasks { get; set; } = null!;
        public IEnumerable<Models.Task> Tasks { get; set; } = null!;
        public IEnumerable<Goal> Goals { get; set; } = null!;
        public string[] DatePeriodInfo { get; set; } = null!;
    }
}
