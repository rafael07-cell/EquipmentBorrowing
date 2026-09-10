using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryStudentRepository : IStudentRepository
{
    public Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    => Task.FromResult((IReadOnlyList<Student>)_students.ToList());

    private readonly List<Student> _students = new()
{
    new Student(1, "Bern Nabuntoran", isAllowedToBorrow: true),
    new Student(2, "Rafael Tulfo", isAllowedToBorrow: false)
};

    public void Add(Student student) => _students.Add(student);

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var student = _students.FirstOrDefault(s => s.Id == id);
        return Task.FromResult(student);
    }
}