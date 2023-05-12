using ToDoListWebApp.Models;

namespace ToDoListWebApp.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<Progress> Progresses { get; set; } = null!;
        public IEnumerable<Models.Schedule> Schedules { get; set; } = null!;
        public IEnumerable<Models.Task> Tasks { get; set; } = null!;
        public IEnumerable<Goal> Goals { get; set; } = null!;
        public string[] DatePeriodInfo { get; set; } = null!;
        public string[] PersianDatePeriodInfo { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
