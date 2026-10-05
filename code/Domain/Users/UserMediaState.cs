using System;

public enum WatchStatus
{
    Planned,
    Watching,
    Watched,
    Paused,
    Dropped,
    Rewatching
}

// Статус перегляду конкретного User для конкретного Media

public class UserMediaState
{
    public Media Item { get; private set; }
    public WatchStatus Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

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

    // чи зараз це дивимося
    public bool IsActive()
    {
        return Status == WatchStatus.Watching || Status == WatchStatus.Rewatching;
    }
}
