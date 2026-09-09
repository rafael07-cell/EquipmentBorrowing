using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

var studentRepo = new InMemoryStudentRepository();
var equipmentRepo = new InMemoryEquipmentRepository();
var borrowingRepo = new InMemoryBorrowingRepository();

// Seed sample data
studentRepo.Add(new Student(1, "Juan Dela Cruz", isAllowedToBorrow: true));
studentRepo.Add(new Student(2, "Maria Santos", isAllowedToBorrow: false)); // used for failure case

equipmentRepo.Add(new Equipment(1, "Projector", isAvailable: true));
equipmentRepo.Add(new Equipment(2, "Laptop", isAvailable: false)); // used for failure case

var service = new BorrowEquipmentService(studentRepo, equipmentRepo, borrowingRepo);

Console.WriteLine("--- Successful case: Student 1 borrows Equipment 1 ---");
var result1 = await service.ExecuteAsync(1, 1);
Console.WriteLine(result1.IsSuccessful
    ? $"Success: Borrowing #{result1.Borrowing!.Id} created."
    : $"Failed: {result1.ErrorMessage}");

Console.WriteLine("\n--- Failure case: Student 2 not allowed to borrow ---");
var result2 = await service.ExecuteAsync(2, 1);
Console.WriteLine(result2.IsSuccessful
    ? $"Success: Borrowing #{result2.Borrowing!.Id} created."
    : $"Failed: {result2.ErrorMessage}");

Console.WriteLine("\n--- Failure case: Equipment 2 unavailable ---");
var result3 = await service.ExecuteAsync(1, 2);
Console.WriteLine(result3.IsSuccessful
    ? $"Success: Borrowing #{result3.Borrowing!.Id} created."
    : $"Failed: {result3.ErrorMessage}");