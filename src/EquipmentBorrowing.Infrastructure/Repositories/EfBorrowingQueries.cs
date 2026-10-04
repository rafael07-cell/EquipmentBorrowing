using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfBorrowingQueries : IBorrowingQueries
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _factory;

    public EfBorrowingQueries(IDbContextFactory<EquipmentBorrowingDbContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<ActiveBorrowingDetails>> GetActiveWithDetailsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);

        // Join the 3 tables by hand because Borrowing has no navigation properties.
        // AsNoTracking: display only, nothing gets modified.
        return await (
            from b in context.Borrowings.AsNoTracking()
            join s in context.Students on b.StudentId equals s.Id
            join e in context.Equipment on b.EquipmentId equals e.Id
            where b.Status == BorrowingStatus.Active
            orderby b.DateBorrowed
            select new ActiveBorrowingDetails(
                b.Id, s.Name, e.Name, b.DateBorrowed, b.ExpectedReturnDate)
        ).ToListAsync(cancellationToken);
    }
}