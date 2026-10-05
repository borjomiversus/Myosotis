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
        List<Media> similar = new List<Media>();
        foreach (var m in library)
        {
            if (m != reference) similar.Add(m);
        }

        similar.Sort((a, b) => {
            int sharedA = a.CountSharedGenres(reference);
            int sharedB = b.CountSharedGenres(reference);
            return sharedB.CompareTo(sharedA);
        });

        List<Media> result = new List<Media>();
        for (int i = 0; i < Math.Min(count, similar.Count); i++)
            result.Add(similar[i]);

        return result;
    }

    public Media? ChooseByRoulette(List<Media> library, RouletteCriteria criteria)
    {
        List<Media> filtered = new List<Media>();
        foreach (var m in library)
        {
            if (m.CalculateTimeDebt() <= criteria.MaxMinutes)
            {
                if (string.IsNullOrEmpty(criteria.Tag))
                    filtered.Add(m);
                else if (m.Tags.Contains(criteria.Tag) || m.Genres.Contains(criteria.Tag))
                    filtered.Add(m);
            }
        }

        if (filtered.Count == 0) return null;

        Random random = new Random();
        return filtered[random.Next(filtered.Count)];
    }
}
