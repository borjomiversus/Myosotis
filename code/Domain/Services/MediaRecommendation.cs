using System;
using System.Collections.Generic;

public class MediaRecommendation
{
    public List<Media> GetTopRated(List<Media> library, int count)
    {
        List<Media> sorted = new List<Media>(library);
        sorted.Sort((a, b) => {
            double rateA = a.Vibe != null ? a.Vibe.CalculateAverage() : 0;
            double rateB = b.Vibe != null ? b.Vibe.CalculateAverage() : 0;
            return rateB.CompareTo(rateA);
        });

        List<Media> result = new List<Media>();
        for (int i = 0; i < sorted.Count && i < count; i++)
            result.Add(sorted[i]);

        return result;
    }

    public List<Media> FindSimilar(List<Media> library, Media reference, int count)
    {
        if (reference == null || library == null)
            return new List<Media>();

        List<Media> similar = new List<Media>();
        for (int i = 0; i < library.Count; i++)
        {
            if (library[i].Id != reference.Id)
            {
                similar.Add(library[i]);
            }
        }

        similar.Sort((a, b) => {

            int sharedGenresA = a.CountSharedGenres(reference);
            int sharedGenresB = b.CountSharedGenres(reference);
            int genreComparison = sharedGenresB.CompareTo(sharedGenresA);
            if (genreComparison != 0) return genreComparison;

            bool isSameTypeA = a.GetType() == reference.GetType();
            bool isSameTypeB = b.GetType() == reference.GetType();
            if (isSameTypeA != isSameTypeB)
            {
                return isSameTypeA.CompareTo(isSameTypeB) * -1; 
            }

            bool sameStudioA = !string.IsNullOrEmpty(a.ProductionStudio) && a.ProductionStudio == reference.ProductionStudio;
            bool sameStudioB = !string.IsNullOrEmpty(b.ProductionStudio) && b.ProductionStudio == reference.ProductionStudio;
            if (sameStudioA != sameStudioB)
            {
                return sameStudioA.CompareTo(sameStudioB) * -1;
            }

            double ratingA = a.TmdbRating ?? 0;
            double ratingB = b.TmdbRating ?? 0;
            return ratingB.CompareTo(ratingA);
        });

        List<Media> result = new List<Media>();
        for (int i = 0; i < Math.Min(count, similar.Count); i++)
        {
            result.Add(similar[i]);
        }

        return result;
    }

    public Media? ChooseByRoulette(List<Media> library, RouletteCriteria criteria)
    {
        var filtered = new List<Media>();

        for (int i = 0; i < library.Count; i++)
        {
            var media = library[i];

            int? duration = null;
            if (media is Movie movie)
            {
                duration = movie.DurationMinutes;
            }
            else if (media is Series series)
            {
                duration = series.AverageEpisodeLength;
            }
            else
            {
                duration = media.CalculateTimeDebt();
            }

            if (!duration.HasValue)
                continue;

            if (duration.Value > criteria.MaxMinutes)
                continue;

            if (!string.IsNullOrWhiteSpace(criteria.Tag))
            {
                string selectedTag = criteria.Tag.Trim();

                bool matchesGenre = false;
                if (media.Genres != null)
                {
                    for (int g = 0; g < media.Genres.Count; g++)
                    {
                        if (string.Equals(media.Genres[g].Trim(), selectedTag, StringComparison.OrdinalIgnoreCase))
                        {
                            matchesGenre = true;
                            break;
                        }
                    }
                }

                bool matchesTag = false;
                if (media.Tags != null)
                {
                    for (int t = 0; t < media.Tags.Count; t++)
                    {
                        if (string.Equals(media.Tags[t].Trim(), selectedTag, StringComparison.OrdinalIgnoreCase))
                        {
                            matchesTag = true;
                            break;
                        }
                    }
                }

                if (!matchesGenre && !matchesTag)
                    continue;
            }

            filtered.Add(media);
        }

        if (filtered.Count == 0)
            return null;

        return filtered[Random.Shared.Next(filtered.Count)];
    }
}