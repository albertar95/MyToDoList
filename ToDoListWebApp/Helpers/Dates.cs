namespace ToDoListWebApp.Helpers
{
    public class Dates
    {
        public static Tuple<DateTime,DateTime> GetWeekPeriod(int DistanceFromNow = 0) 
        {
            int diff = (7 + (DateTime.Now.DayOfWeek - DayOfWeek.Saturday)) % 7;
            DateTime CurrentWeekStart = DateTime.Now.AddDays(-1 * diff);
            return new Tuple<DateTime, DateTime>(CurrentWeekStart.AddDays(7 * DistanceFromNow).Date, CurrentWeekStart.AddDays(7 * DistanceFromNow + 6).Date);
        }
    }
}
