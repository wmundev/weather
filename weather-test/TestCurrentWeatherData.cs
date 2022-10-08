﻿using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using weather_backend.Services;
using weather_test.Logger;
using Xunit.Abstractions;

namespace weather_test
{
    public class TestCurrentWeatherData
    {
        private readonly Mock<IConfiguration> _configuration = new();

        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler = new();
        private readonly Mock<ISecretService> _mockSecretService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ICurrentWeatherData _underTest;

        public TestCurrentWeatherData(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;


            var inMemorySettings = new Dictionary<string, string> {{"OpenWeatherApiKey", "1"}};

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var logger = XUnitLogger.CreateLogger<CurrentWeatherData>(_testOutputHelper);

            _mockSecretService = new Mock<ISecretService>();
            _underTest = new CurrentWeatherData(configuration, new HttpClient(_mockHttpMessageHandler.Object), logger, _mockSecretService.Object);
        }

        // FIXME
        // [Fact]
        // public async Task TestGetCurrentWeatherDataByCityId()
        // {
        //     // see https://gingter.org/2018/07/26/how-to-mock-httpclient-in-your-net-c-unit-tests/ on why we use protected
        //     _mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
        //             ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        //         .ReturnsAsync(new HttpResponseMessage {StatusCode = HttpStatusCode.OK});
        //
        //     await _underTest.GetCurrentWeatherDataByCityId(3029.00);
        //
        //     var expectedUri =
        //         new Uri("https://api.openweathermap.org:443/data/2.5/weather?id=3029&appid=1&units=metric");
        //     _mockHttpMessageHandler.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.Is<HttpRequestMessage>(
        //             req =>
        //                 req.Method == HttpMethod.Get // we expected a GET request
        //                 && req.RequestUri == expectedUri // to this uri
        //         ),
        //         ItExpr.IsAny<CancellationToken>());
        // }
    }
}