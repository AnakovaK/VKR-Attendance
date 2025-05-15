using AttendDatabase;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json.Linq;
using Npgsql;

namespace RTUAttendAPI.API.Services.Initialization;

public class MigrateDb : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MigrateDb> _logger;

    public MigrateDb(IServiceScopeFactory serviceScopeFactory, ILogger<MigrateDb> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AttendDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        if (db.Database.GetDbConnection() is NpgsqlConnection npgsqlConnection)
        {
            await npgsqlConnection.OpenAsync(cancellationToken);
            try
            {
                await npgsqlConnection.ReloadTypesAsync();
            }
            finally
            {
                await npgsqlConnection.CloseAsync();
            }
        }
        _logger.LogInformation("DB migrated");
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
