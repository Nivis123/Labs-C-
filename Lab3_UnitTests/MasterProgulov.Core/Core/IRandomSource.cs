namespace MasterProgulov.Core;

public interface IRandomSource
{
    double NextDouble();
    int Next(int exclusiveMax);
}

public sealed class SystemRandomSource : IRandomSource
{
    private readonly Random _random = new();
    public double NextDouble() => _random.NextDouble();
    public int Next(int exclusiveMax) => _random.Next(exclusiveMax);
}
