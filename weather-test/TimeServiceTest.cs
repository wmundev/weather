using System;
using weather_backend.Services;
using Xunit;

namespace weather_test
{
    public class TimeServiceTest
    {
        private readonly TimeService _timeService;

        public TimeServiceTest()
        {
            _timeService = new TimeService();
        }

        [Fact]
        public void CompareTimeTest()
        {
            var date1 = new DateTime(2020, 12, 20);
            var date2 = DateTime.Now;

            var result = _timeService.CompareTime(date1, date2);

            Assert.Equal(-1, result);
        }

        [Fact]
        public void CalculateTimeDifferenceToMinutesTest()
        {
            var result = _timeService.CalculateTimeDifferenceToMinutes(1, 20, 2, 40);

            Assert.Equal(80, result);
        }
    }
}