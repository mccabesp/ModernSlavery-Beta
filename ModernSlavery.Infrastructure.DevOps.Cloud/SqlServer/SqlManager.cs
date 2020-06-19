using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Sql.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Management.Sql.Fluent.Models;

namespace ModernSlavery.Infrastructure.Azure.DevOps
{
    public class SqlManager
    {
        IAzure _azure;
        private IList<ISqlDatabase> _SqlDatabases;
        public IList<ISqlDatabase> SqlDatabases => _SqlDatabases ??= _azure.SqlServers.List().SelectMany(server=> server.Databases.List()).ToList();

        private IList<ISqlServer> _SqlServers;
        public IList<ISqlServer> SqlServers => _SqlServers ??= _azure.SqlServers.List().ToList();

        public SqlManager(IAzure azure)
        {
            _azure = azure ?? throw new ArgumentNullException(nameof(azure));
        }

        public ISqlDatabase GetSqlDatabase(string sqlDatabaseName)
        {
            if (string.IsNullOrWhiteSpace(sqlDatabaseName)) throw new ArgumentNullException(nameof(sqlDatabaseName));

            return SqlDatabases.FirstOrDefault(w => w.Name.EqualsI(sqlDatabaseName));
        }
        public ISqlServer GetSqlServer(string sqlServerName)
        {
            return SqlServers.FirstOrDefault(w => w.Name.EqualsI(sqlServerName));
        }
        public ISqlServer GetSqlServer(ISqlDatabase sqlDatabase)
        {
            if (sqlDatabase == null) throw new ArgumentNullException(nameof(sqlDatabase));

            var sqlServer = SqlServers.FirstOrDefault(server => server.Name.EqualsI(sqlDatabase.SqlServerName));
            return sqlServer;
        }

        #region Vertical scaling
        public void SetDatabaseEdition(string databaseName, DatabaseEdition newDatabaseEdition)
        {
            var sqlDatabase=GetSqlDatabase(databaseName);
            if (sqlDatabase == null) throw new ArgumentNullException(nameof(sqlDatabase), $"Cannot find Sql database '{databaseName}'");

            SetDatabaseEdition(sqlDatabase, newDatabaseEdition);
        }

        public void SetDatabaseEdition(ISqlDatabase sqlDatabase, DatabaseEdition newDatabaseEdition)
        {
            if (sqlDatabase == null) throw new ArgumentNullException(nameof(sqlDatabase));
            if (newDatabaseEdition == null) throw new ArgumentNullException(nameof(newDatabaseEdition));

            if (sqlDatabase.Edition.Value!= newDatabaseEdition.Value)sqlDatabase.Update().WithEdition(newDatabaseEdition).Apply();
        }
        #endregion
    }
}
