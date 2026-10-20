using System;
using System.Collections.Generic;
using System.Linq;

namespace MyosotisWeb.ServicesWeb
{
    public class DemoDataService
    {
        public User CurrentUser { get; private set; }
        public WatchCheckpoint ActiveCheckpoint { get; private set; }

        // Бібліотека всіх фільмів 
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

            var gilligan = new Director("Vince Gilligan", 1967, "Character Drama");
            var fletcher = new Director("Anne Fletcher", 1966, "Rom-Com");
            var karWai = new Director("Wong Kar-wai", 1958, "Visual Poetry");
            var tarantino = new Director("Quentin Tarantino", 1963, "Stylized Violence");
            var rogen = new Director("Seth Rogen", 1982, "Satirical Sci-Fi");
            var chanWook = new Director("Park Chan-wook", 1963, "Revenge Thrillers");

            var breakingBad = new Series("Breaking Bad", 2008, 47, 1, gilligan);
            breakingBad.UpdateDescription("Вчитель хімії дізнається про свій діагноз і вирішує забезпечити сім'ю, виготовляючи метамфетамін.");
            breakingBad.AddGenre("Crime");
            breakingBad.AddGenre("Drama");
            breakingBad.AddGenre("Thriller");
            breakingBad.SetPosterUrl("/images/posters/breaking-bad.jpg");
            breakingBad.SetVibeRating(new VibeRating(10, 9, 8, 9, 10, 7));
            var season1 = new Season(1);

            season1.AddEpisode(new Episode(1, "Pilot", 58));
            breakingBad.AddSeason(season1);

            var dresses = new Movie("27 Dresses", 2008, 111, fletcher);
            dresses.UpdateDescription("Джейн завжди була подружкою нареченої, але ніколи — головною героїнею. Все змінюється, коли її сестра збирається заміж за чоловіка, якого кохає Джейн.");
            dresses.AddGenre("Comedy");
            dresses.AddGenre("Romance");
            dresses.SetPosterUrl("/images/posters/27-dresses.jpg");
            var awesomeVibe = new VibeRating(8, 7, 10, 9, 10, 10);
            dresses.SetVibeRating(awesomeVibe);

            var moodForLove = new Movie("In the Mood for Love", 2000, 98, karWai);
            moodForLove.UpdateDescription("Гонконг, 1960-ті. Двоє сусідів виявляють, що їхні подружжя зраджують їм одне з одним, і між ними виникає особливий, але стриманий зв'язок.");
            moodForLove.AddGenre("Romance");
            moodForLove.AddGenre("Drama");
            moodForLove.SetPosterUrl("/images/posters/mood-for-love.jpg");

            var killBill = new Movie("Kill Bill: Vol. 1", 2003, 111, tarantino);
            killBill.UpdateDescription("Колишня наймана вбивця прокидається з коми і вирушає мстити своєму колишньому босу Біллу та його банді.");
            killBill.AddGenre("Action");
            killBill.AddGenre("Thriller");
            killBill.SetPosterUrl("/images/posters/kill-bill.jpg");

            var futureMan = new Series("Future Man", 2017, 30, 13, rogen);
            futureMan.UpdateDescription("Прибиральник Джош проходить відеогру і раптово зустрічає прибульців з майбутнього, які кажуть йому врятувати світ.");
            futureMan.AddGenre("Comedy");
            futureMan.AddGenre("Sci-Fi");
            futureMan.SetPosterUrl("/images/posters/future-man.jpg");

            var oldboy = new Movie("Oldboy", 2003, 120, chanWook);
            oldboy.UpdateDescription("Чоловіка викрадають і тримають у кімнаті 15 років без пояснення причин. Звільнившись, він має 5 днів, щоб знайти свого викрадача.");
            oldboy.AddGenre("Action");
            oldboy.AddGenre("Drama");
            oldboy.AddGenre("Mystery");
            oldboy.SetPosterUrl("/images/posters/oldboy.jpg");

            Library.AddRange(new Media[] { breakingBad, dresses, moodForLove, killBill, futureMan, oldboy });

            var vibeList = CurrentUser.CreateWatchlist("Comfort Vibes", true);
            vibeList.AddEntry(breakingBad, "Можна передивлятись вічно");
            vibeList.AddEntry(moodForLove, "Ідеальна естетика");
            vibeList.AddEntry(dresses, "Для розвантаження мозку");

            var crazyList = CurrentUser.CreateWatchlist("Adrenaline", true);
            crazyList.AddEntry(killBill, "Класика Тарантіно");
            crazyList.AddEntry(oldboy, "Обов'язково додивитись");
            crazyList.AddEntry(futureMan, "Просто цікаво");

            // Історія
            CurrentUser.MarkAsWatched(moodForLove);
            CurrentUser.MarkAsWatched(dresses);

            CurrentUser.AddCapture("Той фільм корейський", "TikTok");
            CurrentUser.AddCapture("Романтика Вонг Карвая", "YouTube");

            // Чекпоінт перегляду
            ActiveCheckpoint = new WatchCheckpoint(futureMan, 1, 4, "12:45", "Треба було йти спати");
            CurrentUser.SetCheckpoint(ActiveCheckpoint);

            CurrentUser.MediaStates.Add(new UserMediaState(moodForLove, WatchStatus.Watched));
            CurrentUser.MediaStates.Add(new UserMediaState(dresses, WatchStatus.Watched));
            CurrentUser.MediaStates.Add(new UserMediaState(futureMan, WatchStatus.Watching));
        }
    }
}