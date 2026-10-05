using System;

// що дивилась + коли — окремо від RecentlyViewed, зберігається повністю для підрахунку статистики.
public class WatchHistoryEntry
{
    public Media WatchedItem { get; private set; }
    public DateTime WatchDate { get; private set; }

    public WatchHistoryEntry(Media watchedItem, DateTime watchDate)
    {
        WatchedItem = watchedItem ?? throw new ArgumentNullException(nameof(watchedItem));
        WatchDate = watchDate;
    }
}