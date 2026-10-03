using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddDbContextFactory<EquipmentBorrowingDbContext>(options =>
            options.UseSqlite(DatabasePaths.ConnectionString));

        services.AddSingleton<IStudentRepository, EfStudentRepository>();
        services.AddSingleton<IEquipmentRepository, EfEquipmentRepository>();
        services.AddSingleton<IBorrowingRepository, EfBorrowingRepository>();
        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider provider)
    {
        var factory = provider.GetRequiredService<IDbContextFactory<EquipmentBorrowingDbContext>>();
        await DbInitializer.InitializeAsync(factory);
    }
}