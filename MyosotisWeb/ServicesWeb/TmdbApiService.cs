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

                        var movie = new Movie(title, releaseYear, 120, null);

                        if (!string.IsNullOrEmpty(item.Overview)) movie.UpdateDescription(item.Overview);
                        if (item.VoteAverage > 0) movie.SetTmdbRating(item.VoteAverage);
                        if (!string.IsNullOrEmpty(item.PosterPath)) movie.SetPosterUrl($"{ImageBaseUrl}{item.PosterPath}");

                        list.Add(movie);
                    }
                    else if (item.MediaType == "tv")
                    {
                        var series = new Series(title, releaseYear, 45, 0, null);

                        if (!string.IsNullOrEmpty(item.Overview)) series.UpdateDescription(item.Overview);
                        if (item.VoteAverage > 0) series.SetTmdbRating(item.VoteAverage);
                        if (!string.IsNullOrEmpty(item.PosterPath)) series.SetPosterUrl($"{ImageBaseUrl}{item.PosterPath}");

                        list.Add(series);
                    }
                }
            }
            catch
            {
               
            }

            return list;
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

    public class TmdbSearchResponse
    {
        [JsonPropertyName("results")]
        public List<TmdbItemDto>? Results { get; set; }
    }

    public class TmdbItemDto
    {
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
    }
}