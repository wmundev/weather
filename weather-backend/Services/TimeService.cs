using System;

namespace weather_backend.Services
{
    public class TimeService
    {
        public int CompareTime(DateTime dateTime1, DateTime dateTime2)
        {
            return dateTime1.CompareTo(dateTime2);
        }

        public int CalculateTimeDifferenceToMinutes(int firstHours, int firstMinutes, int secondHours,
            int secondMinutes)
        {
            int firstHoursConvertedToMin = firstHours * 60;
            int secondHoursConvertedToMin = secondHours * 60;

            int diff = (secondMinutes + secondHoursConvertedToMin) - (firstHoursConvertedToMin + firstMinutes);
            return diff;
        }
    }
}