namespace EquipmentBorrowing.Application.Interfaces;

// Read-only shape for the screen: only the columns we need, no EF Core types.
public record ActiveBorrowingDetails(
    int BorrowingId,
    string StudentName,
    string EquipmentName,
    DateTime DateBorrowed,
    DateTime ExpectedReturnDate);

public interface IBorrowingQueries
{
    Task<IReadOnlyList<ActiveBorrowingDetails>> GetActiveWithDetailsAsync(
        CancellationToken cancellationToken = default);
}