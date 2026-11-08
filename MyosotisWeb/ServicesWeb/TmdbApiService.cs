using Microsoft.AspNetCore.Http.HttpResults;
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

        private List<Media> MapTmdbResultsToMedia(IEnumerable<TmdbItemDto> results, string? fallbackMediaType = null)
        {
            var list = new List<Media>();
            foreach (var item in results)
            {
                int releaseYear = ParseYear(item.ReleaseDate ?? item.FirstAirDate);
                string title = item.Title ?? item.Name ?? "Untitled";
                string mediaType = item.MediaType ?? fallbackMediaType ?? "movie";

                if (mediaType == "movie")
                {
                    var movie = new Movie(title, releaseYear, 1, null);
                    PopulateBasicMedia(movie, item);
                    list.Add(movie);
                }
                else if (mediaType == "tv")
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

        public async Task<PagedMediaResult> SearchMediaPagedAsync(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query)) return new PagedMediaResult();

            try
            {
                await EnsureGenresLoadedAsync();
                var client = _httpClientFactory.CreateClient("TmdbClient");
                var encodedQuery = Uri.EscapeDataString(query);

                var multiTask = client.GetFromJsonAsync<TmdbSearchResponse>($"search/multi?query={encodedQuery}&include_adult=false&language=en-US&page={page}");
                var movieTask = client.GetFromJsonAsync<TmdbSearchResponse>($"search/movie?query={encodedQuery}&include_adult=false&language=en-US&page=1");
                var tvTask = client.GetFromJsonAsync<TmdbSearchResponse>($"search/tv?query={encodedQuery}&include_adult=false&language=en-US&page=1");
                var personTask = client.GetFromJsonAsync<TmdbPersonSearchResponse>($"search/person?query={encodedQuery}&include_adult=false&language=en-US&page={page}");

                await Task.WhenAll(multiTask, movieTask, tvTask, personTask);

                var response = await multiTask;
                var movieResp = await movieTask;
                var tvResp = await tvTask;
                var personResp = await personTask;

                return new PagedMediaResult
                {
                    Results = response?.Results != null ? MapTmdbResultsToMedia(response.Results) : new List<Media>(),
                    People = personResp?.Results ?? new List<TmdbPersonSearchDto>(),
                    CurrentPage = response?.Page ?? 1,
                    TotalPages = response?.TotalPages ?? 1,
                    TotalResults = response?.TotalResults ?? 0,
                    TotalMovies = movieResp?.TotalResults ?? 0,
                    TotalSeries = tvResp?.TotalResults ?? 0,
                    TotalPeople = personResp?.TotalResults ?? 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Multi Search Error]: {ex.Message}");
                return new PagedMediaResult();
            }
        }

        public async Task<PagedMediaResult> SearchMoviesPagedAsync(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query)) return new PagedMediaResult();

            try
            {
                await EnsureGenresLoadedAsync();
                var client = _httpClientFactory.CreateClient("TmdbClient");
                var encodedQuery = Uri.EscapeDataString(query);

                var movieTask = client.GetFromJsonAsync<TmdbSearchResponse>($"search/movie?query={encodedQuery}&include_adult=false&language=en-US&page={page}");
                var tvTask = client.GetFromJsonAsync<TmdbSearchResponse>($"search/tv?query={encodedQuery}&include_adult=false&language=en-US&page=1");
                var personTask = client.GetFromJsonAsync<TmdbPersonSearchResponse>($"search/person?query={encodedQuery}&include_adult=false&language=en-US&page=1");

                await Task.WhenAll(movieTask, tvTask, personTask);

                var movieResp = await movieTask;
                var tvResp = await tvTask;
                var personResp = await personTask;

                return new PagedMediaResult
                {
                    Results = movieResp?.Results != null ? MapTmdbResultsToMedia(movieResp.Results, "movie") : new List<Media>(),
                    People = new List<TmdbPersonSearchDto>(),
                    CurrentPage = movieResp?.Page ?? 1,
                    TotalPages = movieResp?.TotalPages ?? 1,
                    TotalResults = movieResp?.TotalResults ?? 0,
                    TotalMovies = movieResp?.TotalResults ?? 0,
                    TotalSeries = tvResp?.TotalResults ?? 0,
                    TotalPeople = personResp?.TotalResults ?? 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Movies Search Error]: {ex.Message}");
                return new PagedMediaResult();
            }
        }

        public async Task<PagedMediaResult> SearchTvPagedAsync(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query)) return new PagedMediaResult();

            try
            {
                await EnsureGenresLoadedAsync();
                var client = _httpClientFactory.CreateClient("TmdbClient");
                var encodedQuery = Uri.EscapeDataString(query);

                var tvTask = client.GetFromJsonAsync<TmdbSearchResponse>($"search/tv?query={encodedQuery}&include_adult=false&language=en-US&page={page}");
                var movieTask = client.GetFromJsonAsync<TmdbSearchResponse>($"search/movie?query={encodedQuery}&include_adult=false&language=en-US&page=1");
                var personTask = client.GetFromJsonAsync<TmdbPersonSearchResponse>($"search/person?query={encodedQuery}&include_adult=false&language=en-US&page=1");

                await Task.WhenAll(tvTask, movieTask, personTask);

                var tvResp = await tvTask;
                var movieResp = await movieTask;
                var personResp = await personTask;

                return new PagedMediaResult
                {
                    Results = tvResp?.Results != null ? MapTmdbResultsToMedia(tvResp.Results, "tv") : new List<Media>(),
                    People = new List<TmdbPersonSearchDto>(),
                    CurrentPage = tvResp?.Page ?? 1,
                    TotalPages = tvResp?.TotalPages ?? 1,
                    TotalResults = tvResp?.TotalResults ?? 0,
                    TotalMovies = movieResp?.TotalResults ?? 0,
                    TotalSeries = tvResp?.TotalResults ?? 0,
                    TotalPeople = personResp?.TotalResults ?? 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB TV Search Error]: {ex.Message}");
                return new PagedMediaResult();
            }
        }

        public async Task<PagedMediaResult> SearchPeoplePagedAsync(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query)) return new PagedMediaResult();

            try
            {
                await EnsureGenresLoadedAsync();
                var client = _httpClientFactory.CreateClient("TmdbClient");
                var encodedQuery = Uri.EscapeDataString(query);

                var personTask = client.GetFromJsonAsync<TmdbPersonSearchResponse>($"search/person?query={encodedQuery}&include_adult=false&language=en-US&page={page}");
                var movieTask = client.GetFromJsonAsync<TmdbSearchResponse>($"search/movie?query={encodedQuery}&include_adult=false&language=en-US&page=1");
                var tvTask = client.GetFromJsonAsync<TmdbSearchResponse>($"search/tv?query={encodedQuery}&include_adult=false&language=en-US&page=1");

                await Task.WhenAll(personTask, movieTask, tvTask);

                var personResp = await personTask;
                var movieResp = await movieTask;
                var tvResp = await tvTask;

                return new PagedMediaResult
                {
                    Results = new List<Media>(),
                    People = personResp?.Results ?? new List<TmdbPersonSearchDto>(),
                    CurrentPage = personResp?.Page ?? 1,
                    TotalPages = personResp?.TotalPages ?? 1,
                    TotalResults = personResp?.TotalResults ?? 0,
                    TotalMovies = movieResp?.TotalResults ?? 0,
                    TotalSeries = tvResp?.TotalResults ?? 0,
                    TotalPeople = personResp?.TotalResults ?? 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Person Search Error]: {ex.Message}");
                return new PagedMediaResult();
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

                int? runtime = dto.Runtime > 0 ? dto.Runtime : null;
                var movie = new Movie(title, releaseYear, runtime, null);
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
                int? avgRuntime = dto.EpisodeRunTime?
                .Where(runtime => runtime > 0)
                .Select(runtime => (int?)runtime)
                .FirstOrDefault();

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
                        int? avgLength = response.EpisodeRunTime?
                            .FirstOrDefault(runtime => runtime > 0);

                        if (avgLength.HasValue)
                            seriesObj.UpdateRuntimeInfo(avgLength.Value);

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
                            foreach (var c in response.Credits.Cast.Take(24))
                            {
                                string? photoUrl = !string.IsNullOrEmpty(c.ProfilePath)
                                    ? $"https://image.tmdb.org/t/p/w185{c.ProfilePath}"
                                    : null;

                                media.AddCastMember(new Actor(
                                    fullName: c.Name,
                                    roleName: string.IsNullOrEmpty(c.Character) ? "Unknown" : c.Character,
                                    tmdbId: c.Id,
                                    photoUrl: photoUrl
                                ));
                            }
                        }

                        media.Directors.Clear();
                        var directorsList = new List<Director>();

                        if (response.Credits?.Crew != null)
                        {
                            var keyCrew = response.Credits.Crew.Where(c =>
                                c.Job == "Director" ||
                                c.Job == "Series Director" ||
                                c.Job == "Showrunner" ||
                                c.Job == "Screenplay" ||
                                c.Job == "Writer" ||
                                c.Job == "Producer" ||
                                c.Job == "Executive Producer" ||
                                c.Job == "Director of Photography" ||
                                c.Job == "Original Music Composer" ||
                                c.Department == "Directing" ||
                                c.Department == "Writing" ||
                                c.Department == "Production"
                            ).Take(24); 
                            foreach (var person in keyCrew)
                            {
                                string? photoUrl = !string.IsNullOrEmpty(person.ProfilePath)
                                    ? $"https://image.tmdb.org/t/p/w185{person.ProfilePath}"
                                    : null;

                                string jobRole = !string.IsNullOrEmpty(person.Job) ? person.Job : person.Department;

                                directorsList.Add(new Director(
                                    fullName: person.Name,
                                    signatureStyle: jobRole,
                                    tmdbId: person.Id,
                                    photoUrl: photoUrl
                                ));
                            }
                        }

                        if (response.CreatedBy != null)
                        {
                            foreach (var creator in response.CreatedBy)
                            {
                                string? photoUrl = !string.IsNullOrEmpty(creator.ProfilePath)
                                    ? $"https://image.tmdb.org/t/p/w185{creator.ProfilePath}"
                                    : null;

                                directorsList.Add(new Director(
                                    fullName: creator.Name,
                                    signatureStyle: "Creator",
                                    tmdbId: creator.Id,
                                    photoUrl: photoUrl
                                ));
                            }
                        }

                        foreach (var dir in directorsList.DistinctBy(d => d.FullName))
                        {
                            media.AddDirector(dir);
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

                await FetchAndApplyKeywordsAsync(media);
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
                            int runtime = epDto.Runtime ?? series.AverageEpisodeLength ?? 45;
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

        public async Task<TmdbPersonDetailsDto?> GetPersonDetailsAsync(int personId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TmdbClient");
                return await client.GetFromJsonAsync<TmdbPersonDetailsDto>($"person/{personId}?language=en-US");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Person Details Error]: {ex.Message}");
                return null;
            }
        }

        public async Task<TmdbCombinedCreditsDto?> GetPersonCombinedCreditsAsync(int personId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TmdbClient");
                return await client.GetFromJsonAsync<TmdbCombinedCreditsDto>($"person/{personId}/combined_credits?language=en-US");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Person Credits Error]: {ex.Message}");
                return null;
            }
        }

        public async Task FetchAndApplyKeywordsAsync(Media media)
        {
            if (!media.TmdbId.HasValue) return;

            try
            {
                var client = _httpClientFactory.CreateClient("TmdbClient");

                if (media is Movie)
                {
                    var response = await client.GetFromJsonAsync<TmdbMovieKeywordsResponseDto>($"movie/{media.TmdbId}/keywords");
                    if (response?.Keywords != null)
                    {
                        foreach (var kw in response.Keywords)
                        {
                            if (!string.IsNullOrWhiteSpace(kw.Name))
                            {
                                media.AddTag(kw.Name);
                            }
                        }
                    }
                }
                else if (media is Series)
                {
                    var response = await client.GetFromJsonAsync<TmdbTvKeywordsResponseDto>($"tv/{media.TmdbId}/keywords");
                    if (response?.Results != null)
                    {
                        foreach (var kw in response.Results)
                        {
                            if (!string.IsNullOrWhiteSpace(kw.Name))
                            {
                                media.AddTag(kw.Name);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Keywords Fetch Error]: {ex.Message}");
            }
        }

    }

    public class TmdbGenreListResponseDto { [JsonPropertyName("genres")] public List<TmdbGenreDto>? Genres { get; set; } }
    public class TmdbReviewsResponseDto { [JsonPropertyName("results")] public List<TmdbReviewItemDto>? Results { get; set; } }

    public class TmdbSearchResponse
    {
        [JsonPropertyName("page")] public int Page { get; set; }
        [JsonPropertyName("results")] public List<TmdbItemDto>? Results { get; set; }
        [JsonPropertyName("total_pages")] public int TotalPages { get; set; }
        [JsonPropertyName("total_results")] public int TotalResults { get; set; }
    }

    public class TmdbPersonSearchResponse
    {
        [JsonPropertyName("page")] public int Page { get; set; }
        [JsonPropertyName("results")] public List<TmdbPersonSearchDto>? Results { get; set; }
        [JsonPropertyName("total_pages")] public int TotalPages { get; set; }
        [JsonPropertyName("total_results")] public int TotalResults { get; set; }
    }

    public class TmdbPersonSearchDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("profile_path")] public string? ProfilePath { get; set; }
        [JsonPropertyName("known_for_department")] public string? KnownForDepartment { get; set; }
        [JsonPropertyName("known_for")] public List<TmdbItemDto>? KnownFor { get; set; }

        public string ProfileUrl => !string.IsNullOrEmpty(ProfilePath)
            ? $"https://image.tmdb.org/t/p/w500{ProfilePath}"
            : string.Empty;

        public string KnownForTitles
        {
            get
            {
                if (KnownFor == null || !KnownFor.Any()) return string.Empty;
                var titles = KnownFor.Select(k => k.Title ?? k.Name).Where(t => !string.IsNullOrWhiteSpace(t)).Take(3);
                return string.Join(", ", titles);
            }
        }
    }

    public class PagedMediaResult
    {
        public List<Media> Results { get; set; } = new();
        public List<TmdbPersonSearchDto> People { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalResults { get; set; } = 0;
        public int TotalMovies { get; set; } = 0;
        public int TotalSeries { get; set; } = 0;
        public int TotalPeople { get; set; } = 0;
    }

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
        [JsonPropertyName("runtime")] public int? Runtime { get; set; }
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
        [JsonPropertyName("created_by")] public List<TmdbCreatorDto>? CreatedBy { get; set; }
    }

    public class TmdbCreatorDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("profile_path")] public string? ProfilePath { get; set; }
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
        [JsonPropertyName("created_by")] public List<TmdbCreatorDto>? CreatedBy { get; set; }
    }

    public class TmdbCredits
    {
        [JsonPropertyName("cast")] public List<TmdbCast>? Cast { get; set; }
        [JsonPropertyName("crew")] public List<TmdbCrew>? Crew { get; set; }
    }

    public class TmdbCast
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("character")] public string Character { get; set; } = "";
        [JsonPropertyName("profile_path")] public string? ProfilePath { get; set; }
    }

    public class TmdbCrew
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("job")] public string Job { get; set; } = "";
        [JsonPropertyName("department")] public string Department { get; set; } = "";
        [JsonPropertyName("profile_path")] public string? ProfilePath { get; set; }
    }

    public class TmdbPersonDetailsDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("biography")] public string? Biography { get; set; }
        [JsonPropertyName("birthday")] public string? Birthday { get; set; }
        [JsonPropertyName("deathday")] public string? Deathday { get; set; }
        [JsonPropertyName("gender")] public int Gender { get; set; }
        [JsonPropertyName("place_of_birth")] public string? PlaceOfBirth { get; set; }
        [JsonPropertyName("profile_path")] public string? ProfilePath { get; set; }
        [JsonPropertyName("known_for_department")] public string? KnownForDepartment { get; set; }
        [JsonPropertyName("imdb_id")] public string? ImdbId { get; set; }

        public string ProfileUrl => !string.IsNullOrEmpty(ProfilePath)
            ? $"https://image.tmdb.org/t/p/w500{ProfilePath}"
            : string.Empty;
    }

    public class TmdbCombinedCreditsDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("cast")] public List<TmdbCreditItemDto> Cast { get; set; } = new();
        [JsonPropertyName("crew")] public List<TmdbCreditItemDto> Crew { get; set; } = new();
    }

    public class TmdbCreditItemDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("media_type")] public string MediaType { get; set; } = "movie";
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("character")] public string? Character { get; set; }
        [JsonPropertyName("job")] public string? Job { get; set; }
        [JsonPropertyName("department")] public string? Department { get; set; }
        [JsonPropertyName("poster_path")] public string? PosterPath { get; set; }
        [JsonPropertyName("release_date")] public string? ReleaseDate { get; set; }
        [JsonPropertyName("first_air_date")] public string? FirstAirDate { get; set; }
        [JsonPropertyName("vote_average")] public double? VoteAverage { get; set; }

        public string DisplayTitle => !string.IsNullOrEmpty(Title) ? Title : (Name ?? "Untitled");

        public string? DisplayYear
        {
            get
            {
                var date = !string.IsNullOrEmpty(ReleaseDate) ? ReleaseDate : FirstAirDate;
                return !string.IsNullOrEmpty(date) && date.Length >= 4 ? date.Substring(0, 4) : null;
            }
        }

        public string PosterUrl => !string.IsNullOrEmpty(PosterPath)
            ? $"https://image.tmdb.org/t/p/w342{PosterPath}"
            : string.Empty;

        public string IconName => MediaType == "tv" ? "live_tv" : "movie";
    }

    public class TmdbMovieKeywordsResponseDto
    {
        [JsonPropertyName("keywords")]
        public List<TmdbKeywordItemDto>? Keywords { get; set; }
    }

    public class TmdbTvKeywordsResponseDto
    {
        [JsonPropertyName("results")]
        public List<TmdbKeywordItemDto>? Results { get; set; }
    }

    public class TmdbKeywordItemDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}