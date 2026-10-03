using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryBorrowingRepository : IBorrowingRepository
{
    private readonly List<Borrowing> _borrowings = new();

    public Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
    {
        _borrowings.Add(borrowing);
        return Task.CompletedTask;
    }

    public Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var count = _borrowings.Count(b => b.StudentId == studentId && b.Status == BorrowingStatus.Active);
        return Task.FromResult(count);
    }

    public Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_borrowings.FirstOrDefault(b => b.Id == id));

    public Task<IReadOnlyList<Borrowing>> GetActiveAsync(CancellationToken cancellationToken = default)
        => Task.FromResult((IReadOnlyList<Borrowing>)_borrowings
            .Where(b => b.Status == BorrowingStatus.Active).ToList());

    public Task<Borrowing?> GetActiveByStudentAndEquipmentAsync(
        int studentId,
        int equipmentId,
        CancellationToken cancellationToken = default)
    {
        var borrowing = _borrowings.FirstOrDefault(b =>
            b.StudentId == studentId &&
            b.EquipmentId == equipmentId &&
            b.Status == BorrowingStatus.Active);

        return Task.FromResult(borrowing);
    }

    // Objects are shared by reference in memory, so there is nothing extra to save.
    public Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}