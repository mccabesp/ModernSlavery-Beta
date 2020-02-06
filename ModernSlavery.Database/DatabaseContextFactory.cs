using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ModernSlavery.Database
{
    /// <summary>
    ///     This class is required for adding migrations and updating the database at design time
    /// </summary>
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {

        public DatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

            //Setup the SQL server with automatic retry on failure
            optionsBuilder.UseSqlServer(DatabaseContext.ConnectionString, options => options.EnableRetryOnFailure());
            return new DatabaseContext(optionsBuilder.Options);
        }

    }
}
