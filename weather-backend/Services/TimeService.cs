using System;

namespace weather_backend.Services
{
    public static class TimeService
    {
        public static int CompareTime(DateTime dateTime1, DateTime dateTime2)
        {
            return dateTime1.CompareTo(dateTime2);
        }

        public static int CalculateTimeDifferenceToMinutes(int firstHours, int firstMinutes, int secondHours,
            int secondMinutes)
        {
            var firstHoursConvertedToMin = firstHours * 60;
            var secondHoursConvertedToMin = secondHours * 60;

            var diff = secondMinutes + secondHoursConvertedToMin - (firstHoursConvertedToMin + firstMinutes);
            return diff;
        }
    }
}
