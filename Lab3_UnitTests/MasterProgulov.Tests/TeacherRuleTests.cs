using MasterProgulov.Core;
using NUnit.Framework;

namespace MasterProgulov.Tests;

[TestFixture]
public class TeacherRuleTests
{
    private static IReadOnlyDictionary<Subject, bool> History(bool a, bool b) => new Dictionary<Subject, bool>
    { [Subject.Calculus] = a, [Subject.LinearAlgebra] = b };

    [TestCase(0.49, true)] [TestCase(0.5, false)] [TestCase(0.99, false)]
    public void RandomRule_AsksWithProbabilityThreshold(double value, bool expected) =>
        Assert.That(new TeacherRule(TeacherRuleKind.RandomHalf).ShouldAsk(History(false, false), new SequenceRandomSource(new[] { value })), Is.EqualTo(expected));

    [TestCase(false, false, false)] [TestCase(false, true, false)] [TestCase(true, false, true)] [TestCase(true, true, true)]
    public void Rule2_FollowsSubjectA(bool a, bool b, bool expected) =>
        Assert.That(new TeacherRule(TeacherRuleKind.AskedYesterdayForSubject, Subject.Calculus).ShouldAsk(History(a,b), new SequenceRandomSource()), Is.EqualTo(expected));

    [TestCase(false, false, false)] [TestCase(false, true, true)] [TestCase(true, false, true)] [TestCase(true, true, false)]
    public void Rule3_IsExclusiveOr(bool a, bool b, bool expected) =>
        Assert.That(new TeacherRule(TeacherRuleKind.ExclusiveOrYesterdaySubjects, Subject.Calculus, Subject.LinearAlgebra).ShouldAsk(History(a,b), new SequenceRandomSource()), Is.EqualTo(expected));

    [Test] public void Rule3_RejectsSameSubjects() => Assert.Throws<ArgumentException>(() => new TeacherRule(TeacherRuleKind.ExclusiveOrYesterdaySubjects, Subject.Physics, Subject.Physics));
    [Test] public void FirstDay_MissingHistoryMeansNobodyWasAsked() => Assert.That(new TeacherRule(TeacherRuleKind.AskedYesterdayForSubject, Subject.English).ShouldAsk(new Dictionary<Subject,bool>(), new SequenceRandomSource()), Is.False);
}
