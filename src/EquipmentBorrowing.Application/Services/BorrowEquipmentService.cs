using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class BorrowEquipmentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;
    private const int MaxActiveBorrowings = 3;

    public BorrowEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<BorrowResult> ExecuteAsync(
        int studentId, int equipmentId, CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByIdAsync(studentId, cancellationToken);
        if (student is null)
            return BorrowResult.Fail("Student not found.");

        if (!student.IsAllowedToBorrow)
            return BorrowResult.Fail("Student is not allowed to borrow.");

        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
        if (equipment is null)
            return BorrowResult.Fail("Equipment not found.");

        if (!equipment.IsAvailable)
            return BorrowResult.Fail("Equipment is not available.");

        var activeCount = await _borrowingRepository.CountActiveByStudentIdAsync(studentId, cancellationToken);
        if (activeCount >= MaxActiveBorrowings)
            return BorrowResult.Fail("Student has reached the maximum number of active borrowings.");

        var borrowing = new Borrowing(
            id: new Random().Next(1000, 9999),
            studentId: studentId,
            equipmentId: equipmentId,
            dateBorrowed: DateTime.Now,
            expectedReturnDate: DateTime.Now.AddDays(7));

        equipment.MarkBorrowed();
        await _borrowingRepository.AddAsync(borrowing, cancellationToken);

        return BorrowResult.Success(borrowing);
    }
}

public class BorrowResult
{
    public bool IsSuccessful { get; }
    public string? ErrorMessage { get; }
    public Borrowing? Borrowing { get; }

    private BorrowResult(bool isSuccessful, string? errorMessage, Borrowing? borrowing)
    {
        IsSuccessful = isSuccessful;
        ErrorMessage = errorMessage;
        Borrowing = borrowing;
    }

    public static BorrowResult Success(Borrowing borrowing) => new(true, null, borrowing);
    public static BorrowResult Fail(string message) => new(false, message, null);
}