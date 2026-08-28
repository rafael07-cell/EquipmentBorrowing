using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly List<Equipment> _equipment = new()
{
    new Equipment(1, "Digital Multimeter", isAvailable: true),
    new Equipment(2, "Oscilloscope", isAvailable: false)
};

    public void Add(Equipment equipment) => _equipment.Add(equipment);

    public Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = _equipment.FirstOrDefault(e => e.Id == id);
        return Task.FromResult(item);
    }
}