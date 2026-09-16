using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

var connectionString = configuration.GetConnectionString("SqlServer")
    ?? configuration["ConnectionStrings:SqlServer"]
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=OneClickYatra;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

var mode = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "migrate";
var databaseRoot = FindDatabaseRoot(AppContext.BaseDirectory);

switch (mode)
{
    case "migrate":
        await RunScriptsAsync(connectionString, Path.Combine(databaseRoot, "Migrations"), "__SchemaVersions");
        break;
    case "seed":
        await RunScriptsAsync(connectionString, Path.Combine(databaseRoot, "Seeds"), "__SeedVersions");
        break;
    case "storedprocs":
        await RunScriptsAsync(connectionString, Path.Combine(databaseRoot, "StoredProcedures"), "__StoredProcedureVersions");
        break;
    case "hash":
        var plainText = args.ElementAtOrDefault(1) ?? throw new ArgumentException("Usage: dotnet run -- hash <plainTextPassword>");
        Console.WriteLine(BCrypt.Net.BCrypt.HashPassword(plainText, workFactor: 12));
        break;
    default:
        Console.WriteLine($"Unknown mode '{mode}'. Use 'migrate', 'seed', 'storedprocs' or 'hash'.");
        Environment.Exit(1);
        break;
}

static string FindDatabaseRoot(string __startDirectory)
{
    var directory = new DirectoryInfo(__startDirectory);
    while (directory is not null)
    {
        var candidate = Path.Combine(directory.FullName, "database");
        if (Directory.Exists(candidate))
        {
            return candidate;
        }

        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException($"Could not locate a 'database' folder above '{__startDirectory}'.");
}

static async Task RunScriptsAsync(string __connectionString, string __scriptsFolder, string __versionTableName)
{
    if (!Directory.Exists(__scriptsFolder))
    {
        Console.WriteLine($"No scripts folder found at '{__scriptsFolder}'. Nothing to do.");
        return;
    }

    await using var connection = new SqlConnection(__connectionString);
    await connection.OpenAsync();

    await EnsureVersionTableAsync(connection, __versionTableName);
    var appliedScripts = await GetAppliedScriptsAsync(connection, __versionTableName);

    var scriptFiles = Directory.GetFiles(__scriptsFolder, "*.sql")
        .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
        .ToList();

    foreach (var scriptFile in scriptFiles)
    {
        var scriptName = Path.GetFileName(scriptFile);
        if (appliedScripts.Contains(scriptName))
        {
            Console.WriteLine($"Skipping already-applied script: {scriptName}");
            continue;
        }

        Console.WriteLine($"Applying: {scriptName}");
        var sqlText = await File.ReadAllTextAsync(scriptFile);
        var batches = Regex.Split(sqlText, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Select(batch => batch.Trim())
            .Where(batch => batch.Length > 0);

        await using var transaction = connection.BeginTransaction();
        try
        {
            foreach (var batch in batches)
            {
                await using var command = new SqlCommand(batch, connection, transaction);
                await command.ExecuteNonQueryAsync();
            }

            await using (var recordCommand = new SqlCommand(
                $"INSERT INTO {__versionTableName} (ScriptName, AppliedAtUtc) VALUES (@ScriptName, SYSUTCDATETIME())",
                connection, transaction))
            {
                recordCommand.Parameters.AddWithValue("@ScriptName", scriptName);
                await recordCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    Console.WriteLine("Done.");
}

static async Task EnsureVersionTableAsync(SqlConnection __connection, string __versionTableName)
{
    var sql = $"""
        IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = '{__versionTableName}')
        BEGIN
            CREATE TABLE {__versionTableName}
            (
                Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_{__versionTableName} PRIMARY KEY,
                ScriptName NVARCHAR(260) NOT NULL,
                AppliedAtUtc DATETIME2 NOT NULL
            );
        END
        """;

    await using var command = new SqlCommand(sql, __connection);
    await command.ExecuteNonQueryAsync();
}

static async Task<HashSet<string>> GetAppliedScriptsAsync(SqlConnection __connection, string __versionTableName)
{
    var applied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    await using var command = new SqlCommand($"SELECT ScriptName FROM {__versionTableName}", __connection);
    await using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        applied.Add(reader.GetString(0));
    }

    return applied;
}
