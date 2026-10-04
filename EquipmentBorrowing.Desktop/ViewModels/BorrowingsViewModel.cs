using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowingsViewModel : ObservableObject
{
    private readonly IBorrowingQueries _borrowingQueries;
    private readonly ReturnEquipmentService _returnEquipmentService;

    public ObservableCollection<ActiveBorrowingDetails> ActiveBorrowings { get; } = new();

    [ObservableProperty]
    private ActiveBorrowingDetails? selectedBorrowing;

    [ObservableProperty]
    private string? statusMessage;

    public BorrowingsViewModel(
        IBorrowingQueries borrowingQueries,
        ReturnEquipmentService returnEquipmentService)
    {
        _borrowingQueries = borrowingQueries;
        _returnEquipmentService = returnEquipmentService;
    }

    public async Task LoadAsync()
    {
        ActiveBorrowings.Clear();
        foreach (var b in await _borrowingQueries.GetActiveWithDetailsAsync())
            ActiveBorrowings.Add(b);
    }

    [RelayCommand]
    private async Task ReturnAsync()
    {
        if (SelectedBorrowing is null)
        {
            StatusMessage = "Please select a borrowing to return.";
            return;
        }

        var result = await _returnEquipmentService.ExecuteAsync(SelectedBorrowing.BorrowingId);

        StatusMessage = result.IsSuccessful
            ? $"Borrowing #{result.Borrowing!.Id} returned successfully."
            : $"Failed: {result.ErrorMessage}";

        await LoadAsync();
    }
}