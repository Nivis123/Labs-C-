namespace MasterProgulov.Core;

public enum TeacherRuleKind { RandomHalf, AskedYesterdayForSubject, ExclusiveOrYesterdaySubjects }

public sealed class TeacherRule
{
    public TeacherRuleKind Kind { get; }
    public Subject SubjectA { get; }
    public Subject SubjectB { get; }

    public TeacherRule(TeacherRuleKind kind, Subject subjectA = Subject.Calculus, Subject subjectB = Subject.LinearAlgebra)
    {
        if (kind == TeacherRuleKind.ExclusiveOrYesterdaySubjects && subjectA == subjectB)
            throw new ArgumentException("Предметы A и B должны различаться.", nameof(subjectB));
        Kind = kind;
        SubjectA = subjectA;
        SubjectB = subjectB;
    }

    public bool ShouldAsk(IReadOnlyDictionary<Subject, bool> yesterday, IRandomSource random) => Kind switch
    {
        TeacherRuleKind.RandomHalf => random.NextDouble() < 0.5,
        TeacherRuleKind.AskedYesterdayForSubject => WasAsked(yesterday, SubjectA),
        TeacherRuleKind.ExclusiveOrYesterdaySubjects => WasAsked(yesterday, SubjectA) != WasAsked(yesterday, SubjectB),
        _ => throw new InvalidOperationException($"Неизвестное правило: {Kind}")
    };

    private static bool WasAsked(IReadOnlyDictionary<Subject, bool> history, Subject subject) =>
        history.TryGetValue(subject, out var asked) && asked;
}
