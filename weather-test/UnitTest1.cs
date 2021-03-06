using System;
using weather_backend.Services;
using Xunit;

namespace weather_test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Number number = new Number();
            int result = number.Get1Plus1();
            Assert.Equal(result, 2);
        }
    }
}
