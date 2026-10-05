// тайтл + мій особистий коментар до нього в межах конкретної підбірки
using System;

public class WatchlistEntry
{
    public Media Item { get; private set; }
    public string? PersonalNote { get; private set; }
    public DateTime AddedAt { get; private set; }

    public WatchlistEntry(Media item, string? personalNote)
    {
        Item = item ?? throw new ArgumentNullException(nameof(item));
        PersonalNote = personalNote;
        AddedAt = DateTime.Now;
    }

    public void UpdateNote(string note)
    {
        PersonalNote = note;
    }

    public string GetSummary() => $"{Item.Title} — {PersonalNote} (додано {AddedAt:d})";
}