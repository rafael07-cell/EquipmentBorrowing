using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public EquipmentViewModel EquipmentViewModel { get; }
    public BorrowingsViewModel BorrowingsViewModel { get; }

    [ObservableProperty]
    private object? currentView;

    public MainWindowViewModel(EquipmentViewModel equipmentViewModel, BorrowingsViewModel borrowingsViewModel)
    {
        EquipmentViewModel = equipmentViewModel;
        BorrowingsViewModel = borrowingsViewModel;
        CurrentView = EquipmentViewModel;

        _ = EquipmentViewModel.LoadAsync();
        _ = BorrowingsViewModel.LoadAsync();
    }

    [RelayCommand]
    private void ShowEquipment() => CurrentView = EquipmentViewModel;

    [RelayCommand]
    private async Task ShowBorrowingsAsync()
    {
        await BorrowingsViewModel.LoadAsync();
        CurrentView = BorrowingsViewModel;
    }
}