using ConsoleApp1.Configuration;
using ConsoleApp1.Models;

namespace ConsoleApp1.Services;

public sealed class Simulator
{
    public void Run()
    {
        var random = new Random();

        int totalPleasure = 0;
        int failed = 0;

        for (int i = 0; i < SimulationSettings.Simulations; i++)
        {
            Teacher[] teachers = GenerateTeachers(random);
            SemesterResult result = SimulateSemester(teachers, random);

            totalPleasure += result.Pleasure;
            if (result.Failed)
                failed++;
        }

        double average = (double)totalPleasure / SimulationSettings.Simulations;

        Console.WriteLine($"Симуляций: {SimulationSettings.Simulations}");
        Console.WriteLine($"Среднее удовольствие за семестр: {average:F2}");
        Console.WriteLine($"Среднее удовольствие в день: {average / SimulationSettings.Days:F2}");
        Console.WriteLine($"Вылетов: {failed} ({100.0 * failed / SimulationSettings.Simulations:F2}%)");

        Console.WriteLine();
        Console.WriteLine("=== Пример одного семестра ===");

        Teacher[] demoTeachers = GenerateTeachers(random);
        PrintTeachers(demoTeachers);

        SemesterResult demo = SimulateSemester(demoTeachers, random, verbose: true);

        Console.WriteLine(
            $"Итог: удовольствие={demo.Pleasure}, " +
            $"пропущено={demo.Skipped}, " +
            $"пирожков={demo.Pies}, " +
            $"вылет={(demo.Failed ? "да" : "нет")}");
    }

    private static void PrintTeachers(Teacher[] teachers)
    {
        for (int i = 0; i < SimulationSettings.Subjects; i++)
        {
            Teacher teacher = teachers[i];

            string ruleArguments = teacher.Rule == RuleType.Random
                ? string.Empty
                : $" A={SimulationSettings.SubjectNames[teacher.A]}, B={SimulationSettings.SubjectNames[teacher.B]}";

            Console.WriteLine($"{SimulationSettings.SubjectNames[i],-22}: {teacher.Rule}{ruleArguments}");
        }
    }

    private static Teacher[] GenerateTeachers(Random random)
    {
        var teachers = new Teacher[SimulationSettings.Subjects];

        for (int i = 0; i < SimulationSettings.Subjects; i++)
        {
            RuleType rule = (RuleType)random.Next(0, 3);

            int a = -1;
            int b = -1;

            if (rule != RuleType.Random)
            {
                a = random.Next(SimulationSettings.Subjects);

                do
                {
                    b = random.Next(SimulationSettings.Subjects);
                }
                while (b == a);
            }

            teachers[i] = new Teacher
            {
                Rule = rule,
                A = a,
                B = b
            };
        }

        return teachers;
    }

    private static bool TeacherAsks(Teacher teacher, bool[][] asked, int day, Random random)
    {
        return teacher.Rule switch
        {
            RuleType.Random => random.NextDouble() < 0.5,
            RuleType.IfA => day == 0 ? false : asked[day - 1][teacher.A],
            RuleType.Xor => day == 0 ? false : asked[day - 1][teacher.A] ^ asked[day - 1][teacher.B],
            _ => false
        };
    }

    private static List<Candidate> InferCandidates(bool[][] asked, int subject, int days)
    {
        var result = new List<Candidate>();

        // Правило 2: спросить, если вчера спросили по предмету A.
        for (int a = 0; a < SimulationSettings.Subjects; a++)
        {
            bool ok = true;

            for (int day = 0; day < days; day++)
            {
                bool predicted = day == 0 ? false : asked[day - 1][a];

                if (predicted != asked[day][subject])
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
            {
                result.Add(new Candidate
                {
                    Rule = RuleType.IfA,
                    A = a
                });
            }
        }

        // Правило 3: XOR по предметам A и B.
        for (int a = 0; a < SimulationSettings.Subjects; a++)
        {
            for (int b = a + 1; b < SimulationSettings.Subjects; b++)
            {
                bool ok = true;

                for (int day = 0; day < days; day++)
                {
                    bool predicted = day == 0
                        ? false
                        : asked[day - 1][a] ^ asked[day - 1][b];

                    if (predicted != asked[day][subject])
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    result.Add(new Candidate
                    {
                        Rule = RuleType.Xor,
                        A = a,
                        B = b
                    });
                }
            }
        }

        return result;
    }

    private static SemesterResult SimulateSemester(
        Teacher[] trueTeachers,
        Random random,
        bool verbose = false)
    {
        var asked = new bool[SimulationSettings.Days][];

        for (int day = 0; day < SimulationSettings.Days; day++)
            asked[day] = new bool[SimulationSettings.Subjects];

        int totalPleasure = 0;
        int skipped = 0;
        int pies = 0;
        bool failed = false;

        // 1. Разведка: ходим на все пары, чтобы узнать поведение преподавателей.
        for (int day = 0; day < SimulationSettings.ExplorationDays; day++)
        {
            for (int subject = 0; subject < SimulationSettings.Subjects; subject++)
                asked[day][subject] = TeacherAsks(trueTeachers[subject], asked, day, random);

            totalPleasure++;
            pies++;
        }

        // 2. По разведочным данным строим возможные модели.
        var candidates = new List<Candidate>[SimulationSettings.Subjects];
        var isRandom = new bool[SimulationSettings.Subjects];

        for (int subject = 0; subject < SimulationSettings.Subjects; subject++)
        {
            List<Candidate> list = InferCandidates(asked, subject, SimulationSettings.ExplorationDays);

            if (list.Count == 0)
            {
                // Ни одно детерминированное правило не подошло.
                // Считаем, что преподаватель спрашивает случайно.
                isRandom[subject] = true;
                candidates[subject] = new List<Candidate>();
            }
            else
            {
                isRandom[subject] = false;
                candidates[subject] = list;
            }
        }

        // 3. Эксплуатация: пропускаем только там, где все модели говорят "не спросят".
        for (int day = SimulationSettings.ExplorationDays;
             day < SimulationSettings.Days && !failed;
             day++)
        {
            var todayAsked = new bool[SimulationSettings.Subjects];
            var skip = new bool[SimulationSettings.Subjects];

            for (int subject = 0; subject < SimulationSettings.Subjects; subject++)
            {
                if (isRandom[subject])
                {
                    skip[subject] = false;
                }
                else
                {
                    bool allPredictFalse = true;

                    foreach (Candidate candidate in candidates[subject])
                    {
                        if (candidate.Predict(asked, day))
                        {
                            allPredictFalse = false;
                            break;
                        }
                    }

                    skip[subject] = allPredictFalse;
                }
            }

            for (int subject = 0; subject < SimulationSettings.Subjects; subject++)
            {
                bool actual = TeacherAsks(trueTeachers[subject], asked, day, random);

                if (skip[subject])
                {
                    if (actual)
                    {
                        // Попался — всё накопленное удовольствие обнуляется.
                        failed = true;
                        totalPleasure = 0;
                        break;
                    }

                    todayAsked[subject] = false;
                    totalPleasure++;
                    skipped++;
                }
                else
                {
                    todayAsked[subject] = actual;

                    if (!isRandom[subject])
                    {
                        candidates[subject].RemoveAll(candidate =>
                            candidate.Predict(asked, day) != actual);

                        if (candidates[subject].Count == 0)
                        {
                            // Перестраховка: если модель пропала, дальше ходим всегда.
                            isRandom[subject] = true;
                        }
                    }
                }
            }

            if (failed)
                break;

            asked[day] = todayAsked;

            totalPleasure++;
            pies++;
        }

        if (verbose)
        {
            Console.WriteLine($"Разведка: {SimulationSettings.ExplorationDays} дней.");
            Console.WriteLine("Дальше — адаптивная стратегия.");
        }

        return new SemesterResult
        {
            Pleasure = totalPleasure,
            Skipped = skipped,
            Pies = pies,
            Failed = failed
        };
    }
}
