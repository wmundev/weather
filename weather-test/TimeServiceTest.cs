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
            DateTime date1 = new DateTime(2020, 12, 20);
            DateTime date2 = DateTime.Now;

            int result = _timeService.CompareTime(date1, date2);
            
            Assert.Equal(-1,result );
        }
    }
}