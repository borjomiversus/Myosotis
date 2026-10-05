using System.Collections.Generic;

public class MediaSearch
{
    public List<Media> SearchByTitle(List<Media> library, string title)
    {
        List<Media> results = new List<Media>();
        foreach (var m in library)
        {
            if (m.Title.ToLower().Contains(title.ToLower()))
                results.Add(m);
        }
        return results;
    }

    public List<Media> FilterByGenre(List<Media> library, string genre)
    {
        List<Media> results = new List<Media>();
        foreach (var m in library)
        {
            if (m.Genres.Exists(g => g.ToLower().Contains(genre.ToLower())))
                results.Add(m);
        }
        return results;
    }

    public List<Media> AdvancedSearch(List<Media> library, string genre, int minYear, int maxYear)
    {
        List<Media> results = new List<Media>();
        foreach (var m in library)
        {
            if (m.Genres.Exists(g => g.ToLower().Contains(genre.ToLower())) &&
                m.ReleaseYear >= minYear && m.ReleaseYear <= maxYear)
            {
                results.Add(m);
            }
        }
        return results;
    }

    public List<Media> SearchByActor(List<Media> library, string actorName)
    {
        List<Media> results = new List<Media>();
        foreach (var m in library)
        {
            foreach (var actor in m.Cast)
            {
                if (actor.FullName.ToLower().Contains(actorName.ToLower()))
                {
                    results.Add(m);
                    break;
                }
            }
        }
        return results;
    }
}