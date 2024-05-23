using DB.Entities;
using LiveTrackerCommonModels.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;

namespace DB.Database;

public class RaceTrackerDbContext: DbContext
{
    #region Contstructor

    private readonly string _connectionString;

    public RaceTrackerDbContext(DbContextOptions<RaceTrackerDbContext> options, IConfiguration configuration) : base(options)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }
    #endregion

    #region Properties
    public DbSet<Collection> Collection { get; set; }
    public DbSet<Driver> Driver { get; set; }
    public DbSet<DriverCollection> DriverCollection { get; set; }
    public DbSet<Layout> Layout { get; set; }
    public DbSet<Registration> Registration { get; set; }
    public DbSet<Session> Session { get; set; }
    public DbSet<SessionNotification> SessionNotification { get; set; }
    public DbSet<SessionResult> SessionResult { get; set; }
    public DbSet<Subscriber> Subscriber { get; set; }
    public DbSet<Team> Team { get; set; }
    public DbSet<TeamCollection> TeamCollection { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<UserRefreshToken> UserRefreshToken { get; set; }
    #endregion

    #region Public Methods

    /// <summary>
    /// If database says it does not know function unaccent, run following command in database console (once per database)
    /// CREATE EXTENSION IF NOT EXISTS unaccent;
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [DbFunction(Name = "unaccent")]
    public string Unaccent(string value)
    {
        throw new NotImplementedException("This is only a code-first stand in for PostgreSQL function unaccent");
    }

    public virtual async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Creates table for session
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task CreateArchiveTable(int sessionId)
    {
        const string schemaName = "\"Sessions\"";
        var tableName = $"Session_{sessionId}";

        var createTableSql = $@"
            CREATE TABLE {schemaName}.{tableName} (
                Id SERIAL PRIMARY KEY,
                DateTime timestamp,
                Data varchar
            )";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(createTableSql, conn);
        await cmd.ExecuteNonQueryAsync();
            
        await conn.CloseAsync();
    }

    public async Task<IEnumerable<RaceDataVM>> GetChunkFromArchive(int sessionId, int start, int length)
    {
        var ret = new List<RaceDataVM>();

        const string schemaName = "\"Sessions\"";
        var tableName = $"Session_{sessionId}";

        var sql = $@"
            SELECT data FROM {schemaName}.{tableName}
            OFFSET @start LIMIT @length";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("start", start);
        cmd.Parameters.AddWithValue("length", length);

        await using (var reader = cmd.ExecuteReader())
        {
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var dataJson = reader.GetString(0);
                    var data = JsonConvert.DeserializeObject<RaceDataVM>(dataJson);
                    if(data is not null)
                        ret.Add(data);
                }
            }
        }

        await conn.CloseAsync();

        return ret;
    }

    public async Task<(int Count, int Time)> GetArchivedSessionInfo(int sessionId)
    {
        const string schemaName = "\"Sessions\"";
        var tableName = $"{schemaName}.Session_{sessionId}";

        var countSql = $"SELECT COUNT(*) FROM {tableName}";

        var timeSql = $@"SELECT (EXTRACT(epoch FROM (MAX(datetime) - MIN(datetime))))::integer AS total_time
                    FROM (
                        SELECT datetime
                        FROM {tableName}
                        WHERE id = (SELECT MIN(id) FROM {tableName})
                        UNION ALL
                        SELECT datetime
                        FROM {tableName}
                        WHERE id = (SELECT MAX(id) FROM {tableName})
                    ) AS dates";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var countCommand = new NpgsqlCommand(countSql, connection);
        await using var differenceCommand = new NpgsqlCommand(timeSql, connection);

        var count = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
        var time = Convert.ToInt32(await differenceCommand.ExecuteScalarAsync());

        return (count, time);
    }


    #endregion

    #region Protected Methods

    /// <summary>
    /// Creates relation between tables
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region Collection

        modelBuilder.Entity<Collection>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId);

        #endregion

        #region DriverCollection

        modelBuilder.Entity<DriverCollection>()
            .HasOne<Driver>()
            .WithMany()
            .HasForeignKey(c => c.DriverId);

        modelBuilder.Entity<DriverCollection>()
            .HasOne<Collection>()
            .WithMany()
            .HasForeignKey(c => c.CollectionId);

        #endregion

        #region Layout

        modelBuilder.Entity<Layout>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(l => l.UserId);

        #endregion

        #region Session

        modelBuilder.Entity<Session>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId);

        modelBuilder.Entity<Session>()
            .HasOne<Layout>()
            .WithMany()
            .HasForeignKey(s => s.LayoutId);

        #endregion

        #region SessionNotification

        modelBuilder.Entity<SessionNotification>()
            .HasOne<Session>()
            .WithMany()
            .HasForeignKey(s => s.SessionId);

        #endregion

        #region SessionResult

        modelBuilder.Entity<SessionResult>()
            .HasOne<Session>()
            .WithOne()
            .HasForeignKey<SessionResult>(s => s.SessionId);

        #endregion

        #region Subscriber

        modelBuilder.Entity<Subscriber>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(u => u.UserId);

        modelBuilder.Entity<Subscriber>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(u => u.OrganizerId);

        #endregion

        #region TeamCollection

        modelBuilder.Entity<TeamCollection>()
            .HasOne<Team>()
            .WithMany()
            .HasForeignKey(c => c.TeamId);

        modelBuilder.Entity<TeamCollection>()
            .HasOne<Collection>()
            .WithMany()
            .HasForeignKey(c => c.CollectionId);

        #endregion

        #region UserRefreshToken

        modelBuilder.Entity<UserRefreshToken>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(u => u.UserId);

        #endregion
    }

    #endregion
}