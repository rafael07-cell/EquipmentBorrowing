using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EquipmentBorrowingDbContext>
{
    public EquipmentBorrowingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<EquipmentBorrowingDbContext>()
            .UseSqlite(DatabasePaths.ConnectionString)
            .Options;
        return new EquipmentBorrowingDbContext(options);
    }
}