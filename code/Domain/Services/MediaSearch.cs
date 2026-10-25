using System.Collections.Generic;

public class MediaSearch
{
    //для Blazor, щоб знаходити медіа за  ID
    public Media? GetById(List<Media> library, Guid id)
    {
        return library.FirstOrDefault(m => m.Id == id);
    }

    public List<Media> SearchByTitle(List<Media> library, string title)
    {
        List<Media> results = new List<Media>();
        foreach (var m in library)
        {
            if (m.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                results.Add(m);
        }
        return results;
    }

    public List<Media> FilterByGenre(List<Media> library, string genre)
    {
        List<Media> results = new List<Media>();
        foreach (var m in library)
        {
            foreach (var g in m.Genres)
            {
                if (g.Contains(genre, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(m);
                    break;
                }
            }
        }
        return results;
    }

    public List<Media> AdvancedSearch(List<Media> library, string genre, int minYear, int maxYear)
    {
        List<Media> results = new List<Media>();
        foreach (var m in library)
        {
            if (m.ReleaseYear >= minYear && m.ReleaseYear <= maxYear)
                    {
                        foreach (var g in m.Genres)
                        {
                            if (g.Contains(genre, StringComparison.OrdinalIgnoreCase))
                            {
                                results.Add(m);
                            }
                        }
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
                if (actor.FullName.Contains(actorName, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(m);
                    break;
                }
            }
        }
        return results;
    }
}