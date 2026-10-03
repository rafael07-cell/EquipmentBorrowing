using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        IDbContextFactory<EquipmentBorrowingDbContext> factory)
    {
        await using var context = await factory.CreateDbContextAsync();

        // Applies pending migrations only; never recreates the database.
        await context.Database.MigrateAsync();

        // Seed only when the database is empty.
        if (await context.Students.AnyAsync() || await context.Equipment.AnyAsync())
            return;

        var ana = new Student(0, "Ana Reyes");
        var ben = new Student(0, "Ben Cruz");
        var carla = new Student(0, "Carla Santos");
        var diego = new Student(0, "Diego Lim", isAllowedToBorrow: false);

        var python = new Equipment(0, "Python Programming");
        var romeo = new Equipment(0, "Romeo and Juliet");
        var algorithms = new Equipment(0, "Introduction to Algorithms");
        var networks = new Equipment(0, "Computer Networks");
        var dbSystems = new Equipment(0, "Database Systems");

        context.Students.AddRange(ana, ben, carla, diego);
        context.Equipment.AddRange(python, romeo, algorithms, networks, dbSystems);
        await context.SaveChangesAsync();

        // One existing active borrowing so the app starts with an unavailable item.
        romeo.MarkBorrowed();
        context.Borrowings.Add(new Borrowing(
            0, ana.Id, romeo.Id, DateTime.Now.AddDays(-2), DateTime.Now.AddDays(5)));
        await context.SaveChangesAsync();
    }
}