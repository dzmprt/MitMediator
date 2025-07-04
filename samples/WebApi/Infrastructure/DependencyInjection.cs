using Application.Abstractions.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

/// <summary>
/// Dependency injection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add application services.
    /// </summary>
    /// <param name="services"><see cref="IServiceProvider"/>.</param>
    /// <returns><see cref="IServiceProvider"/></returns>
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return services.AddDbContext<DbContext, ApplicationDbContext>(options =>
            {
                options.UseSqlite(connection);
            })
            .AddTransient(typeof(IBaseProvider<>), typeof(BaseProvider<>))
            .AddTransient(typeof(IBaseRepository<>), typeof(BaseRepository<>));

    }
}