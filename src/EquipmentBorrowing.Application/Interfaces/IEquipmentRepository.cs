using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // NEW — needed so the Equipment screen can list everything, not just one item
    Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default);
}