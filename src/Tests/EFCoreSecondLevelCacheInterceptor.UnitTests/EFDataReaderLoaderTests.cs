#nullable enable
using System.Data;
using System.Data.Common;
using System.Reflection;
using Moq;
using Assert = Xunit.Assert;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

// ReSharper disable once InconsistentNaming
public class EFDataReaderLoaderTests
{
    [Fact]
    public void HasRows_ReturnsTrue_WhenDbReaderHasRows()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.HasRows).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.HasRows;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void HasRows_ReturnsFalse_WhenDbReaderHasNoRows()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.HasRows).Returns(value: false);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.HasRows;

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void RecordsAffected_ReturnsExpectedValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.RecordsAffected).Returns(value: 5);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.RecordsAffected;

        // Assert
        Assert.Equal(expected: 5, actual);
    }

    [Fact]
    public void RecordsAffected_ReturnsZero_WhenNoRecordsAffected()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.RecordsAffected).Returns(value: 0);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.RecordsAffected;

        // Assert
        Assert.Equal(expected: 0, actual);
    }

    [Fact]
    public void IsClosed_ReturnsTrue_WhenDbReaderIsClosed()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.IsClosed).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.IsClosed;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsClosed_ReturnsFalse_WhenDbReaderIsNotClosed()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.IsClosed).Returns(value: false);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.IsClosed;

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Depth_ReturnsExpectedValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.Depth).Returns(value: 3);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.Depth;

        // Assert
        Assert.Equal(expected: 3, actual);
    }

    [Fact]
    public void Depth_ReturnsZero_WhenDepthIsZero()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.Depth).Returns(value: 0);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.Depth;

        // Assert
        Assert.Equal(expected: 0, actual);
    }

    [Fact]
    public void FieldCount_ReturnsExpectedValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 10);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.FieldCount;

        // Assert
        Assert.Equal(expected: 10, actual);
    }

    [Fact]
    public void FieldCount_ReturnsZero_WhenNoFields()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 0);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.FieldCount;

        // Assert
        Assert.Equal(expected: 0, actual);
    }

    [Fact]
    public void VisibleFieldCount_ReturnsExpectedValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.VisibleFieldCount).Returns(value: 7);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.VisibleFieldCount;

        // Assert
        Assert.Equal(expected: 7, actual);
    }

    [Fact]
    public void VisibleFieldCount_ReturnsZero_WhenNoVisibleFields()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.VisibleFieldCount).Returns(value: 0);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.VisibleFieldCount;

        // Assert
        Assert.Equal(expected: 0, actual);
    }

    [Fact]
    public void Indexer_ReturnsExpectedValue_ByName()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetOrdinal("ColumnName")).Returns(value: 0);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "Value");
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader[name: "ColumnName"];

        // Assert
        Assert.Equal(expected: "Value", actual);
    }

    [Fact]
    public void Indexer_ThrowsIndexOutOfRangeException_WhenColumnNameIsInvalid()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetOrdinal("InvalidColumn")).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader[name: "InvalidColumn"]);
    }

    [Fact]
    public void Indexer_ReturnsExpectedValue_ByOrdinal()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "Value");
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader[ordinal: 0];

        // Assert
        Assert.Equal(expected: "Value", actual);
    }

    [Fact]
    public void Indexer_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader[ordinal: 100]);
    }

    [Fact]
    public void GetDataTypeName_ReturnsExpectedTypeName()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetDataTypeName(1)).Returns(value: "Int32");

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.GetDataTypeName(ordinal: 1);

        // Assert
        Assert.Equal(expected: "Int32", actual);
    }

    [Fact]
    public void GetDataTypeName_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetDataTypeName(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetDataTypeName(ordinal: 100));
    }

    [Fact]
    public void GetFieldType_ReturnsExpectedType()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetFieldType(1)).Returns(typeof(int));

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.GetFieldType(ordinal: 1);

        // Assert
        Assert.Equal(typeof(int), actual);
    }

    [Fact]
    public void GetFieldType_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetFieldType(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetFieldType(ordinal: 100));
    }

    [Fact]
    public void GetName_ReturnsExpectedName()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetName(1)).Returns(value: "ColumnName");

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.GetName(ordinal: 1);

        // Assert
        Assert.Equal(expected: "ColumnName", actual);
    }

    [Fact]
    public void GetName_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetName(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetName(ordinal: 100));
    }

    [Fact]
    public void GetOrdinal_ReturnsExpectedOrdinal()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetOrdinal("ColumnName")).Returns(value: 1);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.GetOrdinal(name: "ColumnName");

        // Assert
        Assert.Equal(expected: 1, actual);
    }

    [Fact]
    public void GetOrdinal_ThrowsIndexOutOfRangeException_WhenColumnNameIsInvalid()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetOrdinal("InvalidColumn")).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetOrdinal(name: "InvalidColumn"));
    }

    [Fact]
    public void GetSchemaTable_ReturnsExpectedSchemaTable()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();
        using var schemaTable = new DataTable();

        dbReaderMock.Setup(r => r.GetSchemaTable()).Returns(schemaTable);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        using var actual = loader.GetSchemaTable();

        // Assert
        Assert.Equal(schemaTable, actual);
    }

    [Fact]
    public void GetSchemaTable_ReturnsNull_WhenSchemaTableIsNull()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetSchemaTable()).Returns((DataTable?)null);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        using var actual = loader.GetSchemaTable();

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void GetBoolean_ReturnsExpectedBooleanValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: true);
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetBoolean(ordinal: 0);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void GetBoolean_ThrowsInvalidCastException_WhenValueIsNotBoolean()
    {
        // Arrange
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: 0);
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetBoolean(ordinal: 0));
    }

    [Fact]
    public void GetBoolean_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetBoolean(ordinal: 100));
    }

    [Fact]
    public void GetByte_ReturnsExpectedByteValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns((byte)123);
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetByte(ordinal: 0);

        // Assert
        Assert.Equal((byte)123, actual);
    }

    [Fact]
    public void GetByte_ThrowsInvalidCastException_WhenValueIsNotByte()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "NotAByte");
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetByte(ordinal: 0));
    }

    [Fact]
    public void GetByte_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetByte(ordinal: 100));
    }

    [Fact]
    public void GetBytes_ReturnsZero()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();
        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.GetBytes(ordinal: 1, dataOffset: 0, buffer: null, bufferOffset: 0, length: 0);

        // Assert
        Assert.Equal(expected: 0L, actual);
    }

    [Fact]
    public void GetBytes_ReturnsZero_WhenBufferIsNotNull()
    {
        // Arrange
        var buffer = new byte[10];
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(buffer);
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetBytes(ordinal: 0, dataOffset: 0, buffer, bufferOffset: 0, length: 10);

        // Assert
        Assert.Equal(expected: 0L, actual);
    }

    [Fact]
    public void GetChar_ReturnsExpectedCharValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: 'A');
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetChar(ordinal: 0);

        // Assert
        Assert.Equal(expected: 'A', actual);
    }

    [Fact]
    public void GetChar_ThrowsInvalidCastException_WhenValueIsNotChar()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "NotAChar");
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetChar(ordinal: 0));
    }

    [Fact]
    public void GetChar_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetChar(ordinal: 100));
    }

    [Fact]
    public void GetChars_ReturnsZero_WhenBufferIsNull()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();
        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.GetChars(ordinal: 0, dataOffset: 0, buffer: null, bufferOffset: 0, length: 0);

        // Assert
        Assert.Equal(expected: 0L, actual);
    }

    [Fact]
    public void GetChars_ReturnsZero_WhenBufferIsNotNull()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();
        using var loader = new EFDataReaderLoader(dbReaderMock.Object);
        var buffer = new char[10];

        loader.Read();

        // Act
        var actual = loader.GetChars(ordinal: 1, dataOffset: 0, buffer, bufferOffset: 0, length: 10);

        // Assert
        Assert.Equal(expected: 0L, actual);
    }

    [Fact]
    public void GetDateTime_ReturnsExpectedDateTimeValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(new DateTime(year: 2023, month: 10, day: 1));
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetDateTime(ordinal: 0);

        // Assert
        Assert.Equal(new DateTime(year: 2023, month: 10, day: 1), actual);
    }

    [Fact]
    public void GetDateTime_ThrowsInvalidCastException_WhenValueIsNotDateTime()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "NotADateTime");
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetDateTime(ordinal: 0));
    }

    [Fact]
    public void GetDateTime_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetDateTime(ordinal: 100));
    }

    [Fact]
    public void GetDecimal_ReturnsExpectedDecimalValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: 123.45m);
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetDecimal(ordinal: 0);

        // Assert
        Assert.Equal(expected: 123.45m, actual);
    }

    [Fact]
    public void GetDecimal_ThrowsInvalidCastException_WhenValueIsNotDecimal()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "NotADecimal");
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetDecimal(ordinal: 0));
    }

    [Fact]
    public void GetDecimal_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetDecimal(ordinal: 100));
    }

    [Fact]
    public void GetDouble_ReturnsExpectedDoubleValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: 123.45);
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetDouble(ordinal: 0);

        // Assert
        Assert.Equal(expected: 123.45, actual);
    }

    [Fact]
    public void GetDouble_ThrowsInvalidCastException_WhenValueIsNotDouble()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "NotADouble");
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetDouble(ordinal: 0));
    }

    [Fact]
    public void GetDouble_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetDouble(ordinal: 100));
    }

    [Fact]
    public void GetEnumerator_ThrowsNotSupportedException()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();
        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<NotSupportedException>(() => loader.GetEnumerator());
    }

    [Fact]
    public void GetFloat_ReturnsExpectedFloatValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: 123.45f);
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetFloat(ordinal: 0);

        // Assert
        Assert.Equal(expected: 123.45f, actual);
    }

    [Fact]
    public void GetFloat_ThrowsInvalidCastException_WhenValueIsNotFloat()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "NotAFloat");
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetFloat(ordinal: 0));
    }

    [Fact]
    public void GetFloat_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetFloat(ordinal: 100));
    }

    [Fact]
    public void GetGuid_ReturnsExpectedGuidValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(new Guid(g: "12345678-1234-1234-1234-1234567890ab"));
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetGuid(ordinal: 0);

        // Assert
        Assert.Equal(new Guid(g: "12345678-1234-1234-1234-1234567890ab"), actual);
    }

    [Fact]
    public void GetGuid_ThrowsInvalidCastException_WhenValueIsNotGuid()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "NotAGuid");
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        loader.Read();

        // Assert
        Assert.Throws<InvalidCastException>(() => loader.GetGuid(ordinal: 0));
    }

    [Fact]
    public void GetGuid_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetGuid(ordinal: 100));
    }

    [Fact]
    public void GetInt16_ReturnsExpectedInt16Value()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns((short)123);
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        var actual = loader.GetInt16(ordinal: 0);

        // Act && Assert
        Assert.Equal((short)123, actual);
    }

    [Fact]
    public void GetInt16_ThrowsInvalidCastException_WhenValueIsNotInt16()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "NotAnInt16");
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetInt16(ordinal: 0));
    }

    [Fact]
    public void GetInt16_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetInt16(ordinal: 100));
    }

    [Fact]
    public void GetInt32_ReturnsExpectedInt32Value()
    {
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: 123);
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetInt32(ordinal: 0);

        // Assert
        Assert.Equal(expected: 123, actual);
    }

    [Fact]
    public void GetInt32_ThrowsInvalidCastException_WhenValueIsNotInt32()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "NotAnInt32");
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetInt32(ordinal: 0));
    }

    [Fact]
    public void GetInt32_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // ACt && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetInt32(ordinal: 100));
    }

    [Fact]
    public void GetInt64_ReturnsExpectedInt64Value()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: 123L);
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetInt64(ordinal: 0);

        // Assert
        Assert.Equal(expected: 123L, actual);
    }

    [Fact]
    public void GetInt64_ThrowsInvalidCastException_WhenValueIsNotInt64()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "NotAnInt64");
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetInt64(ordinal: 0));
    }

    [Fact]
    public void GetInt64_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetInt64(ordinal: 100));
    }

    [Fact]
    public void GetString_ReturnsExpectedStringValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "TestString");
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetString(ordinal: 0);

        // Assert
        Assert.Equal(expected: "TestString", actual);
    }

    [Fact]
    public void GetString_ThrowsInvalidCastException_WhenValueIsNotString()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: 123);
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetString(ordinal: 0));
    }

    [Fact]
    public void GetString_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetString(ordinal: 100));
    }

    [Fact]
    public void GetValues_ReturnsExpectedNumberOfValues()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 3);
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        var values = new object[3];

        // Act
        var actual = loader.GetValues(values);

        // Assert
        Assert.Equal(expected: 3, actual);
    }

    [Fact]
    public void GetValues_CopiesValuesExpectedly()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 3);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(1)).Returns(value: "Test");
        dbReaderMock.Setup(r => r.GetValue(2)).Returns(value: true);
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        var values = new object[3];

        // Act
        loader.GetValues(values);

        // Assert
        Assert.Equal(expected: 1, values[0]);
        Assert.Equal(expected: "Test", values[1]);
        Assert.Equal(expected: true, values[2]);
    }

    [Fact]
    public void GetValues_ThrowsArgumentNullException_WhenValuesArrayIsNull()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();
        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<ArgumentNullException>(() => loader.GetValues(null!));
    }

    [Fact]
    public void GetValues_ThrowsArgumentException_WhenValuesArrayIsTooSmall()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 3);
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        var values = new object[2];

        // Act && Assert
        Assert.Throws<ArgumentException>(() => loader.GetValues(values));
    }

    [Fact]
    public void NextResult_ReturnsTrue_WhenThereAreMoreResults()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.NextResult()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.NextResult();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void NextResult_ReturnsFalse_WhenThereAreNoMoreResults()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.NextResult()).Returns(value: false);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.NextResult();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void NextResult_ThrowsInvalidOperationException_WhenDbReaderThrowsException()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.NextResult()).Throws<InvalidOperationException>();

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<InvalidOperationException>(() => loader.NextResult());
    }

    [Fact]
    public void LoadAndClose_ReturnsTableRowsWithAllData()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.Read())
            .Returns(value: true)
            .Callback(() => dbReaderMock.Setup(r => r.Read()).Returns(value: false));

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "value");

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.Load();

        // Assert
        Assert.Single(actual.Rows);
        Assert.Equal(expected: "value", actual.Rows[index: 0][ordinal: 0]);
    }

    [Fact]
    public void Read_ReturnsFalse_WhenNoMoreRows()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.SetupSequence(r => r.Read()).Returns(value: true).Returns(value: false);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.Read();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void GetValue_ReturnsExpectedValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "value");
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetValue(ordinal: 0);

        // Assert
        Assert.Equal(expected: "value", actual);
    }

    [Fact]
    public void GetValue_ReturnsExpectedBinaryValue()
    {
        // Arrange
        var expected = new byte[]
        {
            1, 2, 3, 4, 5
        };

        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(value: "value");
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);
        using var memoryStream = new MemoryStream(expected);
        dbReaderMock.Setup(r => r.GetStream(0)).Returns(memoryStream);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        var tableRows = loader
            .GetType()
            .GetField(name: "_tableRows", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(loader) as EFTableRows;

        tableRows!.ColumnsInfo[key: 0].TypeName = "Microsoft.SqlServer.Types.SqlGeography";

        loader.Read();

        // Act
        var actual = loader.GetValue(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsDBNull_ReturnsTrue_WhenValueIsDBNull()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(value: 1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(DBNull.Value);
        dbReaderMock.Setup(r => r.Read()).Returns(value: true);

        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.IsDBNull(ordinal: 0);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Close_ClosesDbReader()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();
        using var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        loader.Close();

        // Assert
        dbReaderMock.Verify(r => r.Close(), Times.Once);
    }
}