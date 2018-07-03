using System.Data.Common;
using System.Reflection;

namespace DataPowerTools.Extensions
{
    /// <summary>
    /// <see cref="DbConnection"/> extensions.
    /// </summary>
    public static class DbConnectionExtensions
    {
        static readonly PropertyInfo ProviderFactoryPropertyInfo = typeof(DbConnection).GetProperty("DbProviderFactory", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Gets the database provider factory for the given <see cref="DbConnection"/>.
        /// </summary>
        /// <param name="dbConnection">The database connection.</param>
        /// <returns><see cref="DbProviderFactory"/>.</returns>
        public static DbProviderFactory GetDbProviderFactory(this DbConnection dbConnection)
        {
            // Note that in .NET v4.5 we could use this new method instead which would avoid the reflection:
            // DbProviderFactory dbProviderFactory = DbProviderFactories.GetFactory( databaseCommand.DbCommand.Connection );

            return (DbProviderFactory)ProviderFactoryPropertyInfo.GetValue(dbConnection, null);
        }
    }
}