using System.Globalization;
using System.Text.Json;
using Assert = Xunit.Assert;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

// ReSharper disable once InconsistentNaming
public class EFTableRowsDataReaderTests
{
    public static IEnumerable<object[]> GetByteData => new List<object[]>
    {
        new object[]
        {
            null, default(byte)
        },
        new object[]
        {
            true, (byte)1
        },
        new object[]
        {
            false, (byte)0
        },
        new object[]
        {
            123L, (byte)123
        },
        new object[]
        {
            123f, (byte)123
        },
        new object[]
        {
            123M, (byte)123
        },
        new object[]
        {
            "123", (byte)123
        },
        new object[]
        {
            (byte)123, (byte)123
        }
    };

    public static IEnumerable<object[]> GetDateTimeData => new List<object[]>
    {
        new object[]
        {
            null, default(DateTime)
        },
        new object[]
        {
            string.Empty, default(DateTime)
        },
        new object[]
        {
            "2023-10-01T12:00:00", new DateTime(year: 2023, month: 10, day: 1, hour: 12, minute: 0, second: 0)
        },
        new object[]
        {
            new DateTime(year: 2023, month: 10, day: 1, hour: 12, minute: 0, second: 0),
            new DateTime(year: 2023, month: 10, day: 1, hour: 12, minute: 0, second: 0)
        },
        new object[]
        {
            " ", default(DateTime)
        }
    };

    public static IEnumerable<object[]> GetDecimalData => new List<object[]>
    {
        new object[]
        {
            null, default(decimal)
        },
        new object[]
        {
            string.Empty, default(decimal)
        },
        new object[]
        {
            "123.45", 123.45m
        },
        new object[]
        {
            123.45m, 123.45m
        },
        new object[]
        {
            123.45f, 123.45m
        },
        new object[]
        {
            123.45, 123.45m
        },
        new object[]
        {
            " ", default(decimal)
        }
    };

    public static IEnumerable<object[]> GetDoubleData => new List<object[]>
    {
        new object[]
        {
            null, default(double)
        },
        new object[]
        {
            "123.45", 123.45
        },
        new object[]
        {
            123.45, 123.45
        }
    };

    public static IEnumerable<object[]> GetFloatData => new List<object[]>
    {
        new object[]
        {
            null, default(float)
        },
        new object[]
        {
            "123.45", 123.45f
        },
        new object[]
        {
            123.45, 123.45f
        },
        new object[]
        {
            123.45f, 123.45f
        }
    };

    public static IEnumerable<object[]> GetGuidData => new List<object[]>
    {
        new object[]
        {
            "d3b07384-d9a7-4f3b-8a1b-3b2c1a1b2c3d", new Guid(g: "d3b07384-d9a7-4f3b-8a1b-3b2c1a1b2c3d")
        },
        new object[]
        {
            new Guid(g: "d3b07384-d9a7-4f3b-8a1b-3b2c1a1b2c3d"), new Guid(g: "d3b07384-d9a7-4f3b-8a1b-3b2c1a1b2c3d")
        },
        new object[]
        {
            new Guid(g: "d3b07384-d9a7-4f3b-8a1b-3b2c1a1b2c3d").ToByteArray(),
            new Guid(g: "d3b07384-d9a7-4f3b-8a1b-3b2c1a1b2c3d")
        }
    };

    public static IEnumerable<object[]> GetInt16Data => new List<object[]>
    {
        new object[]
        {
            null, default(short)
        },
        new object[]
        {
            true, (short)1
        },
        new object[]
        {
            false, (short)0
        },
        new object[]
        {
            (long)123, (short)123
        },
        new object[]
        {
            123, (short)123
        },
        new object[]
        {
            (short)123, (short)123
        }
    };

    public static IEnumerable<object[]> GetInt32Data => new List<object[]>
    {
        new object[]
        {
            null, default(int)
        },
        new object[]
        {
            true, 1
        },
        new object[]
        {
            false, 0
        },
        new object[]
        {
            "123", 123
        },
        new object[]
        {
            123, 123
        }
    };

    public static IEnumerable<object[]> GetInt64Data => new List<object[]>
    {
        new object[]
        {
            null, default(long)
        },
        new object[]
        {
            true, 1L
        },
        new object[]
        {
            false, 0L
        },
        new object[]
        {
            123L, 123L
        },
        new object[]
        {
            "123", 123L
        }
    };

    public static IEnumerable<object[]> GetStringData => new List<object[]>
    {
        new object[]
        {
            null, string.Empty
        },
        new object[]
        {
            string.Empty, string.Empty
        }
    };

    public static IEnumerable<object[]> ValidNumberData => new List<object[]>
    {
        new object[]
        {
            (sbyte)1
        },
        new object[]
        {
            (byte)1
        },
        new object[]
        {
            (short)1
        },
        new object[]
        {
            (ushort)1
        },
        new object[]
        {
            1
        },
        new object[]
        {
            1U
        },
        new object[]
        {
            1L
        },
        new object[]
        {
            1UL
        },
        new object[]
        {
            1F
        },
        new object[]
        {
            1D
        },
        new object[]
        {
            1M
        }
    };

    public static IEnumerable<object[]> InvalidNumberData => new List<object[]>
    {
        new object[]
        {
            (nint)1
        },
        new object[]
        {
            (nuint)1
        },
        new object[]
        {
            "1"
        }
    };

    [Fact]
    public void GetFieldCount_ReturnsExpectedFieldCount()
    {
        // Arrange
        var tableRows = new EFTableRows
        {
            FieldCount = 1
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.FieldCount;

        // Assert
        Assert.Equal(expected: 1, actual);
    }

    [Fact]
    public void GetHasRows_ReturnsTrue_WhenRowsArePresent()
    {
        // Arrange
        var values = new List<object>
        {
            new()
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.HasRows;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void GetHasRows_ReturnsFalse_WhenNoRowsArePresent()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.HasRows;

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void GetIsClosed_ReturnsTrue_WhenDataReaderIsClosed()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        dataReader.Close();

        // Assert
        var actual = dataReader.IsClosed;

        Assert.True(actual);
    }

    [Fact]
    public void GetIsClosed_ReturnsFalse_WhenDataReaderIsOpen()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.IsClosed;

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void GetDepth_ReturnsExpectedDepth()
    {
        // Arrange
        var values = new List<object>
        {
            new()
        };

        var tableRow = new EFTableRow(values)
        {
            Depth = 1
        };

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.Depth;

        // Assert
        Assert.Equal(expected: 1, actual);
    }

    [Fact]
    public void GetRecordsAffected_ReturnsMinusOne()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.RecordsAffected;

        // Assert
        Assert.Equal(expected: -1, actual);
    }

    [Fact]
    public void GetTableName_ReturnsExpectedTableName()
    {
        // Arrange
        var tableRows = new EFTableRows
        {
            TableName = "TestTable"
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.TableName;

        // Assert
        Assert.Equal(expected: "TestTable", actual);
    }

    [Fact]
    public void GetTableName_ReturnsGuid()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = Guid.TryParse(dataReader.TableName, out _);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Indexer_ReturnsExpectedValue_WhenNameIsValid()
    {
        // Arrange
        var values = new List<object>
        {
            123,
            "test",
            true
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        Name = "test",
                        Ordinal = 1
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader[name: "test"];

        // Assert
        Assert.Equal(expected: "test", actual);
    }

    [Fact]
    public void Indexer_ThrowsArgumentOutOfRangeException_WhenNameIsInvalid()
    {
        // Arrange
        var values = new List<object>
        {
            123,
            "test",
            true
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        dataReader.Read();

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => dataReader[name: "invalid"]);
    }

    [Fact]
    public void Indexer_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        var values = new List<object>
        {
            null
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        Name = "null",
                        Ordinal = 0
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader[name: "null"];

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void IndexerByOrdinal_ReturnsExpectedValue_WhenOrdinalIsValid()
    {
        // Arrange
        var values = new List<object>
        {
            123,
            "test",
            true
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader[ordinal: 1];

        // Assert
        Assert.Equal(expected: "test", actual);
    }

    [Fact]
    public void IndexerByOrdinal_ThrowsArgumentOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var values = new List<object>
        {
            123,
            "test",
            true
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act && Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => dataReader[ordinal: 5]);
    }

    [Fact]
    public void IndexerByOrdinal_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        var values = new List<object>
        {
            null
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader[ordinal: 0];

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void GetDataTypeName_ReturnsExpectedTypeName_WhenOrdinalIsValid()
    {
        // Arrange
        var tableRows = new EFTableRows
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        DbTypeName = "Int32"
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.GetDataTypeName(ordinal: 0);

        // Assert
        Assert.Equal(expected: "Int32", actual);
    }

    [Fact]
    public void GetDataTypeName_ThrowsKeyNotFoundException_WhenOrdinalNotFound()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Assert
        Assert.Throws<KeyNotFoundException>(() => dataReader.GetDataTypeName(ordinal: 5));
    }

    [Fact]
    public void GetDataTypeName_ReturnsEmptyString_WhenTypeNameIsEmpty()
    {
        // Arrange
        var tableRows = new EFTableRows
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        DbTypeName = string.Empty
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.GetDataTypeName(ordinal: 0);

        // Assert
        Assert.Equal(string.Empty, actual);
    }

    [Fact]
    public void GetFieldType_ReturnsExpectedType_WhenOrdinalIsValid()
    {
        // Arrange
        var tableRows = new EFTableRows
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        TypeName = typeof(int).ToString()
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.GetFieldType(ordinal: 0);

        // Assert
        Assert.Equal(typeof(int), actual);
    }

    [Fact]
    public void GetFieldType_ThrowsKeyNotFoundException_WhenOrdinalNotFound()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act && Assert
        Assert.Throws<KeyNotFoundException>(() => dataReader.GetFieldType(ordinal: 5));
    }

    [Fact]
    public void GetFieldType_ReturnsNull_WhenFieldTypeIsNull()
    {
        // Arrange
        var tableRows = new EFTableRows
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        TypeName = string.Empty
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.GetFieldType(ordinal: 0);

        // Assert
        Assert.Equal(typeof(string), actual);
    }

    [Fact]
    public void GetName_ReturnsExpectedName_WhenOrdinalIsValid()
    {
        // Arrange
        var expected = Guid.NewGuid().ToString();

        var tableRows = new EFTableRows
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        Name = expected
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.GetName(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetName_ThrowsKeyNotFoundException_WhenOrdinalNotFound()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Assert
        Assert.Throws<KeyNotFoundException>(() => dataReader.GetName(ordinal: 5));
    }

    [Fact]
    public void GetName_ReturnsEmptyString_WhenNameIsEmpty()
    {
        // Arrange
        var tableRows = new EFTableRows
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        Name = string.Empty
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.GetName(ordinal: 0);

        // Assert
        Assert.Equal(string.Empty, actual);
    }

    [Fact]
    public void GetSchemaTable_ThrowsInvalidOperationException()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act && Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            using var data = dataReader.GetSchemaTable();
        });
    }

    [Fact]
    public void NextResult_ReturnsFalse()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.NextResult();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Close_SetsIsClosedToTrue()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        dataReader.Close();

        // Assert
        Assert.True(dataReader.IsClosed);
    }

    [Fact]
    public void Read_ReturnsTrue_WhenCurrentRowIsWithinRange()
    {
        // Arrange
        var values = new List<object>
        {
            123,
            "test",
            true
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.Read();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Read_ReturnsFalse_WhenCurrentRowIsOutOfRange()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.Read();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Read_UpdatesRowValues_WhenCurrentRowIsWithinRange()
    {
        // Arrange
        var values = new List<object>
        {
            123,
            "test",
            true
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        dataReader.Read();

        // Assert
        var actual = dataReader.GetValue(ordinal: 0);

        Assert.Equal(expected: 123, actual);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(1L, true)]
    [InlineData(0L, false)]
    [InlineData(1UL, true)]
    [InlineData(0UL, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    public void GetBoolean_ReturnsExpectedValue(object value, bool expected)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetBoolean(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(GetByteData))]
    public void GetByte_ReturnsExpectedValue(object value, byte expected)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetByte(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetByte_ThrowsFormatException_WhenValueIsEmptyString()
    {
        // Arrange
        var values = new List<object>
        {
            string.Empty
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act && Assert
        Assert.Throws<FormatException>(() => dataReader.GetByte(ordinal: 0));
    }

    [Fact]
    public void GetByte_ThrowsFormatException_WhenValueIsWhiteSpace()
    {
        // Arrange
        var values = new List<object>
        {
            " "
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act && Assert
        Assert.Throws<FormatException>(() => dataReader.GetByte(ordinal: 0));
    }

    [Fact]
    public void GetBytes_ReturnsZero_WhenBufferIsNull()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.GetBytes(ordinal: 0, dataOffset: 0, buffer: null, bufferOffset: 0, length: 0);

        // Assert
        Assert.Equal(expected: 0L, actual);
    }

    [Fact]
    public void GetBytes_ReturnsZero_WhenBufferIsNotNull()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());
        var buffer = new byte[10];

        // Act
        var actual = dataReader.GetBytes(ordinal: 0, dataOffset: 0, buffer, bufferOffset: 0, buffer.Length);

        // Assert
        Assert.Equal(expected: 0L, actual);
    }

    [Fact]
    public void GetBytes_ReturnsZero_WhenDataOffsetIsNonZero()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());
        var buffer = new byte[10];

        // Act
        var actual = dataReader.GetBytes(ordinal: 0, dataOffset: 5, buffer, bufferOffset: 0, buffer.Length);

        // Assert
        Assert.Equal(expected: 0L, actual);
    }

    [Theory]
    [InlineData(null, default(char))]
    [InlineData("", default(char))]
    [InlineData(" ", default(char))]
    [InlineData("A", 'A')]
    [InlineData("65", 'A')]
    [InlineData(65L, 'A')]
    [InlineData(65, 'A')]
    public void GetChar_ReturnsExpectedValue(object value, char expected)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetChar(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetChars_ReturnsZero_WhenBufferIsNull()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act
        var actual = dataReader.GetChars(ordinal: 0, dataOffset: 0, buffer: null, bufferOffset: 0, length: 0);

        // Assert
        Assert.Equal(expected: 0L, actual);
    }

    [Fact]
    public void GetChars_ReturnsZero_WhenBufferIsNotNull()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());
        var buffer = new char[10];

        // Act
        var actual = dataReader.GetChars(ordinal: 0, dataOffset: 0, buffer, bufferOffset: 0, buffer.Length);

        // Assert
        Assert.Equal(expected: 0L, actual);
    }

    [Fact]
    public void GetChars_ReturnsZero_WhenDataOffsetIsNonZero()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());
        var buffer = new char[10];

        // Act
        var actual = dataReader.GetChars(ordinal: 0, dataOffset: 5, buffer, bufferOffset: 0, buffer.Length);

        // Assert
        Assert.Equal(expected: 0L, actual);
    }

    [Fact]
    public void GetChars_ReturnsZero_WhenBufferOffsetIsNonZero()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());
        var buffer = new char[10];

        // Act
        var actual = dataReader.GetChars(ordinal: 0, dataOffset: 0, buffer, bufferOffset: 5, buffer.Length - 5);

        // Assert
        Assert.Equal(expected: 0L, actual);
    }

    [Fact]
    public void GetChars_ReturnsZero_WhenLengthIsNonZero()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());
        var buffer = new char[10];

        // Act
        var actual = dataReader.GetChars(ordinal: 0, dataOffset: 0, buffer, bufferOffset: 0, length: 5);

        // Assert
        Assert.Equal(expected: 0L, actual);
    }

    [Theory]
    [MemberData(nameof(GetDateTimeData))]
    public void GetDateTime_ReturnsExpectedValue(object value, DateTime expected)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetDateTime(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(GetDecimalData))]
    public void GetDecimal_ReturnsExpectedValue(object value, decimal expected)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetDecimal(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(GetDoubleData))]
    public void GetDouble_ReturnsExpectedValue(object value, double expected)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetDouble(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetDouble_ThrowsFormatException_WhenValueIsEmptyString()
    {
        // Arrange
        var values = new List<object>
        {
            string.Empty
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act && Assert
        Assert.Throws<FormatException>(() => dataReader.GetDouble(ordinal: 0));
    }

    [Fact]
    public void GetDouble_ThrowsFormatException_WhenValueIsWhiteSpace()
    {
        // Arrange
        var values = new List<object>
        {
            " "
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act && Assert
        Assert.Throws<FormatException>(() => dataReader.GetDouble(ordinal: 0));
    }

    [Fact]
    public void GetEnumerator_ThrowsNotSupportedException()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        // Act && Assert
        Assert.Throws<NotSupportedException>(() => dataReader.GetEnumerator());
    }

    [Theory]
    [MemberData(nameof(GetFloatData))]
    public void GetFloat_ReturnsExpectedValue(object value, float expected)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFloat(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetFloat_ThrowsFormatException_WhenValueIsEmptyString()
    {
        // Arrange
        var values = new List<object>
        {
            string.Empty
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act && Assert
        Assert.Throws<FormatException>(() => dataReader.GetFloat(ordinal: 0));
    }

    [Fact]
    public void GetFloat_ThrowsFormatException_WhenValueIsWhiteSpace()
    {
        // Arrange
        var values = new List<object>
        {
            " "
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act && Assert
        Assert.Throws<FormatException>(() => dataReader.GetFloat(ordinal: 0));
    }

    [Theory]
    [MemberData(nameof(GetGuidData))]
    public void GetGuid_ReturnsExpectedValue(object value, Guid expected)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetGuid(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetGuid_ReturnsGuid_WhenValueIsNull()
    {
        // Arrange
        var values = new List<object>
        {
            null
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetGuid(ordinal: 0);

        // Assert
        Assert.IsType<Guid>(actual);
    }

    [Fact]
    public void GetGuid_ReturnsGuid_WhenValueIsEmptyString()
    {
        // Arrange
        var values = new List<object>
        {
            string.Empty
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetGuid(ordinal: 0);

        // Assert
        Assert.IsType<Guid>(actual);
    }

    [Fact]
    public void GetGuid_ReturnsGuid_WhenValueIsWhiteSpace()
    {
        // Arrange
        var values = new List<object>
        {
            " "
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetGuid(ordinal: 0);

        // Assert
        Assert.IsType<Guid>(actual);
    }

    [Theory]
    [MemberData(nameof(GetInt16Data))]
    public void GetInt16_ReturnsExpectedValue(object value, short expected)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetInt16(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(GetInt32Data))]
    public void GetInt32_ReturnsExpectedValue(object value, int expected)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetInt32(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(GetInt64Data))]
    public void GetInt64_ReturnsExpectedValue(object value, long expected)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetInt64(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(GetStringData))]
    public void GetString_ReturnsExpectedValue(object value, string expected)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetString(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetString_ReturnsExpectedValueFromInvariantDecimal()
    {
        // Arrange
        var values = new List<object>
        {
            123.45.ToString(CultureInfo.InvariantCulture)
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetString(ordinal: 0);

        // Assert
        Assert.Equal(expected: "123.45", actual);
    }

    [Fact]
    public void GetValue_ReturnsExpectedValue_WhenOrdinalIsValid()
    {
        // Arrange
        var values = new List<object>
        {
            123,
            "test",
            true
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetValue(ordinal: 1);

        // Assert
        Assert.Equal(expected: "test", actual);
    }

    [Fact]
    public void GetValue_ThrowsArgumentOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var values = new List<object>
        {
            123,
            "test",
            true
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act && Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => dataReader.GetValue(ordinal: 5));
    }

    [Fact]
    public void GetValue_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        var values = new List<object>
        {
            null
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetValue(ordinal: 0);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void GetValue_ReturnsEmptyString_WhenValueIsEmptyString()
    {
        // Arrange
        var values = new List<object>
        {
            string.Empty
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetValue(ordinal: 0);

        // Assert
        Assert.Equal(string.Empty, actual);
    }

    [Fact]
    public void GetValue_ReturnsBoolean_WhenValueIsBoolean()
    {
        // Arrange
        var values = new List<object>
        {
            true
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetValue(ordinal: 0);

        // Assert
        Assert.Equal(expected: true, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedByteValue()
    {
        // Arrange
        const byte expected = 123;

        var values = new List<object>
        {
            expected
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<byte>(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedBoolValue()
    {
        // Arrange
        var values = new List<object>
        {
            true
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<bool>(ordinal: 0);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedBoolValue_WhenNumberIsULongType()
    {
        // Arrange
        var values = new List<object>
        {
            (ulong)1
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<bool>(ordinal: 0);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void GetFieldValue_ThrowsInvalidCastException_WhenNumberIsNotULongType()
    {
        // Arrange
        var values = new List<object>
        {
            (long)1
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        void Act() => dataReader.GetFieldValue<bool>(ordinal: 0);

        // Assert
        Assert.Throws<InvalidCastException>(Act);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedBoolValueFromString()
    {
        // Arrange
        var values = new List<object>
        {
            bool.TrueString
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        DbTypeName = nameof(Boolean)
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        void Act() => dataReader.GetFieldValue<bool>(ordinal: 0);

        // Assert
        Assert.Throws<InvalidCastException>(Act);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedDateTimeOffsetValueFromDateTime()
    {
        // Arrange
        var values = new List<object>
        {
            DateTime.MaxValue
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<DateTimeOffset>(ordinal: 0);

        // Assert
        Assert.Equal(DateTimeOffset.MaxValue.Date, actual.Date);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedDateTimeOffsetValueFromString()
    {
        // Arrange
        var values = new List<object>
        {
            DateTime.MaxValue.ToString(CultureInfo.InvariantCulture)
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<DateTimeOffset>(ordinal: 0);

        // Assert
        Assert.Equal(DateTimeOffset.MaxValue.Date, actual.Date);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedTimeSpanValueFromString()
    {
        // Arrange
        var values = new List<object>
        {
            TimeSpan.MaxValue.ToString()
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<TimeSpan>(ordinal: 0);

        // Assert
        Assert.Equal(TimeSpan.MaxValue, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedTimeSpanValueFromNumber()
    {
        // Arrange
        var values = new List<object>
        {
            TimeSpan.MaxValue.Ticks
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<TimeSpan>(ordinal: 0);

        // Assert
        Assert.Equal(TimeSpan.MaxValue, actual);
    }

    [Theory]
    [MemberData(nameof(ValidNumberData))]
    public void GetFieldValue_ShouldReturnExpectedDecimalNumber(object value)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        DbTypeName = value.GetType().Name,
                        Ordinal = 0
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<decimal>(ordinal: 0);

        // Assert
        Assert.Equal(expected: 1M, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedByteNumberFromChar()
    {
        // Arrange
        var values = new List<object>
        {
            '1'
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        DbTypeName = nameof(Char),
                        Ordinal = 0
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<byte>(ordinal: 0);

        // Assert
        Assert.Equal(expected: 49, actual);
    }

    [Theory]
    [MemberData(nameof(ValidNumberData))]
    public void GetFieldValue_ShouldNotThrowInvalidCastExceptionWhenConvertToDecimal(object value)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        DbTypeName = value.GetType().Name,
                        Ordinal = 0
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = Record.Exception(() => dataReader.GetFieldValue<decimal>(ordinal: 0));

        // Assert
        Assert.Null(actual);
    }

    [Theory]
    [MemberData(nameof(InvalidNumberData))]
    public void GetFieldValue_ThrowsInvalidCastException_WhenValueIsNotNumber(object value)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        DbTypeName = value.GetType().Name,
                        Ordinal = 0
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        void Act() => dataReader.GetFieldValue<decimal>(ordinal: 0);

        // Assert
        Assert.Throws<InvalidCastException>(Act);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void GetFieldValue_ShouldReturnExpectedNumberValueFromBool(bool value, int expected)
    {
        // Arrange
        var values = new List<object>
        {
            value
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<int>(ordinal: 0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedArray()
    {
        // Arrange
        var values = new List<object>
        {
            new object[]
            {
                1, 2, 3
            }
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<int[]>(ordinal: 0);

        // Assert
        Assert.Equal(new[]
        {
            1, 2, 3
        }, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedList()
    {
        // Arrange
        var values = new List<object>
        {
            new[]
            {
                1, 2, 3
            }
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<List<int>>(ordinal: 0);

        // Assert
        Assert.Equal(new List<int>
        {
            1,
            2,
            3
        }, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedObject()
    {
        // Arrange
        var values = new List<object>
        {
            "{}"
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        DbTypeName = "jsonb",
                        Ordinal = 0
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = JsonSerializer.Serialize(dataReader.GetFieldValue<object>(ordinal: 0));

        // Assert
        Assert.Equal(expected: "{}", actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedDateOnlyValueFromDateTime()
    {
        // Arrange
        var values = new List<object>
        {
            DateTime.MaxValue
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        DbTypeName = nameof(DateTime),
                        Ordinal = 0
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<DateOnly>(ordinal: 0);

        // Assert
        Assert.Equal(DateOnly.MaxValue, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedDateOnlyValueFromString()
    {
        // Arrange
        var values = new List<object>
        {
            DateOnly.MaxValue.ToString(CultureInfo.InvariantCulture)
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        DbTypeName = nameof(String),
                        Ordinal = 0
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<DateOnly>(ordinal: 0);

        // Assert
        Assert.Equal(DateOnly.MaxValue, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldNotThrowInvalidCastExceptionWhenValueConversionFromStringToDateOnly()
    {
        // Arrange
        var values = new List<object>
        {
            DateOnly.MaxValue.ToString(CultureInfo.InvariantCulture)
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        DbTypeName = nameof(String),
                        Ordinal = 0
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = Record.Exception(() => dataReader.GetFieldValue<DateOnly>(ordinal: 0));

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedTimeOnlyValueFromTimeSpan()
    {
        // Arrange
        var values = new List<object>
        {
            new TimeSpan(ticks: 863999999999)
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        DbTypeName = nameof(TimeSpan),
                        Ordinal = 0
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<TimeOnly>(ordinal: 0);

        // Assert
        Assert.Equal(TimeOnly.MaxValue, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedTimeOnlyValueFromNumber()
    {
        // Arrange
        var values = new List<object>
        {
            863999999999
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        DbTypeName = nameof(TimeSpan),
                        Ordinal = 0
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<TimeOnly>(ordinal: 0);

        // Assert
        Assert.Equal(TimeOnly.MaxValue, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedTimeOnlyValueFromString()
    {
        // Arrange
        var values = new List<object>
        {
            TimeOnly.MaxValue.ToString(format: "O", CultureInfo.InvariantCulture)
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        DbTypeName = nameof(String),
                        Ordinal = 0
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<TimeOnly>(ordinal: 0);

        // Assert
        Assert.Equal(TimeOnly.MaxValue, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldNotThrowInvalidCastExceptionWhenValueConversionFromStringToTimeOnly()
    {
        // Arrange
        var values = new List<object>
        {
            TimeOnly.MaxValue.ToString(CultureInfo.InvariantCulture)
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        DbTypeName = nameof(String),
                        Ordinal = 0
                    }
                }
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = Record.Exception(() => dataReader.GetFieldValue<TimeOnly>(ordinal: 0));

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void GetValues_CopiesRowValuesToArray()
    {
        // Arrange
        var expected = new List<object>
        {
            123,
            "test",
            true
        };

        var tableRow = new EFTableRow(expected);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = new object[expected.Count];

        dataReader.GetValues(actual);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetValues_ReturnsRowValuesCount()
    {
        // Arrange
        var values = new List<object>
        {
            123,
            "test",
            true
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.GetValues(new object[values.Count]);

        // Assert
        Assert.Equal(values.Count, actual);
    }

    [Fact]
    public void GetValues_CopiesEmptyRowValuesToArray()
    {
        // Arrange
        var tableRows = new EFTableRows();
        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = Array.Empty<object>();

        dataReader.GetValues(actual);

        // Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void GetValues_CopiesPartialRowValuesToArray()
    {
        // Arrange
        var values = new List<object>
        {
            123,
            "test",
            true
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = new object[values.Count + 1];

        dataReader.GetValues(actual);

        // Assert
        Assert.Equal(actual.Take(values.Count), values);
    }

    [Fact]
    public void IsDBNull_ReturnsTrue_WhenValueIsNull()
    {
        // Arrange
        var values = new List<object>
        {
            null
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.IsDBNull(ordinal: 0);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsDBNull_ReturnsFalse_WhenValueIsNotNull()
    {
        // Arrange
        var values = new List<object>
        {
            123,
            "test",
            true
        };

        var tableRow = new EFTableRow(values);

        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow>
            {
                tableRow
            }
        };

        using var dataReader = new EFTableRowsDataReader(tableRows, new EFCoreSecondLevelCacheSettings());

        dataReader.Read();

        // Act
        var actual = dataReader.IsDBNull(ordinal: 0);

        // Assert
        Assert.False(actual);
    }
}