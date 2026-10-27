using MasterProgulov.Core;
using NUnit.Framework;

namespace MasterProgulov.Tests;

[TestFixture]
public class SemesterEmulatorTests
{
    private static IReadOnlyList<Teacher> AllWith(TeacherRule rule) => Enum.GetValues<Subject>().Select(s => new Teacher(s, rule)).ToArray();

    [TestCase(true, false, false)] // присутствует, не спросили
    [TestCase(true, true, false)]  // присутствует, спросили
    [TestCase(false, false, false)] // отсутствует, не спросили
    [TestCase(false, true, true)] // отсутствует, спросили — вылет
    public void FourAttendanceQuestionCombinationsHaveExpectedOutcome(bool attends, bool asked, bool eliminated)
    {
        var teachers = Enum.GetValues<Subject>().Select(s => new Teacher(s, new TeacherRule(TeacherRuleKind.RandomHalf))).ToArray();
        var random = new SequenceRandomSource(Enumerable.Repeat(asked ? 0.1 : 0.9, 20));
        var emulator = new SemesterEmulator(teachers, new FixedStrategy((_,__) => attends), random);
        var result = emulator.ProcessNextDay();
        Assert.That(result.Eliminated, Is.EqualTo(eliminated));
        if (eliminated)
        {
            Assert.That(result.TotalPleasure, Is.Zero);
            Assert.That(emulator.IsStopped, Is.True);
        }
        else
        {
            Assert.That(result.PleasureGainedToday, Is.EqualTo(attends ? 1 : 7));
            Assert.That(result.TotalPleasure, Is.EqualTo(attends ? 1 : 7));
        }
    }

    [Test] public void AbsentAndNotAsked_GainsPieAndSixSkippedClasses()
    {
        var emulator = new SemesterEmulator(AllWith(new TeacherRule(TeacherRuleKind.AskedYesterdayForSubject, Subject.Calculus)), new FixedStrategy((_,__) => false), new SequenceRandomSource());
        var result = emulator.ProcessNextDay();
        Assert.That(result.Eliminated, Is.False);
        Assert.That(result.PleasureGainedToday, Is.EqualTo(7));
    }

    [Test] public void AbsentAndAsked_EliminatesResetsPleasureAndStops()
    {
        var teachers = Enum.GetValues<Subject>().Select(s => new Teacher(s, new TeacherRule(TeacherRuleKind.RandomHalf))).ToArray();
        // First random draw makes the first teacher ask.
        var emulator = new SemesterEmulator(teachers, new FixedStrategy((_,__) => false), new SequenceRandomSource(new[] { 0.1, 0.9, 0.9, 0.9, 0.9, 0.9 }));
        var result = emulator.ProcessNextDay();
        Assert.That(result.Eliminated, Is.True);
        Assert.That(result.TotalPleasure, Is.Zero);
        Assert.That(emulator.TotalPleasure, Is.Zero);
        Assert.That(emulator.IsStopped, Is.True);
        Assert.That(emulator.DaysProcessed, Is.EqualTo(1));
        Assert.Throws<InvalidOperationException>(() => emulator.ProcessNextDay());
    }

    [Test] public void YesterdayIsUpdatedAfterSuccessfulDay()
    {
        var teachers = Enum.GetValues<Subject>().Select(s => new Teacher(s, new TeacherRule(TeacherRuleKind.RandomHalf))).ToArray();
        var emulator = new SemesterEmulator(teachers, new FixedStrategy((_,__) => true), new SequenceRandomSource(new[] { 0.1, 0.9, 0.1, 0.9, 0.1, 0.9 }));
        var result = emulator.ProcessNextDay();
        Assert.That(result.AskedToday[Subject.Calculus], Is.True);
        Assert.That(result.AskedToday[Subject.LinearAlgebra], Is.False);
        Assert.That(emulator.Yesterday[Subject.Calculus], Is.True);
        Assert.That(emulator.Yesterday[Subject.LinearAlgebra], Is.False);
    }

    [Test] public void EjectedDayDoesNotUpdateYesterdayOrProcessMoreDays()
    {
        var teachers = Enum.GetValues<Subject>().Select(s => new Teacher(s, new TeacherRule(TeacherRuleKind.RandomHalf))).ToArray();
        var emulator = new SemesterEmulator(teachers, new FixedStrategy((_,__) => false), new SequenceRandomSource(new[] { 0.1, 0.9, 0.9, 0.9, 0.9, 0.9 }));
        emulator.ProcessNextDay();
        Assert.That(emulator.DaysProcessed, Is.EqualTo(1));
        Assert.That(emulator.Yesterday.Values.All(v => !v), Is.True);
    }
}
