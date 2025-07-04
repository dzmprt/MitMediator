using Application.Abstractions.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
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