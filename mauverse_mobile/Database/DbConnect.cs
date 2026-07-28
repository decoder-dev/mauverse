using mau.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace mau.Database;

public sealed class DbConnect : DbContext
{
    private static readonly SemaphoreSlim InitializationGate = new(1, 1);
    private static volatile bool _isInitialized;

    public DbConnect()
    {
    }

    public DbConnect(DbContextOptions<DbConnect> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Note> Notes => Set<Note>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(FileSystem.AppDataDirectory, "local.db"),
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = true,
            DefaultTimeout = 10
        }.ToString();

        optionsBuilder.UseSqlite(connectionString, sqliteOptions => sqliteOptions.CommandTimeout(10));
    }

    public async Task EnsureDatabaseCreatedAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            return;

        await InitializationGate.WaitAsync(cancellationToken);
        try
        {
            if (_isInitialized)
                return;

            await Database.EnsureCreatedAsync(cancellationToken);
            _isInitialized = true;
        }
        finally
        {
            InitializationGate.Release();
        }
    }
}
