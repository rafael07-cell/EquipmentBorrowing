using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IBorrowingRepository
{
    Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default);

    Task<int> CountActiveByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);

    // NEW — needed so the Return use case can look up a specific borrowing by its id
    Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // NEW — needed so the Active Borrowings screen has something to display
    Task<IReadOnlyList<Borrowing>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<Borrowing?> GetActiveByStudentAndEquipmentAsync(
        int studentId,
        int equipmentId,
        CancellationToken cancellationToken = default);
}