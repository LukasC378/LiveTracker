using DB.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace BL.Extensions;

public static class CreateDatabaseExtension
{
    public static void ApplyMigration(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<RaceTrackerDbContext>();
        dbContext.Database.Migrate();
    }

    public static void ApplyDbExtensions(this IApplicationBuilder app, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        const string createScript = @"
            CREATE EXTENSION IF NOT EXISTS unaccent;
            CREATE SCHEMA IF NOT EXISTS ""Sessions"";
        ";

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(createScript, conn);
        cmd.ExecuteNonQuery();

        conn.Close();
    }
}