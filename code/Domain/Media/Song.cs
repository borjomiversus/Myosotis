// Одна пісня з саундтреку конкретного тайтла
// Дані сюди мають підтягуватись з реального джерела 

public class Song
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; }
    public string Artist { get; private set; }
    public string? SpotifyUrl { get; private set; }

    public Song(string title, string artist, string? spotifyUrl = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Назва пісні відсутня.");
        if (string.IsNullOrWhiteSpace(artist))
            throw new ArgumentException("Виконавець не вказаний.");

        Title = title;
        Artist = artist;
        SpotifyUrl = spotifyUrl;
    }

    public string GetDisplayName()
    {
        return $"{Title} — {Artist}";
    }
}
