using ToDoListWebApp.Models;

namespace ToDoListWebApp.ViewModels
{
    public class ProgressViewModel
    {
        public List<Models.Task> Tasks { get; set; }
        public List<Schedule> Schedules { get; set; }
        public Progress Progress { get; set; }
    }
}
