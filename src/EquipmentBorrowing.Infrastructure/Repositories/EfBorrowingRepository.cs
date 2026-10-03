using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfBorrowingRepository : IBorrowingRepository
{
    private readonly IDbContextFactory<EquipmentBorrowingDbContext> _factory;

    public EfBorrowingRepository(IDbContextFactory<EquipmentBorrowingDbContext> factory)
    {
        _factory = factory;
    }

    public async Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        await context.Borrowings.AddAsync(borrowing, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountActiveByStudentIdAsync(
        int studentId, CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Borrowings
            .AsNoTracking()
            .CountAsync(b => b.StudentId == studentId
                          && b.Status == BorrowingStatus.Active,
                        cancellationToken);
    }

    // Tracked: callers (ReturnEquipmentService) modify this entity afterwards.
    public async Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Borrowings
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    // Read-only list for display: no tracking.
    public async Task<IReadOnlyList<Borrowing>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Borrowings
            .AsNoTracking()
            .Where(b => b.Status == BorrowingStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<Borrowing?> GetActiveByStudentAndEquipmentAsync(
        int studentId, int equipmentId, CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Borrowings
            .FirstOrDefaultAsync(b => b.StudentId == studentId
                                   && b.EquipmentId == equipmentId
                                   && b.Status == BorrowingStatus.Active,
                                 cancellationToken);
    }

    public async Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        context.Borrowings.Update(borrowing);
        await context.SaveChangesAsync(cancellationToken);
    }
}