using System;

public class Movie : Media
{
    public int DurationMinutes { get; private set; }

    public Movie( string title, int releaseYear, int durationMinutes, Director? director) : base(title, releaseYear, director)
    {
        if (durationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes));

        DurationMinutes = durationMinutes;
    }

    public override int CalculateTimeDebt()
    {
        return DurationMinutes;
    }
}