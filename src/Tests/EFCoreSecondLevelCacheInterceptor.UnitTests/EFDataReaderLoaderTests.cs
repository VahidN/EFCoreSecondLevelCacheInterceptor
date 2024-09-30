#nullable enable
using System.Data;
using System.Data.Common;
using System.Reflection;
using Moq;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

// ReSharper disable once InconsistentNaming
public class EFDataReaderLoaderTests
{
    [Fact]
    public void HasRows_ReturnsTrue_WhenDbReaderHasRows()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.HasRows).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

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

        dbReaderMock.Setup(r => r.HasRows).Returns(false);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

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

        dbReaderMock.Setup(r => r.RecordsAffected).Returns(5);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.RecordsAffected;

        // Assert
        Assert.Equal(5, actual);
    }

    [Fact]
    public void RecordsAffected_ReturnsZero_WhenNoRecordsAffected()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.RecordsAffected).Returns(0);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.RecordsAffected;

        // Assert
        Assert.Equal(0, actual);
    }

    [Fact]
    public void IsClosed_ReturnsTrue_WhenDbReaderIsClosed()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.IsClosed).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

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

        dbReaderMock.Setup(r => r.IsClosed).Returns(false);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

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

        dbReaderMock.Setup(r => r.Depth).Returns(3);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.Depth;

        // Assert
        Assert.Equal(3, actual);
    }

    [Fact]
    public void Depth_ReturnsZero_WhenDepthIsZero()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.Depth).Returns(0);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.Depth;

        // Assert
        Assert.Equal(0, actual);
    }

    [Fact]
    public void FieldCount_ReturnsExpectedValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(10);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.FieldCount;

        // Assert
        Assert.Equal(10, actual);
    }

    [Fact]
    public void FieldCount_ReturnsZero_WhenNoFields()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(0);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.FieldCount;

        // Assert
        Assert.Equal(0, actual);
    }

    [Fact]
    public void VisibleFieldCount_ReturnsExpectedValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.VisibleFieldCount).Returns(7);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.VisibleFieldCount;

        // Assert
        Assert.Equal(7, actual);
    }

    [Fact]
    public void VisibleFieldCount_ReturnsZero_WhenNoVisibleFields()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.VisibleFieldCount).Returns(0);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.VisibleFieldCount;

        // Assert
        Assert.Equal(0, actual);
    }

    [Fact]
    public void Indexer_ReturnsExpectedValue_ByName()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetOrdinal("ColumnName")).Returns(0);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("Value");
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader["ColumnName"];

        // Assert
        Assert.Equal("Value", actual);
    }

    [Fact]
    public void Indexer_ThrowsIndexOutOfRangeException_WhenColumnNameIsInvalid()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetOrdinal("InvalidColumn")).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader["InvalidColumn"]);
    }

    [Fact]
    public void Indexer_ReturnsExpectedValue_ByOrdinal()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("Value");
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader[0];

        // Assert
        Assert.Equal("Value", actual);
    }

    [Fact]
    public void Indexer_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader[100]);
    }

    [Fact]
    public void GetDataTypeName_ReturnsExpectedTypeName()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetDataTypeName(1)).Returns("Int32");

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.GetDataTypeName(1);

        // Assert
        Assert.Equal("Int32", actual);
    }

    [Fact]
    public void GetDataTypeName_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetDataTypeName(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetDataTypeName(100));
    }

    [Fact]
    public void GetFieldType_ReturnsExpectedType()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetFieldType(1)).Returns(typeof(int));

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.GetFieldType(1);

        // Assert
        Assert.Equal(typeof(int), actual);
    }

    [Fact]
    public void GetFieldType_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetFieldType(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetFieldType(100));
    }

    [Fact]
    public void GetName_ReturnsExpectedName()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetName(1)).Returns("ColumnName");

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.GetName(1);

        // Assert
        Assert.Equal("ColumnName", actual);
    }

    [Fact]
    public void GetName_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetName(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetName(100));
    }

    [Fact]
    public void GetOrdinal_ReturnsExpectedOrdinal()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetOrdinal("ColumnName")).Returns(1);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.GetOrdinal("ColumnName");

        // Assert
        Assert.Equal(1, actual);
    }

    [Fact]
    public void GetOrdinal_ThrowsIndexOutOfRangeException_WhenColumnNameIsInvalid()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetOrdinal("InvalidColumn")).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetOrdinal("InvalidColumn"));
    }

    [Fact]
    public void GetSchemaTable_ReturnsExpectedSchemaTable()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();
        var schemaTable = new DataTable();

        dbReaderMock.Setup(r => r.GetSchemaTable()).Returns(schemaTable);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.GetSchemaTable();

        // Assert
        Assert.Equal(schemaTable, actual);
    }

    [Fact]
    public void GetSchemaTable_ReturnsNull_WhenSchemaTableIsNull()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetSchemaTable()).Returns((DataTable?)null);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.GetSchemaTable();

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void GetBoolean_ReturnsExpectedBooleanValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(true);
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetBoolean(0);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void GetBoolean_ThrowsInvalidCastException_WhenValueIsNotBoolean()
    {
        // Arrange
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(0);
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetBoolean(0));
    }

    [Fact]
    public void GetBoolean_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetBoolean(100));
    }

    [Fact]
    public void GetByte_ReturnsExpectedByteValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns((byte)123);
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetByte(0);

        // Assert
        Assert.Equal((byte)123, actual);
    }

    [Fact]
    public void GetByte_ThrowsInvalidCastException_WhenValueIsNotByte()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("NotAByte");
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetByte(0));
    }

    [Fact]
    public void GetByte_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetByte(100));
    }

    [Fact]
    public void GetBytes_ReturnsZero()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();
        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.GetBytes(1, 0, null, 0, 0);

        // Assert
        Assert.Equal(0L, actual);
    }

    [Fact]
    public void GetBytes_ReturnsZero_WhenBufferIsNotNull()
    {
        // Arrange
        var buffer = new byte[10];
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(buffer);
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetBytes(0, 0, buffer, 0, 10);

        // Assert
        Assert.Equal(0L, actual);
    }

    [Fact]
    public void GetChar_ReturnsExpectedCharValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns('A');
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetChar(0);

        // Assert
        Assert.Equal('A', actual);
    }

    [Fact]
    public void GetChar_ThrowsInvalidCastException_WhenValueIsNotChar()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("NotAChar");
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetChar(0));
    }

    [Fact]
    public void GetChar_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetChar(100));
    }

    [Fact]
    public void GetChars_ReturnsZero_WhenBufferIsNull()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();
        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.GetChars(0, 0, null, 0, 0);

        // Assert
        Assert.Equal(0L, actual);
    }

    [Fact]
    public void GetChars_ReturnsZero_WhenBufferIsNotNull()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();
        var loader = new EFDataReaderLoader(dbReaderMock.Object);
        var buffer = new char[10];

        loader.Read();

        // Act
        var actual = loader.GetChars(1, 0, buffer, 0, 10);

        // Assert
        Assert.Equal(0L, actual);
    }

    [Fact]
    public void GetDateTime_ReturnsExpectedDateTimeValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(new DateTime(2023, 10, 1));
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetDateTime(0);

        // Assert
        Assert.Equal(new DateTime(2023, 10, 1), actual);
    }

    [Fact]
    public void GetDateTime_ThrowsInvalidCastException_WhenValueIsNotDateTime()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("NotADateTime");
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetDateTime(0));
    }

    [Fact]
    public void GetDateTime_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetDateTime(100));
    }

    [Fact]
    public void GetDecimal_ReturnsExpectedDecimalValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(123.45m);
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetDecimal(0);

        // Assert
        Assert.Equal(123.45m, actual);
    }

    [Fact]
    public void GetDecimal_ThrowsInvalidCastException_WhenValueIsNotDecimal()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("NotADecimal");
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetDecimal(0));
    }

    [Fact]
    public void GetDecimal_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetDecimal(100));
    }

    [Fact]
    public void GetDouble_ReturnsExpectedDoubleValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(123.45);
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetDouble(0);

        // Assert
        Assert.Equal(123.45, actual);
    }

    [Fact]
    public void GetDouble_ThrowsInvalidCastException_WhenValueIsNotDouble()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("NotADouble");
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetDouble(0));
    }

    [Fact]
    public void GetDouble_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetDouble(100));
    }

    [Fact]
    public void GetEnumerator_ThrowsNotSupportedException()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();
        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<NotSupportedException>(() => loader.GetEnumerator());
    }

    [Fact]
    public void GetFloat_ReturnsExpectedFloatValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(123.45f);
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetFloat(0);

        // Assert
        Assert.Equal(123.45f, actual);
    }

    [Fact]
    public void GetFloat_ThrowsInvalidCastException_WhenValueIsNotFloat()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("NotAFloat");
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetFloat(0));
    }

    [Fact]
    public void GetFloat_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetFloat(100));
    }

    [Fact]
    public void GetGuid_ReturnsExpectedGuidValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(new Guid("12345678-1234-1234-1234-1234567890ab"));
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetGuid(0);

        // Assert
        Assert.Equal(new Guid("12345678-1234-1234-1234-1234567890ab"), actual);
    }

    [Fact]
    public void GetGuid_ThrowsInvalidCastException_WhenValueIsNotGuid()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("NotAGuid");
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        loader.Read();

        // Assert
        Assert.Throws<InvalidCastException>(() => loader.GetGuid(0));
    }

    [Fact]
    public void GetGuid_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetGuid(100));
    }

    [Fact]
    public void GetInt16_ReturnsExpectedInt16Value()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns((short)123);
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        var actual = loader.GetInt16(0);

        // Act && Assert
        Assert.Equal((short)123, actual);
    }

    [Fact]
    public void GetInt16_ThrowsInvalidCastException_WhenValueIsNotInt16()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("NotAnInt16");
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetInt16(0));
    }

    [Fact]
    public void GetInt16_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetInt16(100));
    }

    [Fact]
    public void GetInt32_ReturnsExpectedInt32Value()
    {
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(123);
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetInt32(0);

        // Assert
        Assert.Equal(123, actual);
    }

    [Fact]
    public void GetInt32_ThrowsInvalidCastException_WhenValueIsNotInt32()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("NotAnInt32");
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetInt32(0));
    }

    [Fact]
    public void GetInt32_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // ACt && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetInt32(100));
    }

    [Fact]
    public void GetInt64_ReturnsExpectedInt64Value()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(123L);
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetInt64(0);

        // Assert
        Assert.Equal(123L, actual);
    }

    [Fact]
    public void GetInt64_ThrowsInvalidCastException_WhenValueIsNotInt64()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("NotAnInt64");
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetInt64(0));
    }

    [Fact]
    public void GetInt64_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetInt64(100));
    }

    [Fact]
    public void GetString_ReturnsExpectedStringValue()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("TestString");
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetString(0);

        // Assert
        Assert.Equal("TestString", actual);
    }

    [Fact]
    public void GetString_ThrowsInvalidCastException_WhenValueIsNotString()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(123);
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<InvalidCastException>(() => loader.GetString(0));
    }

    [Fact]
    public void GetString_ThrowsIndexOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.GetValue(It.IsAny<int>())).Throws<IndexOutOfRangeException>();

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<IndexOutOfRangeException>(() => loader.GetString(100));
    }

    [Fact]
    public void GetValues_ReturnsExpectedNumberOfValues()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(3);
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        var values = new object[3];

        // Act
        var actual = loader.GetValues(values);

        // Assert
        Assert.Equal(3, actual);
    }

    [Fact]
    public void GetValues_CopiesValuesExpectedly()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(3);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(1)).Returns("Test");
        dbReaderMock.Setup(r => r.GetValue(2)).Returns(true);
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        var values = new object[3];

        // Act
        loader.GetValues(values);

        // Assert
        Assert.Equal(1, values[0]);
        Assert.Equal("Test", values[1]);
        Assert.Equal(true, values[2]);
    }

    [Fact]
    public void GetValues_ThrowsArgumentNullException_WhenValuesArrayIsNull()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();
        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act && Assert
        Assert.Throws<ArgumentNullException>(() => loader.GetValues(null!));
    }

    [Fact]
    public void GetValues_ThrowsArgumentException_WhenValuesArrayIsTooSmall()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(3);
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

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

        dbReaderMock.Setup(r => r.NextResult()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

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

        dbReaderMock.Setup(r => r.NextResult()).Returns(false);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

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

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act && Assert
        Assert.Throws<InvalidOperationException>(() => loader.NextResult());
    }

    [Fact]
    public void LoadAndClose_ReturnsTableRowsWithAllData()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.Read()).Returns(true)
            .Callback(() => dbReaderMock.Setup(r => r.Read()).Returns(false));
        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("value");

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        var actual = loader.LoadAndClose();

        // Assert
        Assert.Single(actual.Rows);
        Assert.Equal("value", actual.Rows[0][0]);
    }

    [Fact]
    public void Read_ReturnsFalse_WhenNoMoreRows()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.SetupSequence(r => r.Read()).Returns(true).Returns(false);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

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

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("value");
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.GetValue(0);

        // Assert
        Assert.Equal("value", actual);
    }

    [Fact]
    public void GetValue_ReturnsExpectedBinaryValue()
    {
        // Arrange
        var expected = new byte[] { 1, 2, 3, 4, 5 };
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns("value");
        dbReaderMock.Setup(r => r.Read()).Returns(true);
        dbReaderMock.Setup(r => r.GetStream(0)).Returns(new MemoryStream(expected));

        var loader = new EFDataReaderLoader(dbReaderMock.Object);
        var tableRows = loader
            .GetType()
            .GetField("_tableRows", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(loader) as EFTableRows;

        tableRows!.ColumnsInfo[0].TypeName = "Microsoft.SqlServer.Types.SqlGeography";

        loader.Read();

        // Act
        var actual = loader.GetValue(0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsDBNull_ReturnsTrue_WhenValueIsDBNull()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();

        dbReaderMock.Setup(r => r.FieldCount).Returns(1);
        dbReaderMock.Setup(r => r.GetValue(0)).Returns(DBNull.Value);
        dbReaderMock.Setup(r => r.Read()).Returns(true);

        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        loader.Read();

        // Act
        var actual = loader.IsDBNull(0);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Close_ClosesDbReader()
    {
        // Arrange
        var dbReaderMock = new Mock<DbDataReader>();
        var loader = new EFDataReaderLoader(dbReaderMock.Object);

        // Act
        loader.Close();

        // Assert
        dbReaderMock.Verify(r => r.Close(), Times.Once);
    }
}