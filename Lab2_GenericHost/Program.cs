using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MasterProgulov.Core;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IRandomSource, SystemRandomSource>();
builder.Services.AddSingleton<IStudentStrategy, CautiousSkippingStrategy>();
builder.Services.AddSingleton(sp => TeacherFactory.CreateRandom(sp.GetRequiredService<IRandomSource>()));
builder.Services.AddSingleton<SemesterEmulator>();
builder.Services.AddHostedService<SemesterWorker>();
using var host = builder.Build();

Console.WriteLine("Мастер прогулов — Generic Host");
Console.WriteLine("Новый день каждые 5 секунд. Нажмите любую клавишу для остановки (или Ctrl+C).");
var stop = host.Services.GetRequiredService<IHostApplicationLifetime>();
var keyListener = Task.Run(() =>
{
    try { if (!Console.IsInputRedirected) Console.ReadKey(intercept: true); }
    catch (InvalidOperationException) { return; }
    stop.StopApplication();
});
await host.RunAsync();
