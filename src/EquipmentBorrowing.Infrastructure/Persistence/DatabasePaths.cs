namespace EquipmentBorrowing.Infrastructure.Persistence;

public static class DatabasePaths
{
    public static string ConnectionString
    {
        get
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EquipmentBorrowing");
            Directory.CreateDirectory(folder);
            return $"Data Source={Path.Combine(folder, "equipmentborrowing.db")}";
        }
    }
}