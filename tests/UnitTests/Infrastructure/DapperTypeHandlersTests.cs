using System.Data;
using OneClickYatra.Api.Infrastructure;

namespace OneClickYatra.UnitTests.Infrastructure;

public class DapperTypeHandlersTests
{
    private sealed class FakeParameter : IDbDataParameter
    {
        public DbType DbType { get; set; }
        public object? Value { get; set; }
        public ParameterDirection Direction { get; set; }
        public bool IsNullable => true;
        public string ParameterName { get; set; } = string.Empty;
        public string SourceColumn { get; set; } = string.Empty;
        public DataRowVersion SourceVersion { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public int Size { get; set; }
    }

    [Fact]
    public void DateOnlyHandler_SetValue_ConvertsToDateTimeWithDateDbType()
    {
        var handler = new DapperTypeHandlers.DateOnlyTypeHandler();
        var parameter = new FakeParameter();

        handler.SetValue(parameter, new DateOnly(2026, 12, 20));

        Assert.Equal(DbType.Date, parameter.DbType);
        Assert.Equal(new DateTime(2026, 12, 20), parameter.Value);
    }

    [Fact]
    public void DateOnlyHandler_Parse_ConvertsDateTimeBackToDateOnly()
    {
        var handler = new DapperTypeHandlers.DateOnlyTypeHandler();

        var result = handler.Parse(new DateTime(2026, 12, 20));

        Assert.Equal(new DateOnly(2026, 12, 20), result);
    }

    [Fact]
    public void NullableDateOnlyHandler_SetValue_HandlesNull()
    {
        var handler = new DapperTypeHandlers.NullableDateOnlyTypeHandler();
        var parameter = new FakeParameter();

        handler.SetValue(parameter, null);

        Assert.Equal(DbType.Date, parameter.DbType);
        Assert.Equal(DBNull.Value, parameter.Value);
    }

    [Fact]
    public void NullableDateOnlyHandler_SetValue_ConvertsNonNullValue()
    {
        var handler = new DapperTypeHandlers.NullableDateOnlyTypeHandler();
        var parameter = new FakeParameter();

        handler.SetValue(parameter, new DateOnly(2026, 1, 1));

        Assert.Equal(new DateTime(2026, 1, 1), parameter.Value);
    }
}
