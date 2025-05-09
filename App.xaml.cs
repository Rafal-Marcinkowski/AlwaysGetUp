﻿using AlwaysGetUp.ViewModels;
using Serilog;
using System.Windows;

namespace AlwaysGetUp;

public partial class App : Application
{
    public App()
    {
        ConfigureLogging();
    }

    private void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("C:\\Users\\rafal\\Desktop\\Pogromcy\\AlwaysGetUp\\log.txt",
            rollingInterval: RollingInterval.Infinite,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Log.Information($"Application started.");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information($"Application exited.");
        MainViewModel.Instance.LogAccumulatedTime();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
