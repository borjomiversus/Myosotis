using System;

public class Episode
{
    public int Number { get; private set; }
    public string Title { get; private set; }
    public int RuntimeMinutes { get; private set; }
    public DateTime? AirDate { get; private set; }

    public Episode(int number, string title, int runtimeMinutes, DateTime? airDate = null)
    {
        if (number <= 0) throw new ArgumentOutOfRangeException(nameof(number));
        if (string.IsNullOrEmpty(title)) throw new ArgumentNullException(nameof(title));
        if (runtimeMinutes <= 0) throw new ArgumentOutOfRangeException(nameof(runtimeMinutes));
    
        Number = number;
        Title = title;
        RuntimeMinutes = runtimeMinutes;
        AirDate = airDate;
    }
}