using GalaSoft.MvvmLight.Command;
using Serilog;
using System.Diagnostics;
using System.Windows.Input;

namespace AlwaysGetUp.ViewModels;

public class MainViewModel : BaseViewModel
{

    private static MainViewModel _instance;
    public static MainViewModel Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MainViewModel();
            }
            return _instance;
        }
    }

    public event EventHandler? OneHourOfSittingEvent;

    public bool IsWorkPeriodTaskActive = false;

    public string? Time { get; set; }
    private Color _background = Color.Gray;
    public Color Background
    {
        get => _background;
        set
        {
            _background = value;
            OnPropertyChanged(nameof(Background));
        }
    }
    public enum Color
    {
        Gray,
        Green,
        Red,
        Black,
        White
    }
    private bool IsWalking = false;
    private CancellationTokenSource cancellation = new();

    private Stopwatch stopwatch = new();

    private TimeSpan totalSittingTime = TimeSpan.Zero;
    private TimeSpan totalWalkingTime = TimeSpan.Zero;

    public ICommand StartCommand =>
        new RelayCommand(async () =>
        {
            cancellation.Cancel();
            cancellation = new CancellationTokenSource();
            if (stopwatch.IsRunning) { stopwatch.Stop(); }
            if (Background == Color.Gray || Background == Color.White || Background == Color.Green)
            {
                if (stopwatch.Elapsed.TotalMinutes > 0)
                {
                    Log.Information($"Walking time: {Math.Round(stopwatch.Elapsed.TotalMinutes, 1):F1} minutes.");
                    totalWalkingTime += TimeSpan.FromMinutes(Math.Round(stopwatch.Elapsed.TotalMinutes, 1));
                }
                Log.Information($"Sitting period started.");
                await WorkPeriod(cancellation.Token);
            }
            else
            {
                if (stopwatch.Elapsed.TotalMinutes > 0)
                {
                    Log.Information($"Sitting time: {Math.Round(stopwatch.Elapsed.TotalMinutes, 1):F1} minutes.");
                    totalSittingTime += TimeSpan.FromMinutes(Math.Round(stopwatch.Elapsed.TotalMinutes, 1));
                }
                Log.Information($"Break period started.");
                await BreakPeriod(cancellation.Token);
            }
        });

    private async Task WorkPeriod(CancellationToken token)
    {
        try
        {
            IsWalking = false;
            token.ThrowIfCancellationRequested();
            stopwatch.Restart();
            Background = Color.Black;
            IsWorkPeriodTaskActive = true;
            while (!token.IsCancellationRequested)
            {
                Time = $"{Math.Round(stopwatch.Elapsed.TotalMinutes, 1)} minutes";
                OnPropertyChanged(nameof(Time));
                if (stopwatch.Elapsed.TotalMinutes > 60 && Background != Color.Red)
                {
                    Background = Color.Red;
                    IsWorkPeriodTaskActive = false;
                    OneHourOfSittingEvent?.Invoke(this, EventArgs.Empty);
                }
                await Task.Delay(1000);
            }
        }
        finally
        {
            IsWorkPeriodTaskActive = false;
        }
    }

    private async Task BreakPeriod(CancellationToken token)
    {
        IsWalking = true;
        token.ThrowIfCancellationRequested();
        Background = Color.White;
        stopwatch.Restart();
        while (!token.IsCancellationRequested)
        {
            Time = $"{Math.Round(stopwatch.Elapsed.TotalMinutes, 1)} minutes";
            OnPropertyChanged(nameof(Time));
            if (stopwatch.Elapsed.TotalMinutes > 5 && Background != Color.Green)
            {
                Background = Color.Green;
            }
            await Task.Delay(1000);
        }
    }

    public void LogAccumulatedTime()
    {
        if (stopwatch.IsRunning) { stopwatch.Stop(); }
        if (IsWalking)
        {
            totalWalkingTime += TimeSpan.FromMinutes(Math.Round(stopwatch.Elapsed.TotalMinutes, 1));
        }
        else
        {
            totalSittingTime += TimeSpan.FromMinutes(Math.Round(stopwatch.Elapsed.TotalMinutes, 1));
        }
        Log.Information($"Total walking time: {totalWalkingTime.TotalMinutes:F1} minutes.");
        Log.Information($"Total sitting time: {totalSittingTime.TotalMinutes:F1} minutes. {Environment.NewLine}");
    }
}