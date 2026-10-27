namespace MasterProgulov.Core;

public sealed class SemesterEmulator
{
    private readonly IReadOnlyList<Teacher> _teachers;
    private readonly IStudentStrategy _strategy;
    private readonly IRandomSource _random;
    private Dictionary<Subject, bool> _yesterday = Enum.GetValues<Subject>().ToDictionary(s => s, _ => false);
    private int _day;

    public int TotalPleasure { get; private set; }
    public bool IsStopped { get; private set; }
    public int DaysProcessed => _day;
    public IReadOnlyDictionary<Subject, bool> Yesterday => _yesterday;

    public SemesterEmulator(IReadOnlyList<Teacher> teachers, IStudentStrategy strategy, IRandomSource random)
    {
        if (teachers.Count != Enum.GetValues<Subject>().Length || teachers.Select(t => t.Subject).Distinct().Count() != teachers.Count)
            throw new ArgumentException("Должен быть ровно один преподаватель на каждый из 6 предметов.", nameof(teachers));
        _teachers = teachers;
        _strategy = strategy;
        _random = random;
    }

    public DayResult ProcessNextDay()
    {
        if (IsStopped) throw new InvalidOperationException("Симуляция уже остановлена.");
        _day++;
        var askedToday = new Dictionary<Subject, bool>();
        var gained = 1; // ежедневный пирожок
        foreach (var teacher in _teachers)
        {
            var asked = teacher.Rule.ShouldAsk(_yesterday, _random);
            askedToday[teacher.Subject] = asked;
            var attends = _strategy.WillAttend(teacher, _yesterday, _random);
            if (!attends && asked)
            {
                TotalPleasure = 0;
                IsStopped = true;
                return new DayResult(_day, 0, 0, true, true, askedToday, 0);
            }
            if (!attends) gained++;
        }
        TotalPleasure += gained;
        _yesterday = Enum.GetValues<Subject>().ToDictionary(s => s, s => askedToday.TryGetValue(s, out var value) && value);
        return new DayResult(_day, TotalPleasure, (double)TotalPleasure / _day, false, false,
            new Dictionary<Subject, bool>(_yesterday), gained);
    }
}

public static class TeacherFactory
{
    public static IReadOnlyList<Teacher> CreateRandom(IRandomSource random)
    {
        var subjects = Enum.GetValues<Subject>();
        var teachers = new List<Teacher>();
        foreach (var subject in subjects)
        {
            var kind = (TeacherRuleKind)random.Next(3);
            var a = subjects[random.Next(subjects.Length)];
            var b = subjects[random.Next(subjects.Length - 1)];
            if ((int)b >= (int)a) b = (Subject)((int)b + 1); // A и B различаются, каждый может совпасть с предметом преподавателя
            teachers.Add(new Teacher(subject, new TeacherRule(kind, a, b)));
        }
        return teachers;
    }
}
