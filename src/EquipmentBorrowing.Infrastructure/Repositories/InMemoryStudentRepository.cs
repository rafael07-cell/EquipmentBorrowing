using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryStudentRepository : IStudentRepository
{
    private readonly List<Student> _students = new()
{
    new Student(1, "Juan Dela Cruz", isAllowedToBorrow: true),
    new Student(2, "Maria Santos", isAllowedToBorrow: false)
};

    public void Add(Student student) => _students.Add(student);

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var student = _students.FirstOrDefault(s => s.Id == id);
        return Task.FromResult(student);
    }
}