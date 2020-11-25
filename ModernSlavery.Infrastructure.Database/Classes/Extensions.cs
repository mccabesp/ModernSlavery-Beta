﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Infrastructure.Database.Classes
{
    public static partial class Extensions
    {
        internal static List<T> SqlQuery<T>(this IDbConnection connection, string query, ILogger logger = null)
        {
            if (connection.State != ConnectionState.Open) connection.Open();

            //context.Database.OpenConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                using (var result = command.ExecuteReader())
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    var list = new List<T>();
                    T obj = default;
                    var type = typeof(T);

                    while (result.Read())
                    {
                        if (type.IsSimpleType())
                        {
                            obj = (T) result.GetValue(0);
                        }
                        else
                        {
                            obj = Activator.CreateInstance<T>();

                            foreach (var prop in obj.GetType().GetProperties())
                                if (!Equals(result[prop.Name], DBNull.Value))
                                    prop.SetValue(obj, result[prop.Name], null);

                            foreach (var field in obj.GetType().GetFields())
                                if (!Equals(result[field.Name], DBNull.Value))
                                    field.SetValue(obj, result[field.Name]);
                        }

                        list.Add(obj);
                    }

                    sw.Stop();
                    logger?.LogInformation($"Executed ({sw.ElapsedMilliseconds}ms)");
                    logger?.LogInformation($"{query}");

                    return list;
                }
            }
        }


        internal static int TableCount(this IDbConnection connection, string tableName = null,
            string schemaName = "dbo")
        {
            var query = "SELECT Count(*) FROM sys.tables";
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                query += $" t WHERE t.name = '{tableName}'";
                if (!string.IsNullOrWhiteSpace(schemaName))
                    query =
                        $"SELECT Count(*) FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = '{schemaName}' AND t.name = '{tableName}'";
            }

            var count = connection.SqlQuery<int>(query).FirstOrDefault().ToInt32();
            return count;
        }

        internal static int ViewCount(this IDbConnection connection, string viewName = null, string schemaName = "dbo")
        {
            var query = "SELECT Count(*) FROM sys.views";
            if (!string.IsNullOrWhiteSpace(viewName))
            {
                query += $" v WHERE v.name = '{viewName}'";
                if (!string.IsNullOrWhiteSpace(schemaName))
                    query =
                        $"SELECT Count(*) FROM sys.views t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = '{schemaName}' AND t.name = '{viewName}'";
            }

            var count = connection.SqlQuery<int>(query).FirstOrDefault().ToInt32();
            return count;
        }
    }
}