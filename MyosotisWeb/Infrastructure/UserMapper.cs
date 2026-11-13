using System;
using System.Collections.Generic;
using System.Linq;
using static MediaDto;

namespace MyosotisWeb.Infrastructure
{
    public static class UserMapper
    {
        public static UserStorageDto ToDto(User user, DateTime createdAt)
        {
            return new UserStorageDto
            {
                Version = 1,
                CreatedAt = createdAt,
                LastUpdatedAt = DateTime.UtcNow,
                Id = user.Id,
                Username = user.Username,
                Watchlists = user.Watchlists.Select(w => new WatchlistDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    IsPrivate = w.IsPrivate,
                    Entries = w.Entries.Select(e => new WatchlistEntryDto
                    {
                        Id = e.Id,
                        Item = MapMediaToDto(e.Item),
                        PersonalNote = e.PersonalNote,
                        AddedAt = e.AddedAt
                    }).ToList()
                }).ToList(),
                Captures = user.Captures.Select(MapCaptureToDto).ToList(),
                RecentSearches = user.RecentSearches.ToList(),
                RecentlyViewed = user.RecentlyViewed.Select(MapMediaToDto).ToList(),
                WatchHistory = user.WatchHistory.Select(h => new WatchHistoryEntryDto
                {
                    Id = h.Id,
                    WatchedItem = MapMediaToDto(h.WatchedItem),
                    WatchDate = h.WatchDate,
                    Action = h.Action
                }).ToList(),
                MediaStates = user.MediaStates.Select(s => new UserMediaStateDto
                {
                    Id = s.Id,
                    Item = MapMediaToDto(s.Item),
                    Status = s.Status,
                    StartedAt = s.StartedAt,
                    CompletedAt = s.CompletedAt,
                    IsFavorite = s.IsFavorite
                }).ToList(),
                Checkpoints = user.Checkpoints.Select(cp => new WatchCheckpointDto
                {
                    Id = cp.Id,
                    PausedMedia = MapMediaToDto(cp.PausedMedia),
                    Season = cp.Season,
                    Episode = cp.Episode,
                    Timecode = cp.Timecode,
                    Reason = cp.Reason,
                    CreatedAt = cp.CreatedAt,
                    IsActive = cp.IsActive
                }).ToList(),

                ActiveCaptures = user.Captures
                    .Where(c => c.Status != CaptureStatus.Archived)
                    .OrderByDescending(c => c.CapturedAt)
                    .Select(MapCaptureToDto)
                    .ToList()
            };
        }

        public static User ToDomain(UserStorageDto dto)
        {
            var user = new User(dto.Username);
            var mediaCache = new Dictionary<Guid, Media>();

            foreach (var wDto in dto.Watchlists ?? new())
            {
                var watchlist = user.CreateWatchlist(wDto.Name, wDto.IsPrivate);
                watchlist.RestoreId(wDto.Id);
                foreach (var eDto in wDto.Entries ?? new())
                {
                    if (eDto.Item != null)
                    {
                        var media = GetOrCreateMedia(eDto.Item, mediaCache);
                        watchlist.AddEntry(media, eDto.PersonalNote);

                        var entry = watchlist.Entries.Last();
                        entry.RestoreStorageData(eDto.Id, eDto.AddedAt);
                    }
                }
            }

            foreach (var cDto in dto.Captures ?? new())
            {
                var capture = user.AddCapture(cDto.RawNote,cDto.Source,cDto.Kind,cDto.Timecode);

                capture.SetAnnotation(cDto.Annotation);

                if (cDto.ResolvedMedia != null)
                {
                    capture.Resolve(GetOrCreateMedia(cDto.ResolvedMedia, mediaCache));
                }

                capture.RestoreStorageData(cDto.Id,cDto.CapturedAt,cDto.Status,cDto.Tags);
            }

            foreach (var sDto in dto.MediaStates ?? new())
            {
                if (sDto.Item == null)
                    continue;

                var media = GetOrCreateMedia(sDto.Item, mediaCache);

                user.SetStatus(media, sDto.Status);

                var state = user.MediaStates.LastOrDefault(
                    s => s.Item.Id == sDto.Item.Id);

                if (state != null)
                {
                    state.RestoreStorageData(sDto.Id,sDto.StartedAt,sDto.CompletedAt);

                    if (sDto.IsFavorite)
                        user.ToggleFavorite(media);
                }
            }

            foreach (var cpDto in dto.Checkpoints ?? new())
            {
                if (cpDto.PausedMedia != null)
                {
                    var media = GetOrCreateMedia(cpDto.PausedMedia, mediaCache);
                    WatchCheckpoint checkpoint;
                    if (media is Series series && cpDto.Season.HasValue && cpDto.Episode.HasValue)
                    {
                        checkpoint = new WatchCheckpoint(series, cpDto.Season.Value, cpDto.Episode.Value, cpDto.Timecode, cpDto.Reason);
                    }
                    else if (media is Movie movie)
                    {
                        checkpoint = new WatchCheckpoint(movie, cpDto.Timecode, cpDto.Reason);
                    }
                    else
                    {
                        continue;
                    }

                    checkpoint.RestoreStorageData(cpDto.Id, cpDto.CreatedAt);

                    if (!cpDto.IsActive)
                    {
                        checkpoint.ResumeWatching();
                    }
                    user.Checkpoints.Add(checkpoint);
                }
            }

            foreach (var hDto in dto.WatchHistory ?? new())
            {
                if (hDto.WatchedItem == null)
                    continue;

                var media = GetOrCreateMedia(hDto.WatchedItem, mediaCache);

                var historyEntry = new WatchHistoryEntry(media,hDto.WatchDate,hDto.Action);

                historyEntry.RestoreStorageData(hDto.Id);

                user.WatchHistory.Add(historyEntry);
            }

            if (dto.RecentSearches != null)
            {
                user.RecentSearches.AddRange(dto.RecentSearches);
            }

            if (dto.RecentlyViewed != null)
            {
                foreach (var mediaDto in dto.RecentlyViewed)
                {
                    if (mediaDto != null)
                    {
                        user.RecentlyViewed.Add(GetOrCreateMedia(mediaDto, mediaCache));
                    }
                }
            }

            return user;
        }

        private static MediaDto MapMediaToDto(Media media)
        {
            var dto = new MediaDto
            {
                Id = media.Id,
                Title = media.Title,
                ReleaseYear = media.ReleaseYear,
                ProductionStudio = media.ProductionStudio,
                PosterUrl = media.PosterUrl,
                Description = media.Description,
                TmdbRating = media.TmdbRating,
                TmdbId = media.TmdbId,
                Genres = media.Genres?.ToList() ?? new(),
                Tags = media.Tags?.ToList() ?? new(),
                BackdropUrl = media.BackdropUrl,
                TrailerUrl = media.TrailerUrl,
                ContentRating = media.ContentRating,
                Country = media.Country,
                AgeRestriction = media.AgeRestriction,

                TmdbReviews = media.TmdbReviews?
                    .Select(r => new TmdbReviewDto
                    {
                        Author = r.Author,
                        Content = r.Content
                    })
                    .ToList() ?? new()
            };

            if (media is Movie movie)
            {
                dto.MediaType = "Movie";
                dto.DurationMinutes = movie.DurationMinutes;
            }
            else if (media is Series series)
            {
                dto.MediaType = "Series";
                dto.AverageEpisodeLength = series.AverageEpisodeLength;
                dto.UnwatchedEpisodes = series.UnwatchedEpisodes;

                dto.Seasons = series.Seasons.Select(season => new SeasonDto
                {
                    Id = season.Id,
                    Number = season.Number,
                    Episodes = season.Episodes.Select(episode => new EpisodeDto
                    {
                        Id = episode.Id,
                        Number = episode.Number,
                        Title = episode.Title,
                        RuntimeMinutes = episode.RuntimeMinutes,
                        AirDate = episode.AirDate
                    }).ToList()
                }).ToList();
            }

            if (media.Vibe != null)
            {
                dto.Vibe = new VibeRatingDto
                {
                    Characters = media.Vibe.Characters,
                    Plot = media.Vibe.Plot,
                    Visuals = media.Vibe.Visuals,
                    Pacing = media.Vibe.Pacing,
                    Vibe = media.Vibe.Vibe,
                    Soundtrack = media.Vibe.Soundtrack
                };
            }

            return dto;
        }

        private static Media MapMediaToDomain(MediaDto dto)
        {
            Media media;
            if (dto.MediaType == "Series")
            {
                var series = new Series(dto.Title, dto.ReleaseYear, dto.AverageEpisodeLength, dto.UnwatchedEpisodes);

                var seasons = (dto.Seasons ?? new List<SeasonDto>())
                    .Select(s =>
                    {
                        var season = new Season(s.Number);
                        season.RestoreId(s.Id);

                        foreach (var e in s.Episodes ?? new List<EpisodeDto>())
                        {
                            var episode = new Episode(e.Number, e.Title, e.RuntimeMinutes, e.AirDate);
                            episode.RestoreId(e.Id);
                            season.AddEpisode(episode);
                        }

                        return season;
                    })
                    .ToList();

                series.RestoreSeasons(seasons);
                media = series;
            }
            else
            {
                media = new Movie(dto.Title, dto.ReleaseYear, dto.DurationMinutes, null);
            }

            media.RestoreId(dto.Id);

            if (!string.IsNullOrEmpty(dto.PosterUrl)) media.SetPosterUrl(dto.PosterUrl);
            if (!string.IsNullOrEmpty(dto.Description)) media.UpdateDescription(dto.Description);
            if (dto.TmdbRating.HasValue) media.SetTmdbRating(dto.TmdbRating.Value);
            if (dto.TmdbId.HasValue) media.SetTmdbId(dto.TmdbId.Value);

            media.RestoreAdditionalData(dto.BackdropUrl, dto.TrailerUrl, dto.ContentRating, dto.Country, dto.AgeRestriction, dto.TmdbReviews?
                    .Select(r => new TmdbReview
                    {
                        Author = r.Author,
                        Content = r.Content
                    })
                    .ToList()
            );

            if (dto.Vibe != null)
            {
                var vibeRating = new VibeRating(dto.Vibe.Characters, dto.Vibe.Plot, dto.Vibe.Visuals,dto.Vibe.Pacing, dto.Vibe.Vibe, dto.Vibe.Soundtrack);
                media.SetVibeRating(vibeRating);
            }

            if (dto.Genres != null)
            {
                foreach (var g in dto.Genres) media.AddGenre(g);
            }
            if (dto.Tags != null)
            {
                foreach (var t in dto.Tags) media.AddTag(t);
            }

            return media;
        }

        private static CaptureEntryDto MapCaptureToDto(CaptureEntry c)
        {
            return new CaptureEntryDto
            {
                Id = c.Id,
                RawNote = c.RawNote,
                Source = c.Source,
                CapturedAt = c.CapturedAt,
                Status = c.Status,
                ResolvedMedia = c.ResolvedMedia != null ? MapMediaToDto(c.ResolvedMedia) : null,
                Kind = c.Kind,
                Timecode = c.Timecode,
                Annotation = c.Annotation,
                Tags = c.Tags?.ToList() ?? new()
            };
        }

        private static Media GetOrCreateMedia(
            MediaDto dto,
            Dictionary<Guid, Media> cache)
        {
            if (dto.Id != Guid.Empty &&
                cache.TryGetValue(dto.Id, out var existing))
            {
                return existing;
            }

            var media = MapMediaToDomain(dto);

            if (dto.Id != Guid.Empty)
                cache[dto.Id] = media;

            return media;
        }
    }
}