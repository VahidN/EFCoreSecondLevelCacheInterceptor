using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     SqlCommands Utils
/// </summary>
public class EFSqlCommandsProcessor : IEFSqlCommandsProcessor
{
    private static readonly string[] CrudMarkers =
    {
        "MERGE ", "insert ", "update ", "delete ", "create "
    };

    private static readonly Type IEntityType =
        Type.GetType("Microsoft.EntityFrameworkCore.Metadata.IEntityType, Microsoft.EntityFrameworkCore") ??
        throw new TypeLoadException("Couldn't load Microsoft.EntityFrameworkCore.Metadata.IEntityType");

    private static readonly PropertyInfo ClrTypePropertyInfo = IEntityType.GetInterfaces().Union(new[]
                                                                   {
                                                                       IEntityType
                                                                   }).Select(i => i.GetProperty("ClrType",
                                                                       BindingFlags.Public | BindingFlags.Instance))
                                                                   .Distinct()
                                                                   .FirstOrDefault(propertyInfo
                                                                       => propertyInfo != null) ??
                                                               throw new KeyNotFoundException(
                                                                   "Couldn't find `ClrType` on IEntityType.");

    private static readonly Type RelationalEntityTypeExtensionsType =
        Type.GetType(
            "Microsoft.EntityFrameworkCore.RelationalEntityTypeExtensions, Microsoft.EntityFrameworkCore.Relational") ??
        throw new TypeLoadException("Couldn't load Microsoft.EntityFrameworkCore.RelationalEntityTypeExtensions");

    private static readonly MethodInfo GetTableNameMethodInfo =
        RelationalEntityTypeExtensionsType.GetMethod("GetTableName", BindingFlags.Static | BindingFlags.Public) ??
        throw new KeyNotFoundException("Couldn't find `GetTableName()` on RelationalEntityTypeExtensions.");

    private static readonly string[] Separator =
    {
        "."
    };

    private readonly ConcurrentDictionary<string, Lazy<SortedSet<string>>> _commandTableNames =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<Type, Lazy<List<TableEntityInfo>>> _contextTableNames = new();
    private readonly IEFHashProvider _hashProvider;

    /// <summary>
    ///     SqlCommands Utils
    /// </summary>
    public EFSqlCommandsProcessor(IEFHashProvider hashProvider)
        => _hashProvider = hashProvider ?? throw new ArgumentNullException(nameof(hashProvider));

    /// <summary>
    ///     Is `insert`, `update` or `delete`?
    /// </summary>
    public bool IsCrudCommand(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            foreach (var marker in CrudMarkers)
            {
                if (line.Trim().StartsWith(marker, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    ///     Returns all of the given context's table names.
    /// </summary>
    public IList<TableEntityInfo> GetAllTableNames(DbContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return _contextTableNames.GetOrAdd(context.GetType(),
            _ => new Lazy<List<TableEntityInfo>>(() => getTableNames(context),
                LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    /// <summary>
    ///     Extracts the table names of an SQL command.
    /// </summary>
    public SortedSet<string> GetSqlCommandTableNames(string commandText)
    {
        var commandTextKey = $"{_hashProvider.ComputeHash(commandText):X}";
        return _commandTableNames.GetOrAdd(commandTextKey,
            _ => new Lazy<SortedSet<string>>(() => getRawSqlCommandTableNames(commandText),
                LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    /// <summary>
    ///     Extracts the entity types of an SQL command.
    /// </summary>
    public IList<Type> GetSqlCommandEntityTypes(string commandText, IList<TableEntityInfo> allEntityTypes)
    {
        var commandTableNames = GetSqlCommandTableNames(commandText);
        return allEntityTypes.Where(entityType => commandTableNames.Contains(entityType.TableName))
            .Select(entityType => entityType.ClrType).ToList();
    }

    private static List<TableEntityInfo> getTableNames(DbContext context)
    {
        var tableNames = new List<TableEntityInfo>();
        foreach (var entityType in context.Model.GetEntityTypes())
        {
            var clrType = getClrType(entityType);
            tableNames.Add(new TableEntityInfo
            {
                ClrType = clrType,
                TableName = getTableName(entityType) ?? clrType.ToString()
            });
        }

        return tableNames;
    }

    private static string? getTableName(object entityType)
        => GetTableNameMethodInfo.Invoke(null, new[]
        {
            entityType
        }) as string;

    private static Type getClrType(object entityType)
    {
        var value = ClrTypePropertyInfo.GetValue(entityType) ??
                    throw new InvalidOperationException($"Couldn't get the ClrType value of `{entityType}`");
        return (Type)value;
    }

    private static SortedSet<string> getRawSqlCommandTableNames(string commandText)
    {
        string[] tableMarkers =
        {
            "FROM", "JOIN", "INTO", "UPDATE", "MERGE"
        };

        var tables = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        var sqlItems = commandText.Split(new[]
        {
            " ", "\r\n", Environment.NewLine, "\n"
        }, StringSplitOptions.RemoveEmptyEntries);
        var sqlItemsLength = sqlItems.Length;
        for (var i = 0; i < sqlItemsLength; i++)
        {
            foreach (var marker in tableMarkers)
            {
                if (!sqlItems[i].Equals(marker, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ++i;
                if (i >= sqlItemsLength)
                {
                    break;
                }

                var tableName = string.Empty;

                var tableNameParts = sqlItems[i].Split(Separator, StringSplitOptions.RemoveEmptyEntries);
                if (tableNameParts.Length == 1)
                {
                    tableName = tableNameParts[0].Trim();
                }
                else if (tableNameParts.Length >= 2)
                {
                    tableName = tableNameParts[1].Trim();
                }

                if (string.IsNullOrWhiteSpace(tableName))
                {
                    continue;
                }

                tableName = tableName.Replace("[", "", StringComparison.Ordinal)
                    .Replace("]", "", StringComparison.Ordinal).Replace("'", "", StringComparison.Ordinal)
                    .Replace("`", "", StringComparison.Ordinal).Replace("\"", "", StringComparison.Ordinal);
                tables.Add(tableName);
            }
        }

        return tables;
    }
}