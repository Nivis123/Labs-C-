namespace ConsoleApp1.Models;

public sealed class Candidate
{
    public RuleType Rule { get; set; }
    public int A { get; set; } = -1;
    public int B { get; set; } = -1;

    public bool Predict(bool[][] asked, int day)
    {
        if (day == 0)
            return false;

        return Rule switch
        {
            RuleType.IfA => asked[day - 1][A],
            RuleType.Xor => asked[day - 1][A] ^ asked[day - 1][B],
            _ => false
        };
    }
}
