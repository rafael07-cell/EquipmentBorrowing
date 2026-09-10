using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EquipmentBorrowing.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IStudentRepository, InMemoryStudentRepository>();
        services.AddSingleton<IEquipmentRepository, InMemoryEquipmentRepository>();
        services.AddSingleton<IBorrowingRepository, InMemoryBorrowingRepository>();

        services.AddTransient<BorrowEquipmentService>();
        services.AddTransient<ReturnEquipmentService>();

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<EquipmentViewModel>();
        services.AddSingleton<BorrowingsViewModel>();

        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}