using System.Data;
using Dapper;

namespace OneClickYatra.Api.Infrastructure;

/// <summary>
/// Dapper has no built-in support for DateOnly/TimeOnly parameter binding or result mapping —
/// without these handlers, any query touching a DATE column bound to a C# DateOnly property
/// throws NotSupportedException. Register once at startup via RegisterAll().
/// </summary>
public static class DapperTypeHandlers
{
    public static void RegisterAll()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());
    }

    // Public (not private) so unit tests can exercise them directly — SqlMapper does not expose
    // a public way to retrieve a previously-registered handler back out.
    public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        }

        public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);
    }

    public sealed class NullableDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly? value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value is null ? DBNull.Value : value.Value.ToDateTime(TimeOnly.MinValue);
        }

        public override DateOnly? Parse(object value) => value is null or DBNull ? null : DateOnly.FromDateTime((DateTime)value);
    }
}
