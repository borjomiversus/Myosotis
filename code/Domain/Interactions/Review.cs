using System;

public class Review
{
    public User Author { get; private set; }
    public Media AboutMedia { get; private set; }
    public string Text { get; private set; }
    public bool IsSpoiler { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Review(User author, Media aboutMedia, string text, bool isSpoiler = false)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Review text cannot be empty.", nameof(text));

        Author = author ?? throw new ArgumentNullException(nameof(author));
        AboutMedia = aboutMedia ?? throw new ArgumentNullException(nameof(aboutMedia));
        Text = text;
        IsSpoiler = isSpoiler;
        CreatedAt = DateTime.Now;
    }

    public string GetDisplayText(bool revealSpoilers = false)
    {
        if (IsSpoiler && !revealSpoilers)
            return $"{Author.Username}: [містить спойлери — приховано]";
        return $"{Author.Username}: {Text}";
    }
}
