namespace MasterProgulov.Core;

public sealed record DayResult(int DayNumber, int TotalPleasure, double AveragePleasurePerDay, bool Eliminated, bool SemesterStopped, IReadOnlyDictionary<Subject, bool> AskedToday, int PleasureGainedToday);
