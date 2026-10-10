using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MasterProgulov.Core;

public sealed class SemesterWorker : BackgroundService
{
    private readonly SemesterEmulator _emulator;
    private readonly ILogger<SemesterWorker> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    public SemesterWorker(SemesterEmulator emulator, ILogger<SemesterWorker> logger, IHostApplicationLifetime lifetime) { _emulator = emulator; _logger = logger; _lifetime = lifetime; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && !_emulator.IsStopped)
        {
            try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); }
            catch (OperationCanceledException) { break; }
            if (stoppingToken.IsCancellationRequested) break;
            var result = _emulator.ProcessNextDay();
            if (result.Eliminated)
            {
                _logger.LogWarning("День {Day}: студент вылетел! Удовольствие обнулено. Симуляция остановлена.", result.DayNumber);
                _lifetime.StopApplication();
                break;
            }
            _logger.LogInformation("День {Day}: +{Gained}; всего удовольствие = {Total}; среднее в день = {Average:F2}",
                result.DayNumber, result.PleasureGainedToday, result.TotalPleasure, result.AveragePleasurePerDay);
        }
        if (!_emulator.IsStopped && !stoppingToken.IsCancellationRequested)
            _logger.LogInformation("Симуляция завершена.");
    }
}
