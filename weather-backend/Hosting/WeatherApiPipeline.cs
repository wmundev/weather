using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using weather_backend.Middleware;

namespace weather_backend.Hosting
{
    /// <summary>
    /// The HTTP request pipeline, called from Program.cs. Middleware order is significant - see the comments.
    /// </summary>
    public static class WeatherApiPipeline
    {
        public static WebApplication UseWeatherApi(this WebApplication app)
        {
            // First, so everything below sees the caller's real address rather than the load balancer's -
            // the rate limiter partitions on it and GeolocationService reports it. Also feeds
            // UseHttpsRedirection the original scheme.
            app.UseForwardedHeaders();

            // Outside the exception handler on purpose. Its finally block records the status code, and
            // from inside it would run before the handler had turned the exception into a 500 - logging
            // every failed request as though it had succeeded. Out here it also puts the correlation
            // scope around the handler, so the exception record carries the same id as the request.
            app.UseLogMiddleware();

            // Catches everything downstream, including the static file middleware. In Development the
            // developer exception page sits inside it and answers first; everywhere else
            // GlobalExceptionHandler writes the ProblemDetails response.
            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "weather_backend v1"));
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();

            app.UseRouting();

            // After UseRouting so the endpoint is known and per-endpoint [EnableRateLimiting] policies
            // resolve; before the endpoints themselves so rejected requests never reach a controller.
            app.UseRateLimiter();

            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");

            return app;
        }
    }
}
