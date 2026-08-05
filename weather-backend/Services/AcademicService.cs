using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using weather_backend.Models;
using weather_backend.Services.Interfaces;

namespace weather_backend.Services
{
    public class AcademicService : IAcademicService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AcademicService> _logger;

        public AcademicService(IConfiguration configuration, ILogger<AcademicService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Academic? GetAcademicById(int id)
        {
            var host = "localhost";
            var username = _configuration.GetValue<string>("DBUser");
            var password = _configuration.GetValue<string>("DBPassword");
            var database = _configuration.GetValue<string>("DBDatabase");
            var connectionString = $"Host={host};Username={username};Password={password};Database={database}";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                var parameters = new {Id = id};
                var query = "select * from academic where ACNUM = @Id";

                // FirstOrDefault, not First: an unknown id is a not-found result, not an exception.
                var firstResult = connection.Query<Academic>(query, parameters).FirstOrDefault();
                if (firstResult is null)
                {
                    _logger.LogInformation("No academic found with id {AcademicId}", id);
                    return null;
                }

                return firstResult;
            }
        }
    }
}
