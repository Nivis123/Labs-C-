namespace MasterProgulov.Core;

public interface IStudentStrategy
{
    bool WillAttend(Teacher teacher, IReadOnlyDictionary<Subject, bool> yesterday, IRandomSource random);
}

public sealed class CautiousSkippingStrategy : IStudentStrategy
{
    public bool WillAttend(Teacher teacher, IReadOnlyDictionary<Subject, bool> yesterday, IRandomSource random)
    {
        if (teacher.Rule.Kind == TeacherRuleKind.RandomHalf) return true;
        return teacher.Rule.ShouldAsk(yesterday, random);
    }
}
