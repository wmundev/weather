using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using weather_backend.Models;
using weather_backend.Services.Interfaces;

namespace weather_backend.Services
{
    /// <summary>
    /// Reads academics from the Postgres database seeded by <c>weather-backend/database/create/academics-create.sql</c>.
    /// Registered only when <c>ConnectionStrings:Academic</c> is configured.
    /// </summary>
    public sealed class AcademicService : IAcademicService
    {
        private const string SelectById = """
            select ACNUM, DEPTNUM, FAMNAME, GIVENAME, INITIALS, TITLE
            from academic
            where ACNUM = @Id
            """;

        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<AcademicService> _logger;

        public AcademicService(NpgsqlDataSource dataSource, ILogger<AcademicService> logger)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Academic?> GetAcademicByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            // The data source pools connections; opening one per call returns it to the pool on dispose.
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

            var academic = await connection.QueryFirstOrDefaultAsync<Academic>(
                new CommandDefinition(SelectById, new { Id = id }, cancellationToken: cancellationToken));

            if (academic is null)
            {
                _logger.LogInformation("No academic found with id {AcademicId}", id);
            }

            return academic;
        }
    }
}
