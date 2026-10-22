using System;
using System.Collections.Generic;
using System.Linq;

namespace MyosotisWeb.ServicesWeb
{
    public class DemoDataService
    {
        public User CurrentUser { get; private set; }
        public WatchCheckpoint? ActiveCheckpoint { get; private set; }

        // Локальна бібліотека
        public List<Media> Library { get; private set; } = new();

        public List<Media> GetAllMedia() => Library;

        public Media? GetById(Guid id)
        {
            return Library.FirstOrDefault(m => m.Id == id);
        }

        public IEnumerable<Media> SearchLibrary(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<Media>();

            var q = query.ToLowerInvariant();
            return Library.Where(m => m.Title.ToLowerInvariant().Contains(q) ||
                                     (m.Genres != null && m.Genres.Any(g => g.ToLowerInvariant().Contains(q))));
        }

        public DemoDataService()
        {
            CurrentUser = new User("spacedream");
            CurrentUser.CreateWatchlist("Comfort Vibes", true);
            CurrentUser.CreateWatchlist("Adrenaline", true);
        }

        // з TMDB у бібліотеку користувача
        public void AddToLibrary(Media media)
        {
            if (!Library.Any(m => m.Title.Equals(media.Title, StringComparison.OrdinalIgnoreCase)))
            {
                Library.Add(media);
            }
        }
    }
}