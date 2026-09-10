using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryEquipmentRepository : IEquipmentRepository
{
    public Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default)
    => Task.FromResult((IReadOnlyList<Equipment>)_equipment.ToList());
    private readonly List<Equipment> _equipment = new()
{
    new Equipment(1, "Python Programming", isAvailable: true),
    new Equipment(2, "Romeo and Juliet", isAvailable: false)
};

    public void Add(Equipment equipment) => _equipment.Add(equipment);

    public Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = _equipment.FirstOrDefault(e => e.Id == id);
        return Task.FromResult(item);
    }
}