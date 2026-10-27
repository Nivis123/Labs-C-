using MasterProgulov.Core;
using NUnit.Framework;

namespace MasterProgulov.Tests;

[TestFixture]
public class StudentStrategyTests
{
    private readonly CautiousSkippingStrategy _strategy = new();
    private static IReadOnlyDictionary<Subject,bool> H(bool asked) => Enum.GetValues<Subject>().ToDictionary(s => s, _ => asked);

    [Test] public void EmptyHistory_StrategySkipsWhenRulePredictsNoQuestion() => Assert.That(_strategy.WillAttend(new Teacher(Subject.Programming, new TeacherRule(TeacherRuleKind.AskedYesterdayForSubject, Subject.Calculus)), new Dictionary<Subject,bool>(), new SequenceRandomSource()), Is.False);
    [Test] public void StableNotAsked_StrategySkipsDeterministicClass() => Assert.That(_strategy.WillAttend(new Teacher(Subject.Programming, new TeacherRule(TeacherRuleKind.AskedYesterdayForSubject, Subject.Calculus)), H(false), new SequenceRandomSource()), Is.False);
    [Test] public void StableAsked_StrategyAttends() => Assert.That(_strategy.WillAttend(new Teacher(Subject.Programming, new TeacherRule(TeacherRuleKind.AskedYesterdayForSubject, Subject.Calculus)), H(true), new SequenceRandomSource()), Is.True);
    [Test] public void MixedHistory_XorPredictsCorrectly() => Assert.That(_strategy.WillAttend(new Teacher(Subject.Programming, new TeacherRule(TeacherRuleKind.ExclusiveOrYesterdaySubjects, Subject.Calculus, Subject.Physics)), new Dictionary<Subject,bool>{{Subject.Calculus,true},{Subject.Physics,false}}, new SequenceRandomSource()), Is.True);
    [Test] public void RandomRule_StrategyAlwaysAttendsToAvoidRisk() => Assert.That(_strategy.WillAttend(new Teacher(Subject.Programming, new TeacherRule(TeacherRuleKind.RandomHalf)), H(false), new SequenceRandomSource()), Is.True);

    [Test] public void DecisionsAreIndependentForEachSubject()
    {
        var teachers = Enum.GetValues<Subject>().Select(s => new Teacher(s, new TeacherRule(TeacherRuleKind.AskedYesterdayForSubject, s))).ToArray();
        var history = Enum.GetValues<Subject>().ToDictionary(s => s, s => s == Subject.Physics || s == Subject.English);
        Assert.That(teachers.Select(t => _strategy.WillAttend(t, history, new SequenceRandomSource())).ToArray(),
            Is.EqualTo(new[] { false, false, false, false, true, true }));
    }
}
