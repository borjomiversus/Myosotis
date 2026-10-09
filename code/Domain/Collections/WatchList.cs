using System;
using System.Collections.Generic;

public class Watchlist
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public bool IsPrivate { get; private set; }
    public List<WatchlistEntry> Entries { get; private set; }

    public Watchlist(string name, bool isPrivate = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва підбірки не може бути порожньою.", nameof(name));

        Name = name;
        IsPrivate = isPrivate;
        Entries = new List<WatchlistEntry>();
    }

    public void AddEntry(Media item, string? personalNote)
    {
        if (item == null) return;
        Entries.Add(new WatchlistEntry(item, personalNote));
    }

    public bool RemoveEntry(Media item)
    {
        WatchlistEntry? targetEntry = null;
        foreach (var e in Entries)
        {
            if (e.Item == item)
            {
                targetEntry = e;
                break;
            }
        }

        if (targetEntry == null) return false;

        Entries.Remove(targetEntry);
        return true;
    }

    public int GetTotalTimeDebt()
    {
        int total = 0;
        foreach (var entry in Entries)
        {
            total += entry.Item.CalculateTimeDebt();
        }
        return total;
    }

    public void PrintContents()
    {
        Console.WriteLine($"--- Підбірка \"{Name}\" ({(IsPrivate ? "приватна" : "публічна")}) ---");
        if (Entries.Count == 0)
        {
            Console.WriteLine("(порожньо)");
            return;
        }
        foreach (var entry in Entries)
            Console.WriteLine(entry.GetSummary());
    }
}