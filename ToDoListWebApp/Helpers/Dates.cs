
using System.Globalization;

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
        public static Tuple<string, string> ToPersianDate(Tuple<DateTime, DateTime> dates) 
        {
            PersianCalendar pc = new PersianCalendar();
            return new Tuple<string, string>($"{pc.GetYear(dates.Item1).ToString("0000")}/{pc.GetMonth(dates.Item1).ToString("00")}/{pc.GetDayOfMonth(dates.Item1).ToString("00")}",
                $"{pc.GetYear(dates.Item2).ToString("0000")}/{pc.GetMonth(dates.Item2).ToString("00")}/{pc.GetDayOfMonth(dates.Item2).ToString("00")}");
        }
        public static string ToPersianDate2(DateTime date,bool IncludeTime = false)
        {
            PersianCalendar pc = new PersianCalendar();
            if(!IncludeTime)
                return $"{pc.GetYear(date).ToString("0000")}/{pc.GetMonth(date).ToString("00")}/{pc.GetDayOfMonth(date).ToString("00")}";
            else
                return $"{pc.GetYear(date).ToString("0000")}/{pc.GetMonth(date).ToString("00")}/{pc.GetDayOfMonth(date).ToString("00")} {date.Hour.ToString("00")}:{date.Minute.ToString("00")}:{date.Second.ToString("00")}";
        }
    }
}
