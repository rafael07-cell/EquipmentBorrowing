using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class ReturnEquipmentService
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public ReturnEquipmentService(
        IBorrowingRepository borrowingRepository,
        IEquipmentRepository equipmentRepository)
    {
        _borrowingRepository = borrowingRepository;
        _equipmentRepository = equipmentRepository;
    }

    public async Task<ReturnResult> ExecuteAsync(int borrowingId, CancellationToken cancellationToken = default)
    {
        var borrowing = await _borrowingRepository.GetByIdAsync(borrowingId, cancellationToken);
        if (borrowing is null)
            return ReturnResult.Fail("Borrowing record not found.");

        if (borrowing.Status == BorrowingStatus.Returned)
            return ReturnResult.Fail("This item has already been returned.");

        var equipment = await _equipmentRepository.GetByIdAsync(borrowing.EquipmentId, cancellationToken);
        if (equipment is null)
            return ReturnResult.Fail("Associated equipment record not found.");

        borrowing.MarkReturned();
        equipment.MarkReturned();

        return ReturnResult.Success(borrowing);
    }
}

public class ReturnResult
{
    public bool IsSuccessful { get; }
    public string? ErrorMessage { get; }
    public Borrowing? Borrowing { get; }

    private ReturnResult(bool isSuccessful, string? errorMessage, Borrowing? borrowing)
    {
        IsSuccessful = isSuccessful;
        ErrorMessage = errorMessage;
        Borrowing = borrowing;
    }

    public static ReturnResult Success(Borrowing borrowing) => new(true, null, borrowing);
    public static ReturnResult Fail(string message) => new(false, message, null);
}