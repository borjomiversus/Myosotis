using System;
using System.Collections.Generic;

public abstract class Media : IComparable<Media>
{
    public string Title { get; private set; }
    public int ReleaseYear { get; private set; }
    public string? ProductionStudio { get; private set; }
    public string? PosterUrl { get; private set; }
    public string? TrailerUrl { get; private set; }
    public string? Description { get; private set; }
    public string? ContentRating { get; private set; }
    public string? Country { get; private set; }
    public int? AgeRestriction { get; private set; }

    public VibeRating? Vibe { get; private set; }
    public Director? MediaDirector { get; private set; }
    public List<Actor> Cast { get; private set; }
    public List<Review> Reviews { get; private set; }
    public List<Song> Soundtracks { get; private set; }
    public List<string> Genres { get; private set; }
    public List<string> Tags { get; private set; }

    protected Media(string title, int releaseYear, Director? director = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Назва не може бути порожньою.", nameof(title));
        if (releaseYear < 1888)
            throw new ArgumentOutOfRangeException(nameof(releaseYear), "Рік випуску некоректний.");

        Title = title;
        ReleaseYear = releaseYear;

        MediaDirector = director;

        Cast = new List<Actor>();
        Reviews = new List<Review>();
        Soundtracks = new List<Song>();
        Genres = new List<string>();
        Tags = new List<string>();
    }

    public void UpdateDescription(string newDescription)
    {
        Description = newDescription;
    }

    public void SetProductionDetails(string studio, string country)
    {
        ProductionStudio = studio;
        Country = country;
    }

    public void ApplyAgeRestriction( int age, string contentRating)
    {
        if (age < 0)
            throw new ArgumentOutOfRangeException(nameof(age));

        AgeRestriction = age;
        ContentRating = contentRating;
    }

    public void SetVibeRating(VibeRating vibe)
    {
        Vibe = vibe;
    }

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return;

        Tags.Add(tag);
    }

    public void AddGenre(string genre)
    {
        if (string.IsNullOrWhiteSpace(genre))
            return;

        Genres.Add(genre);
    }

    public void AddCastMember(Actor actor)
    {
        if (actor == null)
            throw new ArgumentNullException(nameof(actor));

        Cast.Add(actor);
    }

    public void AddSong(Song song)
    {
        if (song == null)
            throw new ArgumentNullException(nameof(song));

        Soundtracks.Add(song);
    }

    public void AddReview(Review review)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        Reviews.Add(review);
    }

    public bool IsAgeAppropriate(int viewerAge)
    {
        if (AgeRestriction == null)
            return true;

        return viewerAge >= AgeRestriction.Value;
    }

    public int CountSharedGenres(Media other)
    {
        if (other == null)
            return 0;

        int sharedCount = 0;

        foreach (var genre in Genres)
        {
            if (other.Genres.Contains(genre))
                sharedCount++;
        }

        return sharedCount;
    }

    public int CompareTo(Media? other)
    {
        if (other == null)
            return 1;

        return ReleaseYear.CompareTo(other.ReleaseYear);
    }

    public bool MatchesFilter(string tag)
    {
        return Tags.Contains(tag) || Genres.Contains(tag);
    }

    public string GetFormattedSummary()
    {
        return $"{Title} ({ReleaseYear}) - {Description}";
    }

    public abstract int CalculateTimeDebt();
}