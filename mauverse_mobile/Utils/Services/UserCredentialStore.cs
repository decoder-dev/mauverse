using System.Data;
using mau.Database;
using mau.Models;
using Microsoft.EntityFrameworkCore;

namespace mau.Utils.Services;

public static class UserCredentialStore
{
    private const string TokenKey = "mauverse.auth.token";
    private const string PrivateTokenKey = "mauverse.auth.private_token";
    private static readonly SemaphoreSlim MigrationGate = new(1, 1);

    public static async Task SaveAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        cancellationToken.ThrowIfCancellationRequested();
        await SecureStorage.Default.SetAsync(PrivateTokenKey, user.PrivateToken ?? string.Empty);
        cancellationToken.ThrowIfCancellationRequested();
        await SecureStorage.Default.SetAsync(TokenKey, user.Token ?? string.Empty);
    }

    public static async Task RestoreAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        cancellationToken.ThrowIfCancellationRequested();
        user.Token = await SecureStorage.Default.GetAsync(TokenKey) ?? string.Empty;
        cancellationToken.ThrowIfCancellationRequested();
        user.PrivateToken = await SecureStorage.Default.GetAsync(PrivateTokenKey) ?? string.Empty;
    }

    public static async Task MigrateLegacyAsync(
        DbConnect context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        await MigrationGate.WaitAsync(cancellationToken);
        try
        {
            await MigrateLegacyCoreAsync(context, cancellationToken);
        }
        finally
        {
            MigrationGate.Release();
        }
    }

    public static void Clear()
    {
        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(PrivateTokenKey);
    }

    private static async Task MigrateLegacyCoreAsync(
        DbConnect context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var storedToken = await SecureStorage.Default.GetAsync(TokenKey);
        var storedPrivateToken = await SecureStorage.Default.GetAsync(PrivateTokenKey);

        var connection = context.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != ConnectionState.Open;
        if (shouldCloseConnection)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var columnsCommand = connection.CreateCommand();
            columnsCommand.CommandText = "PRAGMA table_info(Users)";
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var reader = await columnsCommand.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                    columns.Add(reader.GetString(1));
            }

            if (columns.Contains("Token") || columns.Contains("PrivateToken"))
            {
                var tokenExpression = columns.Contains("Token") ? "\"Token\"" : "NULL";
                var privateTokenExpression = columns.Contains("PrivateToken") ? "\"PrivateToken\"" : "NULL";
                await using var readCommand = connection.CreateCommand();
                readCommand.CommandText = $"SELECT {tokenExpression}, {privateTokenExpression} FROM Users LIMIT 1";
                await using var reader = await readCommand.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    if (storedPrivateToken is null && !reader.IsDBNull(1))
                        await SecureStorage.Default.SetAsync(PrivateTokenKey, reader.GetString(1));
                    if (storedToken is null && !reader.IsDBNull(0))
                        await SecureStorage.Default.SetAsync(TokenKey, reader.GetString(0));
                }
            }

            if (!columns.Overlaps(["Id", "Token", "PrivateToken"]))
                return;

            static string TextColumn(HashSet<string> existingColumns, string name) =>
                existingColumns.Contains(name) ? $"COALESCE(\"{name}\", '')" : "''";

            static string IntegerColumn(
                HashSet<string> existingColumns,
                string name,
                string fallback = "")
            {
                if (existingColumns.Contains(name))
                    return $"COALESCE(\"{name}\", 0)";
                return !string.IsNullOrEmpty(fallback) && existingColumns.Contains(fallback)
                    ? $"COALESCE(\"{fallback}\", 0)"
                    : "0";
            }

            // SQLite cannot drop legacy secret columns in place on every supported
            // Android version, so rebuild the table inside one transaction.
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            await using var migrateCommand = connection.CreateCommand();
            migrateCommand.Transaction = transaction;
            migrateCommand.CommandText = $"""
                DROP TABLE IF EXISTS "Users_Migration";
                CREATE TABLE "Users_Migration" (
                    "UserId" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY,
                    "Username" TEXT NOT NULL,
                    "FirstName" TEXT NOT NULL,
                    "FullName" TEXT NOT NULL,
                    "Role" INTEGER NOT NULL,
                    "CreditBook" TEXT NOT NULL,
                    "GroupId" TEXT NOT NULL,
                    "SubGroupId" TEXT NOT NULL,
                    "GroupName" TEXT NOT NULL,
                    "GroupDescription" TEXT NOT NULL
                );
                INSERT OR REPLACE INTO "Users_Migration"
                    ("UserId", "Username", "FirstName", "FullName", "Role", "CreditBook", "GroupId", "SubGroupId", "GroupName", "GroupDescription")
                SELECT
                    {IntegerColumn(columns, "UserId", "Id")},
                    {TextColumn(columns, "Username")},
                    {TextColumn(columns, "FirstName")},
                    {TextColumn(columns, "FullName")},
                    {IntegerColumn(columns, "Role")},
                    {TextColumn(columns, "CreditBook")},
                    {TextColumn(columns, "GroupId")},
                    {TextColumn(columns, "SubGroupId")},
                    {TextColumn(columns, "GroupName")},
                    {TextColumn(columns, "GroupDescription")}
                FROM "Users";
                DROP TABLE "Users";
                ALTER TABLE "Users_Migration" RENAME TO "Users";
                """;
            await migrateCommand.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            context.ChangeTracker.Clear();
        }
        finally
        {
            if (shouldCloseConnection)
                await connection.CloseAsync();
        }
    }
}
