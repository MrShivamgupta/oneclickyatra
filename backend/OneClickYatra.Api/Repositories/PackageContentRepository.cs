using System.Data;
using Dapper;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Repositories;

public sealed class PackageContentRepository : IPackageContentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PackageContentRepository(IDbConnectionFactory __connectionFactory)
    {
        _connectionFactory = __connectionFactory;
    }

    public async Task<IReadOnlyList<PackageItineraryDayModel>> GetItineraryAsync(Guid __packageId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, PackageId, DayNumber, Title, Description
            FROM PackageItineraries WHERE PackageId = @PackageId ORDER BY DayNumber
            """;
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<PackageItineraryDayModel>(
            new CommandDefinition(sql, new { PackageId = __packageId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task ReplaceItineraryAsync(Guid __packageId, IReadOnlyList<PackageItineraryDayModel> __days, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM PackageItineraries WHERE PackageId = @PackageId",
            new { PackageId = __packageId }, transaction, cancellationToken: __cancellationToken));

        const string insertSql = """
            INSERT INTO PackageItineraries (Id, PackageId, DayNumber, Title, Description)
            VALUES (@Id, @PackageId, @DayNumber, @Title, @Description)
            """;
        foreach (var day in __days)
        {
            await connection.ExecuteAsync(new CommandDefinition(insertSql, new
            {
                Id = Guid.NewGuid(),
                PackageId = __packageId,
                day.DayNumber,
                day.Title,
                day.Description
            }, transaction, cancellationToken: __cancellationToken));
        }

        transaction.Commit();
    }

    public async Task<IReadOnlyList<PackageInclusionModel>> GetInclusionsAsync(Guid __packageId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, PackageId, Description, IsIncluded, SortOrder
            FROM PackageInclusions WHERE PackageId = @PackageId ORDER BY SortOrder
            """;
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<PackageInclusionModel>(
            new CommandDefinition(sql, new { PackageId = __packageId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task ReplaceInclusionsAsync(Guid __packageId, IReadOnlyList<PackageInclusionModel> __inclusions, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM PackageInclusions WHERE PackageId = @PackageId",
            new { PackageId = __packageId }, transaction, cancellationToken: __cancellationToken));

        const string insertSql = """
            INSERT INTO PackageInclusions (Id, PackageId, Description, IsIncluded, SortOrder)
            VALUES (@Id, @PackageId, @Description, @IsIncluded, @SortOrder)
            """;
        foreach (var inclusion in __inclusions)
        {
            await connection.ExecuteAsync(new CommandDefinition(insertSql, new
            {
                Id = Guid.NewGuid(),
                PackageId = __packageId,
                inclusion.Description,
                inclusion.IsIncluded,
                inclusion.SortOrder
            }, transaction, cancellationToken: __cancellationToken));
        }

        transaction.Commit();
    }

    public async Task<IReadOnlyList<PackagePricingTierModel>> GetPricingAsync(Guid __packageId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, PackageId, TierName, HotelCategory, PricePerPerson, ChildPrice, ValidFrom, ValidTo, Currency
            FROM PackagePricing WHERE PackageId = @PackageId ORDER BY PricePerPerson
            """;
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<PackagePricingTierModel>(
            new CommandDefinition(sql, new { PackageId = __packageId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task ReplacePricingAsync(Guid __packageId, IReadOnlyList<PackagePricingTierModel> __tiers, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM PackagePricing WHERE PackageId = @PackageId",
            new { PackageId = __packageId }, transaction, cancellationToken: __cancellationToken));

        const string insertSql = """
            INSERT INTO PackagePricing (Id, PackageId, TierName, HotelCategory, PricePerPerson, ChildPrice, ValidFrom, ValidTo, Currency)
            VALUES (@Id, @PackageId, @TierName, @HotelCategory, @PricePerPerson, @ChildPrice, @ValidFrom, @ValidTo, @Currency)
            """;
        foreach (var tier in __tiers)
        {
            await connection.ExecuteAsync(new CommandDefinition(insertSql, new
            {
                Id = Guid.NewGuid(),
                PackageId = __packageId,
                tier.TierName,
                tier.HotelCategory,
                tier.PricePerPerson,
                tier.ChildPrice,
                tier.ValidFrom,
                tier.ValidTo,
                tier.Currency
            }, transaction, cancellationToken: __cancellationToken));
        }

        transaction.Commit();
    }

    public async Task<IReadOnlyList<PackageInventoryModel>> GetInventoryAsync(Guid __packageId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, PackageId, DepartureDate, TotalSeats, BookedSeats, Status
            FROM PackageInventory WHERE PackageId = @PackageId ORDER BY DepartureDate
            """;
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<PackageInventoryModel>(
            new CommandDefinition(sql, new { PackageId = __packageId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task ReplaceInventoryAsync(Guid __packageId, IReadOnlyList<PackageInventoryModel> __departures, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM PackageInventory WHERE PackageId = @PackageId",
            new { PackageId = __packageId }, transaction, cancellationToken: __cancellationToken));

        const string insertSql = """
            INSERT INTO PackageInventory (Id, PackageId, DepartureDate, TotalSeats, BookedSeats, Status)
            VALUES (@Id, @PackageId, @DepartureDate, @TotalSeats, @BookedSeats, @Status)
            """;
        foreach (var departure in __departures)
        {
            await connection.ExecuteAsync(new CommandDefinition(insertSql, new
            {
                Id = Guid.NewGuid(),
                PackageId = __packageId,
                departure.DepartureDate,
                departure.TotalSeats,
                departure.BookedSeats,
                departure.Status
            }, transaction, cancellationToken: __cancellationToken));
        }

        transaction.Commit();
    }

    public async Task<IReadOnlyList<PackageMediaModel>> GetMediaAsync(Guid __packageId, CancellationToken __cancellationToken)
    {
        const string sql = """
            SELECT Id, PackageId, MediaUrl, MediaType, SortOrder, IsCoverImage
            FROM PackageMedia WHERE PackageId = @PackageId ORDER BY SortOrder
            """;
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<PackageMediaModel>(
            new CommandDefinition(sql, new { PackageId = __packageId }, cancellationToken: __cancellationToken));
        return result.ToList();
    }

    public async Task ReplaceMediaAsync(Guid __packageId, IReadOnlyList<PackageMediaModel> __media, CancellationToken __cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM PackageMedia WHERE PackageId = @PackageId",
            new { PackageId = __packageId }, transaction, cancellationToken: __cancellationToken));

        const string insertSql = """
            INSERT INTO PackageMedia (Id, PackageId, MediaUrl, MediaType, SortOrder, IsCoverImage)
            VALUES (@Id, @PackageId, @MediaUrl, @MediaType, @SortOrder, @IsCoverImage)
            """;
        foreach (var media in __media)
        {
            await connection.ExecuteAsync(new CommandDefinition(insertSql, new
            {
                Id = Guid.NewGuid(),
                PackageId = __packageId,
                media.MediaUrl,
                media.MediaType,
                media.SortOrder,
                media.IsCoverImage
            }, transaction, cancellationToken: __cancellationToken));
        }

        transaction.Commit();
    }
}
