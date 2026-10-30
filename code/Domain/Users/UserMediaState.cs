using System;

public enum WatchStatus
{
    Planned,
    Watching,
    Watched,
    Paused,
    Dropped,
    Rewatching, 
    None
}

// Статус перегляду конкретного User для конкретного Media

public class UserMediaState
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Media Item { get; private set; }
    public WatchStatus Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public bool IsFavorite { get;  private set; } = false;

    public UserMediaState(Media item, WatchStatus status)
    {

        Item = item ?? throw new ArgumentNullException(nameof(item));
        Status = status;

        if (status == WatchStatus.Watching || status == WatchStatus.Rewatching)
            StartedAt = DateTime.Now;

        if (status == WatchStatus.Watched)
            CompletedAt = DateTime.Now;
    }

   
    public void ChangeStatus(WatchStatus newStatus)
    {
        // Якщо тільки почали дивитися
        if (StartedAt == null && (newStatus == WatchStatus.Watching || newStatus == WatchStatus.Rewatching))
        {
            StartedAt = DateTime.Now;
        }

        // Якщо додивилися до кінця
        if (newStatus == WatchStatus.Watched)
        {
            CompletedAt = DateTime.Now;
        }

        Status = newStatus;
    }

    public bool IsActive()
    {
        return Status == WatchStatus.Watching || Status == WatchStatus.Rewatching;
    }

    public void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
    }
}
