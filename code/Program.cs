using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        List<Media> library = new List<Media>();
        MediaLibrary mediaLibrary = new MediaLibrary();
        User me = new User("spacedream");

        Director dir = new Director("Eric Kripke", 1974, "Satire");
        Director dir1 = new Director("Quentin Tarantino", 1963, "Non-linear storytelling");
        Director dir2 = new Director("Wong Kar-wai", 1958, "Romantic visual storytelling");

        Series theBoys = new Series("The Boys", 2019, 60, 8, dir);
        theBoys.SetVibeRating(new VibeRating(8, 7, 5, 6, 9, 4));
        theBoys.AddGenre("Action");
        library.Add(theBoys);

        Movie bill = new Movie("Kill Bill: Vol. 1", 2003, 111, dir1);
        bill.SetVibeRating(new VibeRating(9, 8, 7, 6, 5, 4));
        bill.AddGenre("Action");

        Actor uma = new Actor("Uma Thurman", 1970, "The Bride");
        bill.AddCastMember(uma);
        library.Add(bill);

        Song bangBang = new Song("Bang Bang", "Nancy Sinatra");
        bill.AddSong(bangBang);

        Review review = new Review(me, bill, "Головна героїня знаходить Білла наприкінці", isSpoiler: true);
        bill.AddReview(review);

        Movie mood = new Movie("In the Mood for Love", 2000, 98, dir2);
        mood.AddGenre("Romance");
        mood.SetVibeRating(new VibeRating(9, 8, 8, 7, 9, 9));
        library.Add(mood);

        FranchiseTimeLine tarantinoVerse = new FranchiseTimeLine("Tarantino Universe");
        tarantinoVerse.AddToTimeline(bill);

        UserMediaState boysState = new UserMediaState(theBoys, WatchStatus.Watching);
        me.MediaStates.Add(boysState);

        WatchCheckpoint boysCheckpoint = new WatchCheckpoint(theBoys, 1, 4, "45:12", "Треба йти на пару");

        me.MarkAsWatched(mood);

        Watchlist myWatchlist = me.CreateWatchlist("Мої плани", isPrivate: true);

        while (true)
        {
            Console.WriteLine("\n===================================");
            Console.WriteLine("1. Показати всю бібліотеку");
            Console.WriteLine("2. Пошук (За назвою або Актором)");
            Console.WriteLine("3. Фільтри (Розширений пошук)");
            Console.WriteLine("4. Топ рейтингу (GetTopRated)");
            Console.WriteLine("5. Знайти схожі (FindSimilar)");
            Console.WriteLine("6. Рулетка");
            Console.WriteLine("7. Нотатки та Підбірки");
            Console.WriteLine("8. Керування переглядом (Статуси, Паузи, Статистика)");
            Console.WriteLine("9. Франшизи та Всесвіти (FranchiseTimeLine)");
            Console.WriteLine("10. Відгуки та Саундтреки (Review, Song)");
            Console.WriteLine("0. Вийти");
            Console.Write("Оберіть дію: ");

            string choice = Console.ReadLine() ?? "";

            if (choice == "1")
            {
                Console.WriteLine("\n--- Ваша бібліотека ---");
                foreach (var item in library)
                    Console.WriteLine($"- {item.GetFormattedSummary()} [Жанри: {string.Join(", ", item.Genres)}]");
            }
            else if (choice == "2")
            {
                Console.WriteLine("Шукати за: 1 - Назвою, 2 - Актором");
                string subChoice = Console.ReadLine() ?? "";
                Console.Write("Введіть запит: ");
                string query = Console.ReadLine() ?? "";

                List<Media> found = new List<Media>();
                if (subChoice == "1")
                    found = mediaLibrary.SearchByTitle(library, query);
                else if (subChoice == "2")
                    found = mediaLibrary.SearchByActor(library, query);

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

                var found = mediaLibrary.AdvancedSearch(library, genre, minYear, maxYear);
                PrintList(found);
            }
            else if (choice == "4")
            {
                Console.Write("Скільки тайтлів показати у топі? ");
                int.TryParse(Console.ReadLine(), out int count);
                var top = mediaLibrary.GetTopRated(library, count);

                Console.WriteLine("\n--- Найвищий Vibe Rating ---");
                foreach (var item in top)
                    Console.WriteLine($"- {item.Title}: {item.Vibe?.CalculateAverage():F1}/10");
            }
            else if (choice == "5")
            {
                Console.WriteLine("Оберіть приклад (введіть точну назву):");
                string targetTitle = Console.ReadLine() ?? "";
                var target = library.Find(m => m.Title.Equals(targetTitle, StringComparison.OrdinalIgnoreCase));

                if (target != null)
                {
                    var similar = mediaLibrary.FindSimilar(library, target, 3);
                    Console.WriteLine($"\nСхожі на '{target.Title}':");
                    PrintList(similar);
                }
                else
                {
                    Console.WriteLine("Такого тайтлу немає в бібліотеці.");
                }
            }
            else if (choice == "6")
            {
                Console.Write("Максимальний час на перегляд (хв): ");
                if (!int.TryParse(Console.ReadLine(), out int maxMins)) maxMins = 1000;

                Console.Write("Бажаний жанр (або натисніть Enter): ");
                string tag = Console.ReadLine() ?? "";
                if (string.IsNullOrWhiteSpace(tag)) tag = null!;

                int matchCount = library.FindAll(m => m.CalculateTimeDebt() <= maxMins &&
                                                (string.IsNullOrEmpty(tag) || m.Genres.Contains(tag))).Count;
                Console.WriteLine($"Рулетка обирає з {matchCount} варіантів.");

                var criteria = new RouletteCriteria(maxMins, tag);
                var randomPick = mediaLibrary.ChooseByRoulette(library, criteria);

                if (randomPick != null)
                    Console.WriteLine($"Випало: {randomPick.Title} ({randomPick.CalculateTimeDebt()} хв.)");
                else
                    Console.WriteLine("Нічого не знайдено.");
            }
            else if (choice == "7")
            {
                Console.WriteLine("1 - Створити нотатку, 2 - Перемістити нерозпізнані");
                string subChoice = Console.ReadLine() ?? "";

                if (subChoice == "1")
                {
                    Console.Write("Нотатка: ");
                    string note = Console.ReadLine() ?? "";
                    me.AddCapture(note, "Console");
                    Console.WriteLine("Збережено!");
                }
                else if (subChoice == "2")
                {
                    var captures = me.GetUnresolvedCaptures();
                    foreach (var cap in captures)
                    {
                        Console.WriteLine($"\nОбробка: {cap.RawNote}");
                        Console.Write("Введіть назву фільму для прив'язки: ");
                        string query = Console.ReadLine() ?? "";
                        var found = mediaLibrary.SearchByTitle(library, query);

                        if (found.Count > 0)
                        {
                            mediaLibrary.ResolveCapture(cap, found[0]);
                            me.MoveResolvedCaptureToWatchlist(cap, "Мої плани");
                            Console.WriteLine($"Прив'язано до {found[0].Title} і додано в 'Мої плани'.");
                        }
                    }
                    myWatchlist.PrintContents();
                }
            }

            else if (choice == "8")
            {
                Console.WriteLine("\n--- Керування переглядом ---");
                Console.WriteLine("1 - Поточні статуси, 2 - Паузи (Checkpoints), 3 - Статистика");
                string subChoice = Console.ReadLine() ?? "";

                if (subChoice == "1")
                {
                    foreach (var state in me.MediaStates)
                        Console.WriteLine($"- {state.Item.Title}: {state.Status} (Почато: {state.StartedAt})");
                }
                else if (subChoice == "2")
                {
                    Console.WriteLine(boysCheckpoint.GetStatusDescription());
                    Console.Write("Відновити перегляд? (т/н): ");
                    if ((Console.ReadLine() ?? "").ToLower() == "т")
                        Console.WriteLine(boysCheckpoint.ResumeWatching());
                }
                else if (subChoice == "3")
                {
                    int year = DateTime.Now.Year;
                    int month = DateTime.Now.Month;
                    int minutes = me.GetMonthlyStats(year, month);
                    Console.WriteLine($"\nЗа цей місяць ви подивилися {minutes} хвилин.");

                    var genres = me.GetMonthlyGenreStats(year, month);
                    foreach (var g in genres)
                        Console.WriteLine($"- Жанр '{g.Key}': {g.Value} тайтл(ів)");
                }
            }
            else if (choice == "9")
            {
                Console.WriteLine("\n--- Франшизи та Всесвіти ---");
                Console.WriteLine($"Всесвіт: {tarantinoVerse.UniverseName}");
                Console.WriteLine("Хронологія:");
                foreach (var item in tarantinoVerse.ChronologicalList)
                    Console.WriteLine($"- {item.Title}");

                tarantinoVerse.CalculateUniverseProgress(1);
            }
            else if (choice == "10")
            {
                Console.WriteLine("\n--- Відгуки та Саундтреки (на прикладі Kill Bill) ---");

                Console.WriteLine("Саундтреки:");
                foreach (var song in bill.Soundtracks)
                    Console.WriteLine($"- {song.GetDisplayName()}");

                Console.WriteLine("\nВідгуки:");
                foreach (var rev in bill.Reviews)
                {
                    Console.WriteLine("Без спойлерів: " + rev.GetDisplayText(revealSpoilers: false));
                    Console.WriteLine("Зі спойлерами: " + rev.GetDisplayText(revealSpoilers: true));
                }
            }
            else if (choice == "0")
            {
                break;
            }
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
}
