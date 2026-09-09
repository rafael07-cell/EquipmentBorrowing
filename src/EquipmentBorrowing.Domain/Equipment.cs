namespace EquipmentBorrowing.Domain;

public class Equipment
{
    public int Id { get; }
    public string Name { get; }
    public bool IsAvailable { get; private set; }

    public Equipment(int id, string name, bool isAvailable = true)
    {
        Id = id;
        Name = name;
        IsAvailable = isAvailable;
    }

    public void MarkBorrowed() => IsAvailable = false;
    public void MarkReturned() => IsAvailable = true;
}