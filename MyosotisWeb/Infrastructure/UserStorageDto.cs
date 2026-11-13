public class UserStorageDto
{
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public Guid Id { get; set; }

    public string Username { get; set; } = "Guest";
    public List<WatchlistDto> Watchlists { get; set; } = new();
    public List<CaptureEntryDto> Captures { get; set; } = new();
    public List<string> RecentSearches { get; set; } = new();
    public List<MediaDto> RecentlyViewed { get; set; } = new();
    public List<WatchHistoryEntryDto> WatchHistory { get; set; } = new();
    public List<UserMediaStateDto> MediaStates { get; set; } = new();
    public List<WatchCheckpointDto> Checkpoints { get; set; } = new();
    public List<CaptureEntryDto> ActiveCaptures { get; set; } = new();
}

public class WatchlistDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsPrivate { get; set; }
    public List<WatchlistEntryDto> Entries { get; set; } = new();
}

public class WatchlistEntryDto
{
    public Guid Id { get; set; }
    public MediaDto? Item { get; set; }
    public string? PersonalNote { get; set; }
    public DateTime AddedAt { get; set; }
}

public class CaptureEntryDto
{
    public Guid Id { get; set; }
    public string RawNote { get; set; } = "";
    public string? Source { get; set; }
    public DateTime CapturedAt { get; set; }
    public CaptureStatus Status { get; set; }
    public MediaDto? ResolvedMedia { get; set; }
    public CaptureKind Kind { get; set; }
    public string? Timecode { get; set; }
    public string? Annotation { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class WatchHistoryEntryDto
{
    public Guid Id { get; set; }
    public MediaDto? WatchedItem { get; set; }
    public DateTime WatchDate { get; set; }
    public ActivityAction Action { get; set; }
}

public class UserMediaStateDto
{
    public Guid Id { get; set; }
    public MediaDto? Item { get; set; }
    public WatchStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsFavorite { get; set; }
}

public class WatchCheckpointDto
{
    public Guid Id { get; set; }
    public MediaDto? PausedMedia { get; set; }
    public int? Season { get; set; }
    public int? Episode { get; set; }
    public string Timecode { get; set; } = "";
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class MediaDto
{
    public Guid Id { get; set; }
    public string MediaType { get; set; } = "Movie";
    public string Title { get; set; } = "";
    public int ReleaseYear { get; set; }
    public string? ProductionStudio { get; set; }
    public string? PosterUrl { get; set; }
    public string? Description { get; set; }
    public double? TmdbRating { get; set; }
    public int? TmdbId { get; set; }
    public string? BackdropUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public string? ContentRating { get; set; }
    public string? Country { get; set; }
    public int? AgeRestriction { get; set; }
    public List<TmdbReviewDto> TmdbReviews { get; set; } = new();
    public int? DurationMinutes { get; set; }
    public int? AverageEpisodeLength { get; set; }
    public int UnwatchedEpisodes { get; set; }
    public List<string> Genres { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public VibeRatingDto? Vibe { get; set; }
    public List<SeasonDto> Seasons { get; set; } = new();

    public class TmdbReviewDto
    {
        public string Author { get; set; } = "";
        public string Content { get; set; } = "";
    }
}

public class SeasonDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public List<EpisodeDto> Episodes { get; set; } = new();
}

public class EpisodeDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Title { get; set; } = "";
    public int RuntimeMinutes { get; set; }
    public DateTime? AirDate { get; set; }
}

public class VibeRatingDto
{
    public int Characters { get; set; }
    public int Plot { get; set; }
    public int Visuals { get; set; }
    public int Pacing { get; set; }
    public int Vibe { get; set; }
    public int Soundtrack { get; set; }
}
