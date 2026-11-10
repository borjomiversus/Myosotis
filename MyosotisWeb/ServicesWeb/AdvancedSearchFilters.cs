using System.Collections.Generic;
using System.Linq;

namespace MyosotisWeb.ServicesWeb
{
    public enum SearchMediaType
    {
        All,
        Movies,
        Series
    }

    public class SelectedKeyword
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public override bool Equals(object? obj) => obj is SelectedKeyword other && other.Id == Id;
        public override int GetHashCode() => Id.GetHashCode();
    }



    public class AdvancedSearchFilters
    {
        public SearchMediaType MediaType { get; set; } = SearchMediaType.All;

        public HashSet<int> SelectedGenreIds { get; set; } = new();

        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }

        public double? RatingFrom { get; set; }
        public double? RatingTo { get; set; }

        public List<SelectedKeyword> SelectedKeywords { get; set; } = new();

        public bool HasAnyFilter =>
            SelectedGenreIds.Count > 0 ||
            YearFrom.HasValue || YearTo.HasValue ||
            RatingFrom.HasValue || RatingTo.HasValue ||
            SelectedKeywords.Count > 0;

        public void Clear()
        {
            MediaType = SearchMediaType.All;
            SelectedGenreIds.Clear();
            YearFrom = null;
            YearTo = null;
            RatingFrom = null;
            RatingTo = null;
            SelectedKeywords.Clear();
        }

        public AdvancedSearchFilters Clone()
        {
            return new AdvancedSearchFilters
            {
                MediaType = MediaType,
                SelectedGenreIds = new HashSet<int>(SelectedGenreIds),
                YearFrom = YearFrom,
                YearTo = YearTo,
                RatingFrom = RatingFrom,
                RatingTo = RatingTo,
                SelectedKeywords = SelectedKeywords.ToList(),
            };
        }
    }
}