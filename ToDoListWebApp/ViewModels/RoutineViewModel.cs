using ToDoListWebApp.Models;

namespace ToDoListWebApp.ViewModels
{
    public class RoutineViewModel
    {
        public List<Routine> Routines { get; set; }
        public List<RoutineProgress> RoutineProgresses { get; set; }
        public string[] DatePeriodInfo { get; set; } = null!;
        public string[] PersianDatePeriodInfo { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
