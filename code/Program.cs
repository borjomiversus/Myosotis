using System;
using System.Collections.Generic;

class Program
{

    static List<Media> library = new List<Media>();
    static MediaSearch searchService = new MediaSearch();
    static MediaRecommendation recommendationService = new MediaRecommendation();
    static CaptureProcessing captureService = new CaptureProcessing();
    static User me = new User("spacedream");
    static Watchlist myWatchlist = null!;
    static FranchiseTimeLine tarantinoVerse = null!;
    static WatchCheckpoint boysCheckpoint = null!;

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        SeedData();

        while (true)
        {
            Console.WriteLine("\n=== ГОЛОВНЕ МЕНЮ MYOSOTIS ===");
            Console.WriteLine("1. Медіатека (Пошук, фільтри, рулетка, топ)");
            Console.WriteLine("2. Профіль та Перегляд (Статуси, паузи, статистика, історія)");
            Console.WriteLine("3. Вхідні нотатки та Підбірки (Captures, Watchlists)");
            Console.WriteLine("4. Деталі тайтлу (Сезони, саундтреки, відгуки)");
            Console.WriteLine("5. Персоналії та Всесвіти (Актори, франшизи)");
            Console.WriteLine("0. Вийти");
            Console.Write("Оберіть розділ: ");

            string choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1": MenuLibrary(); break;
                case "2": MenuUserAndHistory(); break;
                case "3": MenuCapturesAndWatchlists(); break;
                case "4": MenuMediaDetails(); break;
                case "5": MenuPersonsAndUniverses(); break;
                case "0": return;
                default: Console.WriteLine("Невідома команда. Спробуйте ще раз."); break;
            }
        }
    }

    // --- ПІДМЕНЮ 1: МЕДІАТЕКА ---
    static void MenuLibrary()
    {
        Console.WriteLine("\n--- МЕДІАТЕКА ---");
        Console.WriteLine("1 - Всі тайтли, 2 - Пошук (Назва/Актор), 3 - Фільтри, 4 - Топ рейтингу, 5 - Схожі, 6 - Рулетка");
        Console.Write("Ваш вибір: ");
        string choice = Console.ReadLine() ?? "";

        if (choice == "1")
        {
            foreach (var item in library)
                Console.WriteLine($"- {item.GetFormattedSummary()} [Жанри: {string.Join(", ", item.Genres)}]");
        }
        else if (choice == "2")
        {
            Console.WriteLine("Шукати за: 1 - Назвою, 2 - Актором");
            string subChoice = Console.ReadLine() ?? "";
            Console.Write("Введіть запит: ");
            string query = Console.ReadLine() ?? "";

            me.LogSearch(query); 

            List<Media> found = new List<Media>();
            if (subChoice == "1") found = searchService.SearchByTitle(library, query);
            else if (subChoice == "2") found = searchService.SearchByActor(library, query);

            PrintList(found);
        }
        else if (choice == "3")
        {
            Console.Write("Введіть жанр (напр., Action): ");
            string genre = Console.ReadLine() ?? "";
            Console.Write("Мінімальний рік (від): ");
            int.TryParse(Console.ReadLine(), out int minYear);
            Console.Write("Максимальний рік (до): ");
            int.TryParse(Console.ReadLine(), out int maxYear);

            var found = searchService.AdvancedSearch(library, genre, minYear, maxYear);
            PrintList(found);
        }
        else if (choice == "4")
        {
            Console.Write("Скільки тайтлів показати у топі? ");
            int.TryParse(Console.ReadLine(), out int count);
            var top = recommendationService.GetTopRated(library, count);

            Console.WriteLine("\n--- Найвищий Vibe Rating ---");
            foreach (var item in top)
                Console.WriteLine($"- {item.Title}: {item.Vibe?.CalculateAverage():F1}/10");
        }
        else if (choice == "5")
        {
            Console.Write("Оберіть приклад (введіть точну назву): ");
            string targetTitle = Console.ReadLine() ?? "";
            var target = library.Find(m => m.Title.Equals(targetTitle, StringComparison.OrdinalIgnoreCase));

            if (target != null)
            {
                var similar = recommendationService.FindSimilar(library, target, 3);
                Console.WriteLine($"\nСхожі на '{target.Title}':");
                PrintList(similar);
            }
            else Console.WriteLine("Тайтл не знайдено.");
        }
        else if (choice == "6")
        {
            Console.Write("Максимальний час на перегляд (хв): ");
            if (!int.TryParse(Console.ReadLine(), out int maxMins)) maxMins = 1000;

            Console.Write("Бажаний жанр (або натисніть Enter): ");
            string tag = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(tag)) tag = null!;

            var criteria = new RouletteCriteria(maxMins, tag);
            var randomPick = recommendationService.ChooseByRoulette(library, criteria);

            if (randomPick != null)
                Console.WriteLine($"\n🎲 Випало: {randomPick.Title} ({randomPick.CalculateTimeDebt()} хв.)");
            else
                Console.WriteLine("\nНічого не знайдено.");
        }
    }

    static void MenuUserAndHistory()
    {
        Console.WriteLine("\n--- ПРОФІЛЬ ТА ПЕРЕГЛЯД ---");
        Console.WriteLine($"Користувач: {me.Username}");
        Console.WriteLine("1 - Статистика за місяць, 2 - Мої статуси перегляду, 3 - Чекпоінти (Паузи), 4 - Історія пошуку/переглядів, 5 - Перевірка віку");
        Console.Write("Ваш вибір: ");
        string choice = Console.ReadLine() ?? "";

        if (choice == "1")
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int minutes = me.GetMonthlyStats(year, month);
            Console.WriteLine($"\nЗа цей місяць ви подивилися {minutes} хвилин.");

            var genres = me.GetMonthlyGenreStats(year, month);
            foreach (var g in genres)
                Console.WriteLine($"- Жанр '{g.Key}': {g.Value} тайтл(ів)");
        }
        else if (choice == "2")
        {
            Console.WriteLine("\nПоточні статуси:");
            foreach (var state in me.MediaStates)
                Console.WriteLine($"- {state.Item.Title}: {state.Status} (Почато: {state.StartedAt})");
        }
        else if (choice == "3")
        {
            Console.WriteLine($"\n{boysCheckpoint.GetStatusDescription()}");
            Console.Write("Відновити перегляд? (т/н): ");
            if ((Console.ReadLine() ?? "").ToLower() == "т")
                Console.WriteLine(boysCheckpoint.ResumeWatching());
        }
        else if (choice == "4")
        {
            Console.WriteLine("\nОстанні пошукові запити (RecentSearches):");
            if (me.RecentSearches.Count == 0) Console.WriteLine("(порожньо)");
            foreach (var s in me.RecentSearches) Console.WriteLine($"- {s}");

            Console.WriteLine("\nНещодавно переглянуті сторінки (RecentlyViewed):");
            if (me.RecentlyViewed.Count == 0) Console.WriteLine("(порожньо)");
            foreach (var r in me.RecentlyViewed) Console.WriteLine($"- {r.Title}");
        }
        else if (choice == "5")
        {
            Console.Write("\nПеревірка доступу. Введіть ваш вік: ");
            if (int.TryParse(Console.ReadLine(), out int age))
            {
                var bill = library.Find(m => m.Title.Contains("Kill Bill"));
                if (bill != null)
                {
                    bool canWatch = bill.IsAgeAppropriate(age);
                    Console.WriteLine(canWatch
                        ? $"Вам {age}, доступ до '{bill.Title}' (18+) дозволено."
                        : $"Вам {age}, доступ до '{bill.Title}' (18+) заборонено!");
                }
            }
        }
    }

    static void MenuCapturesAndWatchlists()
    {
        Console.WriteLine("\n--- НОТАТКИ ТА ПІДБІРКИ ---");
        Console.WriteLine("1 - Створити нотатку (Capture), 2 - Перемістити нерозпізнані у Watchlist, 3 - Показати мої підбірки");
        Console.Write("Ваш вибір: ");
        string choice = Console.ReadLine() ?? "";

        if (choice == "1")
        {
            Console.Write("Нотатка: ");
            string note = Console.ReadLine() ?? "";
            me.AddCapture(note, "Console");
            Console.WriteLine("Збережено.");
        }
        else if (choice == "2")
        {
            var captures = me.GetUnresolvedCaptures();
            if (captures.Count == 0) Console.WriteLine("Немає нових нотаток.");

            foreach (var cap in captures)
            {
                Console.WriteLine($"\nОбробка: {cap.RawNote}");
                Console.Write("Введіть назву фільму для прив'язки (або Enter для пропуску): ");
                string query = Console.ReadLine() ?? "";
                if (string.IsNullOrWhiteSpace(query)) continue;

                var found = searchService.SearchByTitle(library, query);
                if (found.Count > 0)
                {
                    captureService.ResolveCapture(cap, found[0]);
                    me.MoveResolvedCaptureToWatchlist(cap, "Мої плани");
                    Console.WriteLine($"Прив'язано до {found[0].Title} і додано в 'Мої плани'.");
                }
            }
        }
        else if (choice == "3")
        {
            foreach (var w in me.Watchlists)
            {
                w.PrintContents();
            }
        }
    }


    static void MenuMediaDetails()
    {
        Console.Write("\nВведіть точну назву тайтлу для огляду: ");
        string targetTitle = Console.ReadLine() ?? "";
        var target = library.Find(m => m.Title.Equals(targetTitle, StringComparison.OrdinalIgnoreCase));

        if (target != null)
        {
            me.RecordView(target);

            Console.WriteLine($"\n=== {target.Title} ({target.ReleaseYear}) ===");
            Console.WriteLine($"Режисер: {(target.MediaDirector != null ? target.MediaDirector.FullName : "Невідомо")}");
            Console.WriteLine($"Вікове обмеження: {(target.AgeRestriction.HasValue ? target.AgeRestriction.Value.ToString() + "+" : "Немає")}");
            Console.WriteLine($"Опис: {target.Description ?? "Немає"}");

            if (target.Tags.Count > 0)
                Console.WriteLine($"Теги: {string.Join(", ", target.Tags)}");

            if (target is Series targetSeries)
            {
                Console.WriteLine($"\nПрогрес: {targetSeries.GetWatchProgressSummary()}");
                Console.WriteLine("Структура сезонів:");
                foreach (var season in targetSeries.Seasons)
                {
                    Console.WriteLine($"- Сезон {season.Number} (Загальний час: {season.GetTotalRuntime()} хв)");
                    foreach (var ep in season.Episodes)
                        Console.WriteLine($"  * Епізод {ep.Number}: {ep.Title} ({ep.RuntimeMinutes} хв)");
                }
            }

            Console.WriteLine("\nАктори:");
            if (target.Cast.Count == 0) Console.WriteLine("(немає)");
            foreach (var actor in target.Cast)
                Console.WriteLine($"- {actor.FullName} (Роль: {actor.RoleName})");

            Console.WriteLine("\nСаундтреки:");
            if (target.Soundtracks.Count == 0) Console.WriteLine("(немає)");
            foreach (var song in target.Soundtracks)
                Console.WriteLine($"- {song.GetDisplayName()}");

            Console.WriteLine("\nВідгуки:");
            if (target.Reviews.Count == 0) Console.WriteLine("(немає)");
            foreach (var rev in target.Reviews)
            {
                Console.WriteLine("Без спойлерів: " + rev.GetDisplayText(revealSpoilers: false));
                Console.WriteLine("Зі спойлерами: " + rev.GetDisplayText(revealSpoilers: true));
            }
        }
        else
        {
            Console.WriteLine("Тайтл не знайдено.");
        }
    }

    static void MenuPersonsAndUniverses()
    {
        Console.WriteLine("\n--- ПЕРСОНАЛІЇ ТА ВСЕСВІТИ ---");
        Console.WriteLine("1 - Перевірити вік режисера (Kill Bill), 2 - Переглянути Франшизу");
        Console.Write("Ваш вибір: ");
        string choice = Console.ReadLine() ?? "";

        if (choice == "1")
        {
            var bill = library.Find(m => m.Title.Contains("Kill Bill"));
            if (bill?.MediaDirector != null)
            {
                int directorAge = bill.MediaDirector.GetAge(DateTime.Now.Year);
                Console.WriteLine($"Режисер {bill.MediaDirector.FullName}: {directorAge} років у поточному році.");
            }
        }
        else if (choice == "2")
        {
            Console.WriteLine($"Всесвіт: {tarantinoVerse.UniverseName}");
            Console.WriteLine("Хронологія:");
            foreach (var item in tarantinoVerse.ChronologicalList)
                Console.WriteLine($"- {item.Title}");

            tarantinoVerse.CalculateUniverseProgress(1);
        }
    }

    static void PrintList(List<Media> list)
    {   
        if (list.Count == 0)
        {
            Console.WriteLine("Нічого не знайдено.");
            return;
        }
        Console.WriteLine("\nРезультат:");
        foreach (var item in list)
        {
            Console.WriteLine($"- {item.Title} ({item.ReleaseYear}) - {item.CalculateTimeDebt()} хв.");
        }
    }

    static void SeedData()
    {
        Director dir = new Director("Eric Kripke", 1974, "Satire");
        Director dir1 = new Director("Quentin Tarantino", 1963, "Non-linear storytelling");
        Director dir2 = new Director("Wong Kar-wai", 1958, "Romantic visual storytelling");

        Series theBoys = new Series("The Boys", 2019, 60, 8, dir);
        theBoys.SetVibeRating(new VibeRating(8, 7, 5, 6, 9, 4));
        theBoys.AddGenre("Action");
        theBoys.AddTag("Superheroes");

        Season season1 = new Season(1);
        season1.AddEpisode(new Episode(1, "The Name of the Game", 60));
        season1.AddEpisode(new Episode(2, "Cherry", 58));
        theBoys.AddSeason(season1);
        library.Add(theBoys);

        Movie bill = new Movie("Kill Bill 1", 2003, 111, dir1);
        bill.SetVibeRating(new VibeRating(9, 8, 7, 6, 5, 4));
        bill.AddGenre("Action");
        bill.ApplyAgeRestriction(18, "R");
        bill.UpdateDescription("A former assassin wakes from a coma and wreaks vengeance on her ex-boss.");

        Actor uma = new Actor("Uma Thurman", 1970, "The Bride");
        bill.AddCastMember(uma);

        Song bangBang = new Song("Bang Bang", "Nancy Sinatra");
        bill.AddSong(bangBang);

        Review review = new Review(me, bill, "Main character finds Bill at the end", isSpoiler: true);
        bill.AddReview(review);

        library.Add(bill);


        Movie mood = new Movie("In the Mood for Love", 2000, 98, dir2);
        mood.AddGenre("Romance");
        mood.SetVibeRating(new VibeRating(9, 8, 8, 7, 9, 9));
        library.Add(mood);

        tarantinoVerse = new FranchiseTimeLine("Tarantino Universe");
        tarantinoVerse.AddToTimeline(bill);

        UserMediaState boysState = new UserMediaState(theBoys, WatchStatus.Watching);
        me.MediaStates.Add(boysState);

        boysCheckpoint = new WatchCheckpoint(theBoys, 1, 4, "45:12", "Its boring.");

        me.MarkAsWatched(mood);

        myWatchlist = me.CreateWatchlist("Мої плани", isPrivate: true);
    }
}