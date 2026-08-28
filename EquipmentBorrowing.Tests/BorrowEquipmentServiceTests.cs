using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Infrastructure.Repositories;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class BorrowEquipmentServiceTests
{
    private static BorrowEquipmentService CreateService()
    {
        var studentRepo = new InMemoryStudentRepository();
        var equipmentRepo = new InMemoryEquipmentRepository();
        var borrowingRepo = new InMemoryBorrowingRepository();
        return new BorrowEquipmentService(studentRepo, equipmentRepo, borrowingRepo);
    }

    [Fact]
    public async Task ExecuteAsync_ValidStudentAndAvailableEquipment_ReturnsSuccess()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.ExecuteAsync(studentId: 1, equipmentId: 1);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Borrowing);
    }

    [Fact]
    public async Task ExecuteAsync_StudentNotAllowedToBorrow_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.ExecuteAsync(studentId: 2, equipmentId: 1);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal("Student is not allowed to borrow.", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_EquipmentNotAvailable_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.ExecuteAsync(studentId: 1, equipmentId: 2);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal("Equipment is not available.", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_StudentDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.ExecuteAsync(studentId: 999, equipmentId: 1);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal("Student not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_EquipmentDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.ExecuteAsync(studentId: 1, equipmentId: 999);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal("Equipment not found.", result.ErrorMessage);
    }
}