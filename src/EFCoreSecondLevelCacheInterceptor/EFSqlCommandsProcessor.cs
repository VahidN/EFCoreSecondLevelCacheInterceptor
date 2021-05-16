using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// SqlCommands Utils
    /// </summary>
    public class EFSqlCommandsProcessor : IEFSqlCommandsProcessor
    {
        private readonly ConcurrentDictionary<Type, Lazy<List<TableEntityInfo>>> _contextTableNames = new();
        private readonly ConcurrentDictionary<string, Lazy<SortedSet<string>>> _commandTableNames = new(StringComparer.OrdinalIgnoreCase);
        private readonly IEFHashProvider _hashProvider;

        /// <summary>
        /// SqlCommands Utils
        /// </summary>
        public EFSqlCommandsProcessor(IEFHashProvider hashProvider)
        {
            _hashProvider = hashProvider ?? throw new ArgumentNullException(nameof(hashProvider));
        }

        /// <summary>
        /// Is `insert`, `update` or `delete`?
        /// </summary>
        public bool IsCrudCommand(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            string[] crudMarkers = { "insert ", "update ", "delete ", "create " };

            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                foreach (var marker in crudMarkers)
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
        /// Returns all of the given context's table names.
        /// </summary>
        public IList<TableEntityInfo> GetAllTableNames(DbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return _contextTableNames.GetOrAdd(context.GetType(),
                            _ => new Lazy<List<TableEntityInfo>>(() =>
                            {
                                var tableNames = new List<TableEntityInfo>();
                                foreach (var entityType in context.Model.GetEntityTypes())
                                {
                                    tableNames.Add(
                                        new TableEntityInfo
                                        {
                                            ClrType = entityType.ClrType,
                                            TableName = entityType.GetTableName() ?? entityType.ClrType.ToString()
                                        });
                                }
                                return tableNames;
                            },
                            LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        /// <summary>
        /// Extracts the table names of an SQL command.
        /// </summary>
        public SortedSet<string> GetSqlCommandTableNames(string commandText)
        {
            var commandTextKey = $"{_hashProvider.ComputeHash(commandText):X}";
            return _commandTableNames.GetOrAdd(commandTextKey,
                    _ => new Lazy<SortedSet<string>>(() => getRawSqlCommandTableNames(commandText),
                            LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        /// <summary>
        /// Extracts the entity types of an SQL command.
        /// </summary>
        public IList<Type> GetSqlCommandEntityTypes(string commandText, IList<TableEntityInfo> allEntityTypes)
        {
            var commandTableNames = GetSqlCommandTableNames(commandText);
            return allEntityTypes.Where(entityType => commandTableNames.Contains(entityType.TableName))
                                .Select(entityType => entityType.ClrType)
                                .ToList();
        }

        private static SortedSet<string> getRawSqlCommandTableNames(string commandText)
        {
            string[] tableMarkers = { "FROM", "JOIN", "INTO", "UPDATE" };

            var tables = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            var sqlItems = commandText.Split(new[] { " ", "\r\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
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
                        continue;
                    }

                    var tableName = string.Empty;

                    var tableNameParts = sqlItems[i].Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
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
                                        .Replace("]", "", StringComparison.Ordinal)
                                        .Replace("'", "", StringComparison.Ordinal)
                                        .Replace("`", "", StringComparison.Ordinal)
                                        .Replace("\"", "", StringComparison.Ordinal);
                    tables.Add(tableName);
                }
            }
            return tables;
        }
    }
}