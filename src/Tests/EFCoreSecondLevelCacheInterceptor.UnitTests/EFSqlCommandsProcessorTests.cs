using Assert = Xunit.Assert;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

/// <summary>
///     Tests for EFSqlCommandsProcessor.GetRawSqlCommandTableNames
/// </summary>
public class EFSqlCommandsProcessorTests
{
    private readonly EFSqlCommandsProcessor _sqlCommandsProcessor;

    public EFSqlCommandsProcessorTests() => _sqlCommandsProcessor = new EFSqlCommandsProcessor(new XxHash64Unsafe());

    [Fact]
    public void GetSqlCommandTableNames_WithSimpleTableName_ReturnsCorrectTableName()
    {
        // Arrange
        const string commandText = @"SELECT * FROM [Users]";

        // Act
        var tableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

        // Assert
        Assert.Single(tableNames);
        Assert.Contains(expected: "Users", tableNames);
    }

    [Fact]
    public void GetSqlCommandTableNames_WithSchemaQualifiedTableName_ReturnsOnlyTableName()
    {
        // Arrange
        const string commandText = @"SELECT * FROM dbo.[Users]";

        // Act
        var tableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

        // Assert
        Assert.Single(tableNames);
        Assert.Contains(expected: "Users", tableNames);
    }

    [Fact]
    public void GetSqlCommandTableNames_WithBracketedSchemaAndTableName_ReturnsOnlyTableName()
    {
        // Arrange
        const string commandText = @"SELECT * FROM [dbo].[Users]";

        // Act
        var tableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

        // Assert
        Assert.Single(tableNames);
        Assert.Contains(expected: "Users", tableNames);
    }

    [Fact]
    public void GetSqlCommandTableNames_WithBracketedTableNameContainingDot_ReturnsBracketedName()
    {
        // Arrange - Issue #333: Table name with dot inside brackets
        const string commandText = @"SELECT * FROM [Library.Tag]";

        // Act
        var tableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

        // Assert
        Assert.Single(tableNames);
        Assert.Contains(expected: "Library.Tag", tableNames);
    }

    [Fact]
    public void GetSqlCommandTableNames_WithMultipleBracketedTableNamesContainingDots_ReturnsAllNames()
    {
        // Arrange - Issue #333: Multiple tables with dots inside brackets
        const string commandText = @"
            SELECT * FROM [Library.Tag] AS t
            INNER JOIN [Library.CostCenter] AS c ON t.Id = c.Id";

        // Act
        var tableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

        // Assert
        Assert.Equal(expected: 2, tableNames.Count);
        Assert.Contains(expected: "Library.Tag", tableNames);
        Assert.Contains(expected: "Library.CostCenter", tableNames);
    }

    [Fact]
    public void GetSqlCommandTableNames_WithBracketedTableNameAndJoin_ReturnsBothNames()
    {
        // Arrange
        const string commandText = @"
            SELECT * FROM [Library.Tag] AS t
            INNER JOIN [Users] AS u ON t.Id = u.Id";

        // Act
        var tableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

        // Assert
        Assert.Equal(expected: 2, tableNames.Count);
        Assert.Contains(expected: "Library.Tag", tableNames);
        Assert.Contains(expected: "Users", tableNames);
    }

    [Fact]
    public void GetSqlCommandTableNames_WithBracketedTableNameInInsert_ReturnsBracketedName()
    {
        // Arrange
        const string commandText = @"INSERT INTO [Library.Tag] (Name) VALUES ('test')";

        // Act
        var tableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

        // Assert
        Assert.Single(tableNames);
        Assert.Contains(expected: "Library.Tag", tableNames);
    }

    [Fact]
    public void GetSqlCommandTableNames_WithBracketedTableNameInUpdate_ReturnsBracketedName()
    {
        // Arrange
        const string commandText = @"UPDATE [Library.Tag] SET Name = 'test' WHERE Id = 1";

        // Act
        var tableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

        // Assert
        Assert.Single(tableNames);
        Assert.Contains(expected: "Library.Tag", tableNames);
    }

    [Fact]
    public void GetSqlCommandTableNames_WithBracketedTableNameInDelete_ReturnsBracketedName()
    {
        // Arrange
        const string commandText = @"DELETE FROM [Library.Tag] WHERE Id = 1";

        // Act
        var tableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

        // Assert
        Assert.Single(tableNames);
        Assert.Contains(expected: "Library.Tag", tableNames);
    }

    [Fact]
    public void GetSqlCommandTableNames_WithBracketedTableNameInMerge_ReturnsBracketedName()
    {
        // Arrange
        const string commandText = @"
            MERGE [Library.Tag] AS target
            USING (SELECT 1 as Id) AS source
            ON target.Id = source.Id";

        // Act
        var tableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

        // Assert
        Assert.Single(tableNames);
        Assert.Contains(expected: "Library.Tag", tableNames);
    }

    [Fact]
    public void GetSqlCommandTableNames_WithComplexQueryMultipleBracketedDotsAndSchemas_ReturnsAllNames()
    {
        // Arrange - Complex scenario mixing normal and dotted table names
        const string commandText = @"
            SELECT *
            FROM [Library.Tag] AS lt
            INNER JOIN dbo.[Posts] AS p ON lt.Id = p.Id
            LEFT JOIN [Catalog.Product] AS cp ON p.Id = cp.Id
            WHERE lt.Id = 1";

        // Act
        var tableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

        // Assert
        Assert.Equal(expected: 3, tableNames.Count);
        Assert.Contains(expected: "Library.Tag", tableNames);
        Assert.Contains(expected: "Posts", tableNames);
        Assert.Contains(expected: "Catalog.Product", tableNames);
    }

    [Fact]
    public void GetSqlCommandTableNames_WithUnbracketedTableName_ReturnsTableName()
    {
        // Arrange
        const string commandText = @"SELECT * FROM Users";

        // Act
        var tableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

        // Assert
        Assert.Single(tableNames);
        Assert.Contains(expected: "Users", tableNames);
    }

    [Fact]
    public void GetSqlCommandTableNames_WithMultipleFromClauses_ReturnsLastTableName()
    {
        // Arrange - This tests behavior with FROM appearing multiple times
        const string commandText = @"
            SELECT * FROM [Library.Tag]
            INNER JOIN [Users] ON [Library.Tag].Id = [Users].Id";

        // Act
        var tableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

        // Assert
        Assert.Equal(expected: 2, tableNames.Count);
        Assert.Contains(expected: "Library.Tag", tableNames);
        Assert.Contains(expected: "Users", tableNames);
    }

    [Fact]
    public void IsCrudCommand_WithInsertCommand_ReturnsTrue()
    {
        // Arrange
        const string commandText = "INSERT INTO Users VALUES (1, 'Test')";

        // Act
        var isCrud = _sqlCommandsProcessor.IsCrudCommand(commandText);

        // Assert
        Assert.True(isCrud);
    }

    [Fact]
    public void IsCrudCommand_WithUpdateCommand_ReturnsTrue()
    {
        // Arrange
        const string commandText = "UPDATE Users SET Name = 'Test' WHERE Id = 1";

        // Act
        var isCrud = _sqlCommandsProcessor.IsCrudCommand(commandText);

        // Assert
        Assert.True(isCrud);
    }

    [Fact]
    public void IsCrudCommand_WithDeleteCommand_ReturnsTrue()
    {
        // Arrange
        const string commandText = "DELETE FROM Users WHERE Id = 1";

        // Act
        var isCrud = _sqlCommandsProcessor.IsCrudCommand(commandText);

        // Assert
        Assert.True(isCrud);
    }

    [Fact]
    public void IsCrudCommand_WithSelectCommand_ReturnsFalse()
    {
        // Arrange
        const string commandText = "SELECT * FROM Users";

        // Act
        var isCrud = _sqlCommandsProcessor.IsCrudCommand(commandText);

        // Assert
        Assert.False(isCrud);
    }

    [Fact]
    public void IsCrudCommand_WithNullCommand_ReturnsFalse()
    {
        // Arrange
        string commandText = null!;

        // Act
        var isCrud = _sqlCommandsProcessor.IsCrudCommand(commandText);

        // Assert
        Assert.False(isCrud);
    }

    [Fact]
    public void IsCrudCommand_WithEmptyCommand_ReturnsFalse()
    {
        // Arrange
        const string commandText = "";

        // Act
        var isCrud = _sqlCommandsProcessor.IsCrudCommand(commandText);

        // Assert
        Assert.False(isCrud);
    }
}