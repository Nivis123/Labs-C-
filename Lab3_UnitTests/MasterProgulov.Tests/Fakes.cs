using MasterProgulov.Core;

namespace MasterProgulov.Tests;

public sealed class SequenceRandomSource : IRandomSource
{
    private readonly Queue<double> _doubles;
    private readonly Queue<int> _ints;
    public SequenceRandomSource(IEnumerable<double>? doubles = null, IEnumerable<int>? ints = null)
    { _doubles = new Queue<double>(doubles ?? new[] { 0.1, 0.9, 0.4, 0.8, 0.2, 0.7, 0.3, 0.6 }); _ints = new Queue<int>(ints ?? Enumerable.Repeat(0, 100)); }
    public double NextDouble() => _doubles.Count > 0 ? _doubles.Dequeue() : 0.9;
    public int Next(int exclusiveMax) => (_ints.Count > 0 ? _ints.Dequeue() : 0) % exclusiveMax;
}

public sealed class FixedStrategy : IStudentStrategy
{
    private readonly Func<Teacher, IReadOnlyDictionary<Subject, bool>, bool> _attend;
    public FixedStrategy(Func<Teacher, IReadOnlyDictionary<Subject, bool>, bool> attend) => _attend = attend;
    public bool WillAttend(Teacher teacher, IReadOnlyDictionary<Subject, bool> yesterday, IRandomSource random) => _attend(teacher, yesterday);
}

public static class TestTeachers
{
    public static IReadOnlyList<Teacher> All(TeacherRuleKind kind = TeacherRuleKind.AskedYesterdayForSubject, Subject a = Subject.Calculus, Subject b = Subject.LinearAlgebra) =>
        Enum.GetValues<Subject>().Select(s => new Teacher(s, new TeacherRule(kind, a, b))).ToArray();
}
