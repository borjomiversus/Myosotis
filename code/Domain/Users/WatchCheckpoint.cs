using System;
using System.Linq;

// запис "де я зупинилась і чому"
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

    public static int GetTargetDurationMinutes(Media media, int? seasonNumber, int? episodeNumber)
    {
        if (media is Movie movie)
            return movie.DurationMinutes ?? 0;

        if (media is Series series)
        {
            var season = series.Seasons.FirstOrDefault(s => s.Number == seasonNumber);
            var ep = season?.Episodes.FirstOrDefault(e => e.Number == episodeNumber);
            if (ep != null && ep.RuntimeMinutes > 0)
                return ep.RuntimeMinutes;

            return series.AverageEpisodeLength ?? 0;
        }

        return media.CalculateTimeDebt();
    }

    public static bool TryValidateAndCreate(
        Media media,
        string? inputTimecode,
        string? inputReason,
        int inputSeason,
        int inputEpisode,
        out WatchCheckpoint? checkpoint,
        out string? error)
    {
        checkpoint = null;
        error = null;

        if (string.IsNullOrWhiteSpace(inputTimecode))
        {
            error = "Please enter a timecode.";
            return false;
        }

        if (!TimecodeHelper.TryParseSeconds(inputTimecode.Trim(), out int enteredSeconds, out string parseError))
        {
            error = parseError;
            return false;
        }

        if (media is Series series)
        {
            var validSeasons = series.Seasons.Where(s => s.Number > 0 && s.Number < 100).ToList();
            var seasonObj = validSeasons.FirstOrDefault(s => s.Number == inputSeason);

            if (validSeasons.Count > 0 && seasonObj == null)
            {
                error = $"Season {inputSeason} does not exist.";
                return false;
            }

            if (seasonObj != null && seasonObj.Episodes.Count > 0 && !seasonObj.Episodes.Any(e => e.Number == inputEpisode))
            {
                error = $"Episode {inputEpisode} does not exist in Season {inputSeason}.";
                return false;
            }
        }

        int maxMinutes = GetTargetDurationMinutes(media, inputSeason, inputEpisode);
        if (maxMinutes > 0 && enteredSeconds > maxMinutes * 60)
        {
            error = $"Timecode exceeds total duration ({TimecodeHelper.FormatMinutesAsTimecode(maxMinutes)} / {maxMinutes} mins).";
            return false;
        }

        if (media is Series sItem)
        {
            checkpoint = new WatchCheckpoint(sItem, inputSeason, inputEpisode, inputTimecode.Trim(), inputReason);
            return true;
        }

        if (media is Movie movie)
        {
            checkpoint = new WatchCheckpoint(movie, inputTimecode.Trim(), inputReason);
            return true;
        }

        return false;
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
        if (!TimecodeHelper.TryParseSeconds(Timecode, out int seconds, out _))
            return 0;

        int totalMinutes = GetTargetDurationMinutes(PausedMedia, Season, Episode);
        int totalSeconds = totalMinutes * 60;
        if (totalSeconds <= 0) return 0;

        int percent = (seconds * 100) / totalSeconds;
        return percent > 100 ? 100 : percent;
    }

    public void RestoreStorageData(Guid id, DateTime createdAt)
    {
        if (id == Guid.Empty) throw new ArgumentException("Checkpoint ID cannot be empty.");
        Id = id;
        CreatedAt = createdAt;
    }
}