namespace ConsoleApp1.Configuration;

public static class SimulationSettings
{
    public const int Days = 100;
    public const int Subjects = 6;
    public const int ExplorationDays = 20;
    public const int Simulations = 1000;

    public static readonly string[] SubjectNames =
    {
        "Матанализ",
        "Линейная алгебра",
        "Дискретная математика",
        "Программирование",
        "Физика",
        "Английский язык"
    };
}
