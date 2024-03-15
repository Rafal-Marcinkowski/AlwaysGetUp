using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AlwaysGetUp.ViewModels;

public class MainViewModel : BaseViewModel
{
    private bool _isWorkPeriodTaskActive = false;
    public bool IsWorkPeriodTaskActive
    {
        get => _isWorkPeriodTaskActive;
        set
        {
            _isWorkPeriodTaskActive = value;
            OnPropertyChanged(nameof(IsWorkPeriodTaskActive));
        }
    }
    public event EventHandler OneHourOfSittingEvent;
    public string Time { get; set; }
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

    private CancellationTokenSource cancellation = new();

    private Stopwatch stopwatch = new();
    public ICommand StartCommand =>
        new RelayCommand(async () =>
        {
            cancellation.Cancel();
            cancellation = new CancellationTokenSource();
            if (stopwatch.IsRunning) { stopwatch.Stop(); }
            if (Background == Color.Gray || Background == Color.White || Background == Color.Green)
            {
                await WorkPeriod(cancellation.Token);
            }
            if (Background == Color.Red || Background == Color.Black)
            {
                await BreakPeriod(cancellation.Token);
            }
        });
    private async Task WorkPeriod(CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            stopwatch.Restart();
            stopwatch.Start();
            Background = Color.Black;
            IsWorkPeriodTaskActive = true;
            while (!token.IsCancellationRequested)
            {
                Time = $"{(int)stopwatch.Elapsed.TotalMinutes} minut";
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
        catch (OperationCanceledException)
        {
            IsWorkPeriodTaskActive = false;
        }
        finally
        {
            IsWorkPeriodTaskActive = false;
        }
    }
    private async Task BreakPeriod(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        Background = Color.White;
        stopwatch.Restart();
        stopwatch.Start();
        while (!token.IsCancellationRequested)
        {
            Time = $"{(int)stopwatch.Elapsed.TotalSeconds} sekund";
            OnPropertyChanged(nameof(Time));
            if (stopwatch.Elapsed.TotalMinutes > 3 && Background != Color.Green)
            {
                Background = Color.Green;
            }
            await Task.Delay(1000);
        }
    }
}