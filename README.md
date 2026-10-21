 <div align="center">

<img width="1687" height="564" alt="logo1" src="https://github.com/user-attachments/assets/eb8e7446-4dd5-416e-9968-fd40d5c9d421" />

# Myosotis

> **Myosotis** — персональний застосунок для пошуку, організації та відстеження фільмів і серіалів.

![C%23](https://img.shields.io/badge/C%23-239120?logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![Blazor Server](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor&logoColor=white)
![MudBlazor](https://img.shields.io/badge/MudBlazor-UI-5C80BC)
![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-CDN-06B6D4?logo=tailwindcss&logoColor=white)

</div>

---

## Ідея

Ідея проєкту поєднує можливості каталогу медіа, персональних списків перегляду, історії перегляду, нотаток та системи збереження випадково знайденого контенту.

Основна особливість проєкту — **Нотатки**: користувач може швидко зберегти будь-яку згадку про фільм або серіал у довільній формі, навіть якщо він ще не знає його точної назви. Пізніше ця нотатка може бути пов’язана з конкретним медіа та додана до відповідного списку.

На відміну від звичайного каталогу, Myosotis робить акцент на **персональному контексті користувача**.
Тобто користувач може спочатку зберегти нечітку згадку про контент, а структурувати її пізніше.

## Мета
Створити єдину екосистему для комфортного управління кіно-вподобаннями, яка дозволяє користувачеві:
- Знаходити актуальні фільми та серіали, переглядати акторський склад, трейлери й постери через інтеграцію з TMDb API.
- Створювати власні приватні чи публічні добірки та зберігати особисті коментарі до них.
- Контролювати статус перегляду та фіксувати перегляд через систему чекпоінтів.
- Використовувати унікальну систему оцінювання за шістьма критеріями та переглядати щомісячну статистику переглядів.
- Використовувати функцію «Рулетки» та рекомендації на основі вільних хвилин або улюблених жанрів.

## Для кого цей застосунок?

Застосунок створений для активних кіноманів і любителів серіалів, чия інформація про цікавий контент зазвичай розкидана між месенджерами, нотатками в телефоні та вкладками браузера. 

## Можливості

| Функція | Опис |
|---|---|
|  Captures | Нотатки типів `Thought`, `Quote`, `Scene`, `Note` зі статусами `Unresolved → Resolved → Archived` |
|  Watchlists | Приватні та публічні добірки з особистими нотатками до кожного тайтлу |
|  Tracking | Статуси перегляду: `Planned`, `Watching`, `Watched`, `Paused`, `Dropped`, `Rewatching` |
|  WatchCheckpoint | Збереження контексту перегляду: сезон і серія або таймкод, а також причина паузи |
|  Roulette | Випадковий вибір серед ще не переглянутих тайтлів з урахуванням доступного часу та жанру |
|  Recommendations | Рекомендації схожих тайтлів на основі спільних жанрів |
|  Vibe Rating | Оцінювання за шістьма критеріями: Characters, Plot, Visuals, Pacing, Vibe, Soundtrack |
|  Статистика | Щомісячна статистика переглядів: кількість, час, жанри та режисер, чиї роботи переглядалися найчастіше |
|  Пошук | Пошук за назвою, жанром та актором |

## Технології

| Технологія | Призначення |
|---|---|
| C# / .NET 10 | Основна мова програмування та середовище виконання |
| Blazor Server | Користувацький інтерфейс |
| Tailwind CSS (CDN) | Стилізація інтерфейсу |
| MudBlazor | Діалогові вікна та сповіщення |
| Material Symbols | Іконки |
| TBDB API | База медіа |

## Обґрунтування вибору технології

Основною мовою розробки проєкту обрано **C#**.

C# добре підтримує основні принципи ООП, які є фундаментальними для подальшого навчання та поглиблення в ООП. 
С# є С-подібною мовою, тож поріг входження та розуміння є легшим, так як досвід вивчення С++ був увесь 1 курс.
Платформа .NET також дозволяє розвивати проєкт від початкового прототипу до застосунку з графічним інтерфейсом, зовнішніми API та постійним збереженням даних.

## Поточний стан

- Реалізовано предметну модель і основні сервіси.
- Створено сторінки Blazor: Overview, Watchlists, Captures, History, Search.
- Наразі використовуються лише демонстраційні дані, що зберігаються в пам'яті.
- Сторінка детальної інформації про фільм або серіал, інтеграція TMDb, збереження даних у JSON та розгортання застосунку заплановані на наступні етапи.

## Модель предметної області (основні класи)

| Клас | Призначення |
|---|---|
| `Media` | Абстрактна базова сутність медіаконтенту |
| `Movie` / `Series` | Типи медіаконтенту |
| `Person` / `Actor` / `Director` | Люди, пов'язані з тайтлами |
| `User` | Власник списків, нотаток, історії перегляду, статусів і чекпоінтів |
| `Watchlist` / `WatchlistEntry` | Користувацькі добірки та записи в них |
| `CaptureEntry` | Необроблена нотатка, яку згодом можна пов'язати з конкретним тайтлом |
| `WatchCheckpoint` | Збережена позиція перегляду та контекст паузи |
| `VibeRating` | Оцінювання за шістьма критеріями з перевіркою значень у діапазоні 0–10 |

## Скріншоти

Файли скріншотів будуть додані згодом.

## Дизайн

<div align="center">
  <a href="https://canva.link/oh12vzw1rp4tucj" target="_blank">
<img width="1748" height="1240" alt="collect watch remember, копія (1)" src="https://github.com/user-attachments/assets/b50ae596-a935-4f95-8a7e-c4c25896436d" />
  </a>
  <br><br>
  
  <!-- Колірна палітра -->
  ![#1A2235](https://img.shields.io/badge/1A2235-1A2235?style=for-the-badge)
  ![#332D57](https://img.shields.io/badge/332D57-332D57?style=for-the-badge)
  ![#5C80BC](https://img.shields.io/badge/5C80BC-5C80BC?style=for-the-badge)
  ![#84DCC6](https://img.shields.io/badge/84DCC6-84DCC6?style=for-the-badge)
  ![#E2E8F0](https://img.shields.io/badge/E2E8F0-E2E8F0?style=for-the-badge)
  ![#D4915A](https://img.shields.io/badge/D4915A-D4915A?style=for-the-badge)
[Переглянути на Coolors](https://coolors.co/1a2235-332d57-5c80bc-84dcc6-e2e8f0-d4915a)
</div>

## Локальний запуск

Для запуску потрібен [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
git clone [https://github.com/your-username/Myosotis.git](https://github.com/your-username/Myosotis.git)
cd Myosotis/MyosotisWeb
dotnet run
```
Після успішного запуску відкрийте у браузері адресу, вказану в терміналі.

## Структура проєкту

```text
Myosotis/
├── MyosotisWeb/      # Застосунок Blazor (інтерфейс і вебсервіси)
├── code/             # Предметна модель і основна логіка
├── Myosotis.Tests/   # Тести
└── README.md
```

## Дорожня карта

- [x] Предметна модель і сервіси
- [x] Інтерфейс Blazor: Overview, Watchlists, Captures, History, Search
- [x] Roulette, рекомендації, щомісячна статистика
- [x] Сторінка фільму / серіалу
- [x] Інтеграція TMDb (метадані та постери)
- [ ] Збереження даних у JSON
- [ ] Розгортання в Azure
- [ ] Користувацький профіль

## Процес розробки

- Ідея продукту, назва, палітра, вибір технологій і рішення щодо структури даних визначені.
- Спочатку було досліджено та створено прототип дизайну, який згодом адаптовано вручну.
- Інструменти штучного інтелекту (Claude, Gemini, Google Studio) використовувалися як допоміжні засоби для роботи з кодом, рефакторингу, дизайном і документацією з подальшою ручною перевіркою.

## Джерела

- [Movie-Tracker-App (ertugrulgacal)](https://github.com/ertugrulgacal/Movie-Tracker-App)
- [movieTracker (LeilaniL)](https://github.com/LeilaniL/movieTracker) 
- [Movie-Website (krishnashah122)](https://github.com/krishnashah122/Movie-Website) 
- [Movie Suggest](https://moviesuggest.netlify.app/) 

- [Serializd Discover](https://serializd.com/discover), [2020 Spotify Wrapped Dashboard](https://dribbble.com/shots/14686871-2020-Spotify-Wrapped-Dashboard-Concept), [Abstract Star Logo](http://dribbble.com/shots/26983428-Abstract-Star-Logo-Concept-Modern-Gradient-Logo-Concept-for-App), [Movie Site](https://dribbble.com/search/movie-site), [Raycast](https://dribbble.com/search/raycast) 
- [Glassmorphism](https://dribbble.com/tags/glassmorphism)
- [OriginKit](https://www.originkit.dev/), [SmoothUI](https://smoothui.dev/docs/components), [MudBlazor Docs](https://mudblazor.com/docs/overview), [Design.dev Tools](https://design.dev/tools/)

- [TMDB API: Getting Started](https://developer.themoviedb.org/docs/getting-started), [Movie Details](https://developer.themoviedb.org/reference/movie-details).
- [Blazor WebAssembly Hosting Models](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-10.0#blazor-webassembly) 
- [Awesome Blazor](https://github.com/AdrienTorris/awesome-blazor) 
- [Calling a Web API from a .NET Client](https://learn.microsoft.com/fr-fr/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client), [Tutorial: Console WebApiClient](https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/console-webapiclient).

- [Blazor Tutorial (YouTube)](https://www.youtube.com/watch?v=dY_AWdCzsCY&list=PLzewa6pjbr3IQEUfNiK2SROQC1NuKl6PV&index=1).
- [TMDB API Tutorial (YouTube)](https://www.youtube.com/watch?v=FlFyrOEz2S4).
- [Tailwind CSS Guide (YouTube)](https://www.youtube.com/watch?v=xS4ISh6ZehU).
- [Уроки з розробки C# (YouTube Playlist)](https://www.youtube.com/watch?v=03YGv7Myb3o&list=PLbDsRCGLbN1_-ZWS6G5ionKox0Ahrdv_H).
- [JSON2CSharp](https://json2csharp.com/) 

- [Blazor](https://learn.microsoft.com/aspnet/core/blazor/hosting-models)
- [MudBlazor](https://mudblazor.com/docs/overview)
- [TMDb API](https://developer.themoviedb.org/docs/getting-started)
- [Клієнт для роботи з Web API у .NET](https://learn.microsoft.com/dotnet/csharp/tutorials/console-webapiclient)

- [Movie-Tracker-App](https://github.com/ertugrulgacal/Movie-Tracker-App)
- [movieTracker](https://github.com/LeilaniL/movieTracker)
- [Movie-Website](https://github.com/krishnashah122/Movie-Website)

- [Glassmorphism (Dribbble)](https://dribbble.com/tags/glassmorphism)
- [Spotify Wrapped Dashboard Concept](https://dribbble.com/shots/14686871-2020-Spotify-Wrapped-Dashboard-Concept)
- [Serializd](https://serializd.com/discover)

- [Посібник із Blazor](https://www.youtube.com/watch?v=dY_AWdCzsCY&list=PLzewa6pjbr3IQEUfNiK2SROQC1NuKl6PV&index=1)
- [Посібник із Tailwind CSS](https://www.youtube.com/watch?v=xS4ISh6ZehU)
- [Посібник із TMDb](https://www.youtube.com/watch?v=FlFyrOEz2S4)

## Ліцензія

Ліцензію ще не визначено.



