using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // NEW — needed for the student dropdown in the Borrow screen
    Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default);
}