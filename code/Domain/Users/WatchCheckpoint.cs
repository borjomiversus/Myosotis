using System;

// запис "де я зупинилась і чому".
public class WatchCheckpoint
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Media PausedMedia { get; private set; }
    public int? Season { get; private set; }      
    public int? Episode { get; private set; }       
    public string Timecode { get; private set; }
    public string? Reason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    public WatchCheckpoint(Series series, int season, int episode, string timecode, string? reason)
    {
        PausedMedia = series ?? throw new ArgumentNullException(nameof(series));
        Season = season;
        Episode = episode;
        Timecode = timecode;
        Reason = reason;
        CreatedAt = DateTime.Now;
        IsActive = true;
    }

    public WatchCheckpoint(Movie movie, string timecode, string? reason)
    {
        PausedMedia = movie ?? throw new ArgumentNullException(nameof(movie));
        Season = null;
        Episode = null;
        Timecode = timecode;
        Reason = reason;
        CreatedAt = DateTime.Now;
        IsActive = true;
    }

    public string ResumeWatching()
    {
        IsActive = false;
        if (PausedMedia is Series && Season.HasValue && Episode.HasValue)
        {
            return $"Resumed watching '{PausedMedia.Title}' from S{Season}E{Episode}, {Timecode}.";
        }
        return $"Resumed watching '{PausedMedia.Title}' from {Timecode}.";
    }

    public string GetStatusDescription()
    {
        if (!IsActive) return "Watching resumed";

        string location = (PausedMedia is Series && Season.HasValue && Episode.HasValue)
            ? $"S{Season}E{Episode} ({Timecode})"
            : $"timecode {Timecode}";

        return string.IsNullOrWhiteSpace(Reason)
            ? $"Paused: {location}"
            : $"Paused: {location}. Reason: {Reason}";
    }

    public int GetProgressPercent()
    {
        string[] parts = Timecode.Split(':');
        int seconds = 0;
        foreach (string part in parts)
        {
            if (!int.TryParse(part, out int number))
                return 0;
            seconds = seconds * 60 + number;
        }

        int totalMinutes = PausedMedia switch
        {
            Movie movie => movie.DurationMinutes,
            Series series => series.AverageEpisodeLength,
            _ => PausedMedia.CalculateTimeDebt()
        };

        int totalSeconds = totalMinutes * 60;
        if (totalSeconds <= 0) return 0;

        int percent = (seconds * 100) / totalSeconds;
        return percent > 100 ? 100 : percent;
    }
}