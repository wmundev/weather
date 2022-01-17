using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using weather_backend.Models;

namespace weather_backend.Services;

public class AcademicService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AcademicService> _logger;

    public AcademicService(IConfiguration configuration, ILogger<AcademicService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Academic GetAcademicById(int id)
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
            var result = connection.Query<Academic>(query, parameters);
            _logger.Log(LogLevel.Warning, result.First().ACNUM.ToString());
            return result.First();
        }

        return null;
    }
}