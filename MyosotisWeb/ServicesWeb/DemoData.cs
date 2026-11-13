using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyosotisWeb.Infrastructure;

namespace MyosotisWeb.ServicesWeb
{
    public class DemoDataService
    {
        private readonly AnonymousUserDataService _storageService;
        private bool _isInitialized = false;

        public User CurrentUser { get; private set; }
        public WatchCheckpoint? ActiveCheckpoint { get; private set; }
        public List<Media> Library { get; private set; } = new();

        public event Action? OnStateChanged;

        public DemoDataService(AnonymousUserDataService storageService)
        {
            _storageService = storageService;
            CurrentUser = new User("Guest"); 
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            CurrentUser = await _storageService.GetUserDataAsync();

            if (!CurrentUser.Watchlists.Any())
            {
                CurrentUser.CreateWatchlist("Romantic", true);
                CurrentUser.CreateWatchlist("Horror", true);
                await SaveStateAsync();
            }

            RebuildLibraryFromUser();

            _isInitialized = true;
            OnStateChanged?.Invoke();
        }

        public async Task SaveStateAsync()
        {
            await _storageService.SaveUserDataAsync(CurrentUser);
            OnStateChanged?.Invoke();
        }

        private void RebuildLibraryFromUser()
        {
            var savedMedia = CurrentUser.Watchlists
                .SelectMany(w => w.Entries.Select(e => e.Item))
                .Concat(CurrentUser.WatchHistory.Select(h => h.WatchedItem))
                .Concat(CurrentUser.MediaStates.Select(s => s.Item))
                .Concat(CurrentUser.RecentlyViewed)
                .Concat(CurrentUser.Captures
                    .Where(c => c.ResolvedMedia != null)
                    .Select(c => c.ResolvedMedia!))
                .Concat(CurrentUser.Checkpoints.Select(c => c.PausedMedia))
                .Where(m => m != null)
                .DistinctBy(m => m.Id);

            Library.Clear();
            Library.AddRange(savedMedia);
        }

        public List<Media> GetAllMedia() => Library;

        public Media? GetById(Guid id) => Library.FirstOrDefault(m => m.Id == id);

        public IEnumerable<Media> SearchLibrary(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<Media>();
            var q = query.ToLowerInvariant();
            return Library.Where(m => m.Title.ToLowerInvariant().Contains(q) ||
                                     (m.Genres != null && m.Genres.Any(g => g.ToLowerInvariant().Contains(q))));
        }

        public Media GetOrAddToLibrary(Media media)
        {
            var existing = Library.FirstOrDefault(m =>
                (m.TmdbId.HasValue && media.TmdbId.HasValue && m.TmdbId == media.TmdbId) ||
                (m.Title.Equals(media.Title, StringComparison.OrdinalIgnoreCase) && m.ReleaseYear == media.ReleaseYear));

            if (existing != null) return existing;

            Library.Add(media);
            return media;
        }
    }
}