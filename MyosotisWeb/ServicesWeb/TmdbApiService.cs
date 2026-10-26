using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyosotisWeb.ServicesWeb
{
    public class TmdbApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string ImageBaseUrl = "https://image.tmdb.org/t/p/w500";
        private const string BackdropBaseUrl = "https://image.tmdb.org/t/p/w1280";
        private static readonly Dictionary<int, string> GenreCache = new();
        private static bool _genresLoaded = false;

        public TmdbApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private List<Media> MapTmdbResultsToMedia(IEnumerable<TmdbItemDto> results)
        {
            var list = new List<Media>();
            foreach (var item in results)
            {
                int releaseYear = ParseYear(item.ReleaseDate ?? item.FirstAirDate);
                string title = item.Title ?? item.Name ?? "Untitled";

                if (item.MediaType == "movie" || item.MediaType == null)
                {
                    var movie = new Movie(title, releaseYear, 1, null);
                    PopulateBasicMedia(movie, item);
                    list.Add(movie);
                }
                else if (item.MediaType == "tv")
                {
                    var series = new Series(title, releaseYear, 1, 1, null);
                    PopulateBasicMedia(series, item);
                    list.Add(series);
                }
            }
            return list;
        }

        private async Task EnsureGenresLoadedAsync()
        {
            if (_genresLoaded) return;

            try
            {
                var client = _httpClientFactory.CreateClient("TmdbClient");

                var movieGenresTask = client.GetFromJsonAsync<TmdbGenreListResponseDto>("genre/movie/list?language=en-US");
                var tvGenresTask = client.GetFromJsonAsync<TmdbGenreListResponseDto>("genre/tv/list?language=en-US");

                await Task.WhenAll(movieGenresTask, tvGenresTask);

                var movieGenres = await movieGenresTask;
                var tvGenres = await tvGenresTask;

                if (movieGenres?.Genres != null)
                {
                    foreach (var g in movieGenres.Genres)
                    {
                        if (!string.IsNullOrEmpty(g.Name)) GenreCache[g.Id] = g.Name;
                    }
                }

                if (tvGenres?.Genres != null)
                {
                    foreach (var g in tvGenres.Genres)
                    {
                        if (!string.IsNullOrEmpty(g.Name)) GenreCache[g.Id] = g.Name;
                    }
                }

                _genresLoaded = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Genre Fetch Error]: {ex.Message}");
            }
        }

        public async Task<List<Media>> SearchMediaAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<Media>();

            try
            {
                await EnsureGenresLoadedAsync();
                var client = _httpClientFactory.CreateClient("TmdbClient");
                var response = await client.GetFromJsonAsync<TmdbSearchResponse>(
                    $"search/multi?query={Uri.EscapeDataString(query)}&include_adult=false&language=en-US&page=1");

                return response?.Results != null ? MapTmdbResultsToMedia(response.Results) : new List<Media>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Search Error]: {ex.Message}");
                return new List<Media>();
            }
        }

        public async Task<Movie?> GetMovieDetailsAsync(int movieId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TmdbClient");
                var dto = await client.GetFromJsonAsync<TmdbMovieDetailsDto>($"movie/{movieId}?language=en-US");

                if (dto == null) return null;

                int releaseYear = ParseYear(dto.ReleaseDate);
                string title = dto.Title ?? "Untitled";

                var movie = new Movie(title, releaseYear, dto.Runtime, null);
                movie.SetTmdbId(dto.Id);

                PopulateFullMedia(movie, dto.Overview, dto.VoteAverage, dto.PosterPath,
                                   dto.Adult, dto.Status, dto.Genres,
                                   dto.ProductionCompanies, dto.ProductionCountries);

                return movie;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Movie Details Error]: {ex.Message}");
                return null;
            }
        }

        public async Task<Series?> GetSeriesDetailsAsync(int seriesId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TmdbClient");
                var dto = await client.GetFromJsonAsync<TmdbSeriesDetailsDto>($"tv/{seriesId}?language=en-US");

                if (dto == null) return null;

                int releaseYear = ParseYear(dto.FirstAirDate);
                string title = dto.Name ?? "Untitled";
                int avgRuntime = dto.EpisodeRunTime?.FirstOrDefault() ?? 0;

                var series = new Series(title, releaseYear, avgRuntime, dto.NumberOfSeasons, null);
                series.SetTmdbId(dto.Id);

                PopulateFullMedia(series, dto.Overview, dto.VoteAverage, dto.PosterPath,
                                   dto.Adult, dto.Status, dto.Genres,
                                   dto.ProductionCompanies, dto.ProductionCountries);

                return series;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Series Details Error]: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Media>> GetTrendingMediaAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TmdbClient");
                var response = await client.GetFromJsonAsync<TmdbSearchResponse>("trending/all/week?language=en-US");

                return response?.Results != null ? MapTmdbResultsToMedia(response.Results) : new List<Media>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Trending Error]: {ex.Message}");
                return new List<Media>();
            }
        }

        public async Task EnrichMediaDetailsAsync(Media media)
        {
            if (!media.TmdbId.HasValue) return;

            try
            {
                var client = _httpClientFactory.CreateClient("TmdbClient");

                string detailsEndpoint = media is Movie
                    ? $"movie/{media.TmdbId}?append_to_response=credits"
                    : $"tv/{media.TmdbId}?append_to_response=credits";

                var response = await client.GetFromJsonAsync<TmdbFullDetailsDto>(detailsEndpoint);
                if (response != null)
                {
                    if (media is Movie && response.Runtime.HasValue)
                    {
                        media.UpdateRuntimeInfo(response.Runtime.Value);
                    }
                    else if (media is Series seriesObj)
                    {
                        int rawLength = response.EpisodeRunTime?.FirstOrDefault() ?? 0;
                        int avgLength = rawLength > 0 ? rawLength : 45;
                        seriesObj.UpdateRuntimeInfo(avgLength);

                        await PopulateSeriesSeasonsAndEpisodesAsync(seriesObj, seriesObj.TmdbId.Value);
                    }

                    if (response.ProductionCountries != null && response.ProductionCountries.Any())
                    {
                        string country = response.ProductionCountries.First().Name;
                        string studio = response.ProductionCompanies?.FirstOrDefault()?.Name ?? "Unknown Studio";
                        media.SetProductionDetails(studio, country);
                    }

                    if (response.Credits != null)
                    {
                        if (response.Credits.Cast != null)
                        {
                            media.Cast.Clear();
                            foreach (var c in response.Credits.Cast.Take(5))
                            {
                                media.AddCastMember(new Actor(c.Name, string.IsNullOrEmpty(c.Character) ? "Unknown" : c.Character));
                            }
                        }

                        var directorDto = response.Credits.Crew?.FirstOrDefault(c => c.Job == "Director" || c.Job == "Executive Producer");
                        if (directorDto != null)
                        {
                            media.SetDirector(new Director(directorDto.Name, "Classic"));
                        }
                    }
                }

                string reviewsEndpoint = media is Movie
                    ? $"movie/{media.TmdbId}/reviews?language=en-US&page=1"
                    : $"tv/{media.TmdbId}/reviews?language=en-US&page=1";

                var reviewsResponse = await client.GetFromJsonAsync<TmdbReviewsResponseDto>(reviewsEndpoint);

                if (reviewsResponse?.Results != null && reviewsResponse.Results.Count > 0)
                {
                    var tmdbReviewsList = new List<TmdbReview>();

                    foreach (var revDto in reviewsResponse.Results.Take(5))
                    {
                        if (string.IsNullOrWhiteSpace(revDto.Content)) continue;

                        string authorName = !string.IsNullOrWhiteSpace(revDto.AuthorDetails?.Username)
                            ? revDto.AuthorDetails.Username
                            : (!string.IsNullOrWhiteSpace(revDto.Author) ? revDto.Author : "TMDB User");

                        tmdbReviewsList.Add(new TmdbReview
                        {
                            Author = authorName,
                            Content = revDto.Content
                        });
                    }

                    media.SetTmdbReviews(tmdbReviewsList);
                }

                if (response?.Genres != null && response.Genres.Count > 0)
                {
                    foreach (var g in response.Genres)
                    {
                        if (!string.IsNullOrWhiteSpace(g.Name))
                        {
                            media.AddGenre(g.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Details Error]: {ex.Message}");
            }
        }

        private async Task PopulateSeriesSeasonsAndEpisodesAsync(Series series, int seriesId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TmdbClient");
                var seriesDetails = await client.GetFromJsonAsync<TmdbSeriesDetailsDto>($"tv/{seriesId}?language=en-US");

                if (seriesDetails?.Seasons == null || seriesDetails.Seasons.Count == 0) return;

                series.Seasons?.Clear();

                var uniqueSeasons = seriesDetails.Seasons
                    .Where(s => s.SeasonNumber > 0)
                    .GroupBy(s => s.SeasonNumber)
                    .Select(g => g.First())
                    .OrderBy(s => s.SeasonNumber);

                foreach (var seasonBrief in uniqueSeasons)
                {
                    var seasonDto = await client.GetFromJsonAsync<TmdbSeasonDetailsDto>(
                        $"tv/{seriesId}/season/{seasonBrief.SeasonNumber}?language=en-US");

                    if (seasonDto == null) continue;

                    var season = new Season(seasonDto.SeasonNumber);

                    if (seasonDto.Episodes != null)
                    {
                        foreach (var epDto in seasonDto.Episodes)
                        {
                            int runtime = epDto.Runtime ?? series.AverageEpisodeLength;
                            if (runtime <= 0) runtime = 45;

                            DateTime? airDate = null;
                            if (DateTime.TryParse(epDto.AirDate, out var parsedDate))
                            {
                                airDate = parsedDate;
                            }

                            string epTitle = string.IsNullOrWhiteSpace(epDto.Name)
                                ? $"Episode {epDto.EpisodeNumber}"
                                : epDto.Name;

                            var episode = new Episode(epDto.EpisodeNumber, epTitle, runtime, airDate);
                            season.AddEpisode(episode);
                        }
                    }

                    series.AddSeason(season);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Season Episodes Error]: {ex.Message}");
            }
        }

        private void PopulateBasicMedia(Media media, TmdbItemDto item)
        {
            if (item.Id > 0) media.SetTmdbId(item.Id);
            if (!string.IsNullOrEmpty(item.Overview)) media.UpdateDescription(item.Overview);
            if (item.VoteAverage > 0) media.SetTmdbRating(item.VoteAverage);
            if (!string.IsNullOrEmpty(item.PosterPath)) media.SetPosterUrl($"{ImageBaseUrl}{item.PosterPath}");
            if (!string.IsNullOrEmpty(item.BackdropPath)) media.SetBackDropUrl($"{BackdropBaseUrl}{item.BackdropPath}");

            if (item.GenreIds != null && item.GenreIds.Count > 0)
            {
                foreach (var id in item.GenreIds)
                {
                    if (GenreCache.TryGetValue(id, out var genreName))
                    {
                        media.AddGenre(genreName);
                    }
                }
            }
        }

        private void PopulateFullMedia(Media media, string? overview, double voteAvg, string? posterPath,
                                       bool adult, string? status, List<TmdbGenreDto>? genres,
                                       List<TmdbCompanyDto>? companies, List<TmdbCountryDto>? countries,
                                       List<TmdbReview>? tmdbReviews = null)
        {
            if (!string.IsNullOrEmpty(overview))
                media.UpdateDescription(overview);

            if (voteAvg > 0)
                media.SetTmdbRating(voteAvg);

            if (!string.IsNullOrEmpty(posterPath))
                media.SetPosterUrl($"{ImageBaseUrl}{posterPath}");

            string studio = companies?.FirstOrDefault()?.Name ?? "Невідомо";
            string country = countries?.FirstOrDefault()?.Name ?? "Невідомо";
            media.SetProductionDetails(studio, country);

            int ageLimit = adult ? 18 : 0;
            string contentRating = status ?? "Released";
            media.ApplyAgeRestriction(ageLimit, contentRating);

            if (genres != null)
            {
                foreach (var genreDto in genres)
                {
                    if (!string.IsNullOrWhiteSpace(genreDto.Name))
                    {
                        media.AddGenre(genreDto.Name);
                    }
                }
            }

            if (tmdbReviews != null)
            {
                media.SetTmdbReviews(tmdbReviews);
            }
        }

        private int ParseYear(string? dateStr)
        {
            if (!string.IsNullOrEmpty(dateStr) && dateStr.Length >= 4 && int.TryParse(dateStr.Substring(0, 4), out int year))
            {
                return year;
            }
            return DateTime.Now.Year;
        }
    }

    // ==========================================
    // СКОРОЧЕНІ ТА ЗГРУПОВАНІ DTO-КЛАСИ
    // ==========================================

    public class TmdbSearchResponse { [JsonPropertyName("results")] public List<TmdbItemDto>? Results { get; set; } }
    public class TmdbGenreListResponseDto { [JsonPropertyName("genres")] public List<TmdbGenreDto>? Genres { get; set; } }
    public class TmdbReviewsResponseDto { [JsonPropertyName("results")] public List<TmdbReviewItemDto>? Results { get; set; } }

    public class TmdbItemDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("media_type")] public string? MediaType { get; set; }
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("overview")] public string? Overview { get; set; }
        [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
        [JsonPropertyName("backdrop_path")] public string? BackdropPath { get; set; }
        [JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }
        [JsonPropertyName("first_air_date")] public string? FirstAirDate { get; set; }
        [JsonPropertyName("vote_average")] public double VoteAverage { get; set; }
        [JsonPropertyName("genre_ids")] public List<int>? GenreIds { get; set; }
    }

    public class TmdbMovieDetailsDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("overview")] public string? Overview { get; set; }
        [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
        [JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }
        [JsonPropertyName("vote_average")] public double VoteAverage { get; set; }
        [JsonPropertyName("runtime")] public int Runtime { get; set; }
        [JsonPropertyName("adult")] public bool Adult { get; set; }
        [JsonPropertyName("status")] public string? Status { get; set; }
        [JsonPropertyName("genres")] public List<TmdbGenreDto>? Genres { get; set; }
        [JsonPropertyName("production_companies")] public List<TmdbCompanyDto>? ProductionCompanies { get; set; }
        [JsonPropertyName("production_countries")] public List<TmdbCountryDto>? ProductionCountries { get; set; }
    }

    public class TmdbSeriesDetailsDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("overview")] public string? Overview { get; set; }
        [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
        [JsonPropertyName("first_air_date")] public string? FirstAirDate { get; set; }
        [JsonPropertyName("vote_average")] public double VoteAverage { get; set; }
        [JsonPropertyName("episode_run_time")] public List<int>? EpisodeRunTime { get; set; }
        [JsonPropertyName("number_of_seasons")] public int NumberOfSeasons { get; set; }
        [JsonPropertyName("number_of_episodes")] public int NumberOfEpisodes { get; set; }
        [JsonPropertyName("adult")] public bool Adult { get; set; }
        [JsonPropertyName("status")] public string? Status { get; set; }
        [JsonPropertyName("seasons")] public List<TmdbSeasonBriefDto>? Seasons { get; set; }
        [JsonPropertyName("genres")] public List<TmdbGenreDto>? Genres { get; set; }
        [JsonPropertyName("production_companies")] public List<TmdbCompanyDto>? ProductionCompanies { get; set; }
        [JsonPropertyName("production_countries")] public List<TmdbCountryDto>? ProductionCountries { get; set; }
    }

    public class TmdbSeasonBriefDto
    {
        [JsonPropertyName("season_number")] public int SeasonNumber { get; set; }
        [JsonPropertyName("episode_count")] public int EpisodeCount { get; set; }
    }

    public class TmdbSeasonDetailsDto
    {
        [JsonPropertyName("season_number")] public int SeasonNumber { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("overview")] public string? Overview { get; set; }
        [JsonPropertyName("episodes")] public List<TmdbEpisodeDto>? Episodes { get; set; }
    }

    public class TmdbEpisodeDto
    {
        [JsonPropertyName("episode_number")] public int EpisodeNumber { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("overview")] public string? Overview { get; set; }
        [JsonPropertyName("runtime")] public int? Runtime { get; set; }
        [JsonPropertyName("air_date")] public string? AirDate { get; set; }
    }

    public class TmdbGenreDto { [JsonPropertyName("id")] public int Id { get; set; } [JsonPropertyName("name")] public string? Name { get; set; } }
    public class TmdbCompanyDto { [JsonPropertyName("name")] public string? Name { get; set; } }
    public class TmdbCountryDto { [JsonPropertyName("name")] public string? Name { get; set; } }

    public class TmdbReviewItemDto
    {
        [JsonPropertyName("author")] public string? Author { get; set; }
        [JsonPropertyName("content")] public string? Content { get; set; }
        [JsonPropertyName("author_details")] public TmdbAuthorDetailsDto? AuthorDetails { get; set; }
    }

    public class TmdbAuthorDetailsDto { [JsonPropertyName("username")] public string? Username { get; set; } }

    public class TmdbFullDetailsDto
    {
        [JsonPropertyName("runtime")] public int? Runtime { get; set; }
        [JsonPropertyName("episode_run_time")] public List<int>? EpisodeRunTime { get; set; }
        [JsonPropertyName("genres")] public List<TmdbGenreDto>? Genres { get; set; }
        [JsonPropertyName("production_countries")] public List<TmdbCountryDto>? ProductionCountries { get; set; }
        [JsonPropertyName("production_companies")] public List<TmdbCompanyDto>? ProductionCompanies { get; set; }
        [JsonPropertyName("credits")] public TmdbCredits? Credits { get; set; }
    }

    public class TmdbCredits
    {
        [JsonPropertyName("cast")] public List<TmdbCast>? Cast { get; set; }
        [JsonPropertyName("crew")] public List<TmdbCrew>? Crew { get; set; }
    }

    public class TmdbCast
    {
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("character")] public string Character { get; set; } = "";
    }

    public class TmdbCrew
    {
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("job")] public string Job { get; set; } = "";
    }
}