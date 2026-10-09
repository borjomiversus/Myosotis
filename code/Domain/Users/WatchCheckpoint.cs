using System;

// запис "де я зупинилась і чому".
public class WatchCheckpoint
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Series PausedSeries { get; private set; }
    public int Season { get; private set; }
    public int Episode { get; private set; }
    public string Timecode { get; private set; }
    public string? Reason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    public WatchCheckpoint(Series series, int season, int episode, string timecode, string? reason)
    {
        PausedSeries = series ?? throw new ArgumentNullException(nameof(series));
        Season = season;
        Episode = episode;
        Timecode = timecode;
        Reason = reason;
        CreatedAt = DateTime.Now;
        IsActive = true;
    }

    public string ResumeWatching()
    {
        IsActive = false;
        return $"Перегляд '{PausedSeries.Title}' відновлено з S{Season}E{Episode}, {Timecode}.";
    }

    public string GetStatusDescription()
    {
        return IsActive
            ? $"На паузі: S{Season}E{Episode} ({Timecode}). Причина: {Reason}"
            : "Перегляд відновлено";
    }

    public int GetProgressPercent()
    {
        string[] parts = Timecode.Split(':');
        int seconds = 0;
        foreach (string part in parts)
        {
            int number;
            if (!int.TryParse(part, out number))
                return 0;
            seconds = seconds * 60 + number;
        }

        int totalSeconds = PausedSeries.AverageEpisodeLength * 60;
        if (totalSeconds <= 0)
            return 0;

        int percent = seconds * 100 / totalSeconds;
        return percent > 100 ? 100 : percent;
    }
}