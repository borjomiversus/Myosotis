using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace MyosotisWeb.ServicesWeb
{
    public class TmdbApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string ImageBaseUrl = "https://image.tmdb.org/t/p/w500";

        public TmdbApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<Media>> SearchMediaAsync(string query)
        {
            var list = new List<Media>();
            if (string.IsNullOrWhiteSpace(query)) return list;

            try
            {
                var client = _httpClientFactory.CreateClient("TmdbClient");
                var response = await client.GetFromJsonAsync<TmdbSearchResponse>(
                    $"search/multi?query={Uri.EscapeDataString(query)}&include_adult=false&language=en-US&page=1");

                if (response?.Results == null) return list;

                foreach (var item in response.Results)
                {
                    int releaseYear = ParseYear(item.ReleaseDate ?? item.FirstAirDate);
                    string title = item.Title ?? item.Name ?? "Untitled";

                    if (item.MediaType == "movie")
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Search Error]: {ex.Message}");
            }

            return list;
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

        // допоміжне

        private void PopulateBasicMedia(Media media, TmdbItemDto item)
        {
            if (item.Id > 0) media.SetTmdbId(item.Id);
            if (!string.IsNullOrEmpty(item.Overview)) media.UpdateDescription(item.Overview);
            if (item.VoteAverage > 0) media.SetTmdbRating(item.VoteAverage);
            if (!string.IsNullOrEmpty(item.PosterPath)) media.SetPosterUrl($"{ImageBaseUrl}{item.PosterPath}");
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

        public async Task<List<Media>> GetTrendingMediaAsync()
        {
            var list = new List<Media>();

            try
            {
                var client = _httpClientFactory.CreateClient("TmdbClient");
                var response = await client.GetFromJsonAsync<TmdbSearchResponse>("trending/all/week?language=en-US");

                if (response?.Results == null) return list;

                foreach (var item in response.Results)
                {
                    int releaseYear = ParseYear(item.ReleaseDate ?? item.FirstAirDate);
                    string title = item.Title ?? item.Name ?? "Untitled";

                    if (item.MediaType == "movie")
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Trending Error]: {ex.Message}");
            }

            return list;
        }

        public async Task EnrichMediaDetailsAsync(Media media)
        {
            if (!media.TmdbId.HasValue) return;

            try
            {
                var client = _httpClientFactory.CreateClient("TmdbClient");
                string endpoint = media is Movie
                    ? $"movie/{media.TmdbId}?append_to_response=credits"
                    : $"tv/{media.TmdbId}?append_to_response=credits";

                var response = await client.GetFromJsonAsync<TmdbFullDetailsDto>(endpoint);
                if (response == null) return;

                if (media is Movie && response.Runtime.HasValue)
                {
                    media.UpdateRuntimeInfo(response.Runtime.Value);
                }
                else if (media is Series seriesObj)
                {
                    int rawLength = response.EpisodeRunTime?.FirstOrDefault() ?? 0;
                    int avgLength = rawLength > 0 ? rawLength : 45;

                    int seasonsCount = response.NumberOfSeasons ?? 1;
                    int totalEpisodes = response.NumberOfEpisodes ?? 1;

                    seriesObj.UpdateRuntimeInfo(avgLength);

                    if (seriesObj.Seasons == null || seriesObj.Seasons.Count == 0)
                    {
                        int episodesPerSeason = seasonsCount > 0 ? Math.Max(1, totalEpisodes / seasonsCount) : totalEpisodes;

                        for (int s = 1; s <= seasonsCount; s++)
                        {
                            var season = new Season(s);
                            for (int e = 1; e <= episodesPerSeason; e++)
                            {
                                season.AddEpisode(new Episode(e, $"Episode {e}", avgLength));
                            }
                            seriesObj.AddSeason(season);
                        }
                    }
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
            catch (Exception ex)
            {
                Console.WriteLine($"[TMDB Details Error]: {ex.Message}");
            }
        }
    }

    

    public class TmdbSearchResponse
    {
        [JsonPropertyName("results")]
        public List<TmdbItemDto>? Results { get; set; }
    }

    public class TmdbItemDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("media_type")]
        public string? MediaType { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("first_air_date")]
        public string? FirstAirDate { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [JsonPropertyName("runtime")] 
        public int? Runtime { get; set; }

        [JsonPropertyName("episode_run_time")]
        public List<int>? EpisodeRunTime { get; set; }
   
}

    public class TmdbMovieDetailsDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [JsonPropertyName("runtime")]
        public int Runtime { get; set; }

        [JsonPropertyName("adult")]
        public bool Adult { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("genres")]
        public List<TmdbGenreDto>? Genres { get; set; }

        [JsonPropertyName("production_companies")]
        public List<TmdbCompanyDto>? ProductionCompanies { get; set; }

        [JsonPropertyName("production_countries")]
        public List<TmdbCountryDto>? ProductionCountries { get; set; }
    }

    public class TmdbSeriesDetailsDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("first_air_date")]
        public string? FirstAirDate { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [JsonPropertyName("episode_run_time")]
        public List<int>? EpisodeRunTime { get; set; }

        [JsonPropertyName("number_of_seasons")]
        public int NumberOfSeasons { get; set; }

        [JsonPropertyName("number_of_episodes")]
        public int NumberOfEpisodes { get; set; }

        [JsonPropertyName("adult")]
        public bool Adult { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("genres")]
        public List<TmdbGenreDto>? Genres { get; set; }

        [JsonPropertyName("production_companies")]
        public List<TmdbCompanyDto>? ProductionCompanies { get; set; }

        [JsonPropertyName("production_countries")]
        public List<TmdbCountryDto>? ProductionCountries { get; set; }
    }

    public class TmdbGenreDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class TmdbCompanyDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class TmdbCountryDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class TmdbReviewsResponseDto
    {
        [JsonPropertyName("results")]
        public List<TmdbReviewDto>? Results { get; set; }
    }

    public class TmdbReviewDto
    {
        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    public class TmdbFullDetailsDto
    {
        [JsonPropertyName("runtime")] public int? Runtime { get; set; }
        [JsonPropertyName("episode_run_time")] public List<int>? EpisodeRunTime { get; set; }
        [JsonPropertyName("number_of_seasons")] public int? NumberOfSeasons { get; set; }
        [JsonPropertyName("number_of_episodes")] public int? NumberOfEpisodes { get; set; }
        [JsonPropertyName("production_countries")] public List<TmdbCountry>? ProductionCountries { get; set; }
        [JsonPropertyName("production_companies")] public List<TmdbCompany>? ProductionCompanies { get; set; }
        [JsonPropertyName("credits")] public TmdbCredits? Credits { get; set; }
    }

    public class TmdbCountry { [JsonPropertyName("name")] public string Name { get; set; } = ""; }
    public class TmdbCompany { [JsonPropertyName("name")] public string Name { get; set; } = ""; }
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