using System;

public class Movie : Media
{
    public int? DurationMinutes { get; private set; }

    public Movie(string title, int releaseYear, int? durationMinutes, Director? director)
        : base(title, releaseYear, director)
    {
        if (durationMinutes.HasValue && durationMinutes.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes));

        DurationMinutes = durationMinutes;
    }

    public override int TotalDurationMinutes => DurationMinutes ?? 0;

    public override void UpdateRuntimeInfo(int length, int count = 1)
    {
        if (length > 0)
            DurationMinutes = length;
    }

    public override int CalculateTimeDebt()
    {
        return DurationMinutes ?? 0;
    }
}