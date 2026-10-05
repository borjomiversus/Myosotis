using System;
using System.Collections.Generic;

public class FranchiseTimeLine
{
	public string UniverseName { get; private set; }
    public List<Media> ChronologicalList { get; private set; }


    public FranchiseTimeLine(string universeName)
    {
        if (string.IsNullOrWhiteSpace(universeName))
            throw new ArgumentException("Назва всесвіту не може бути порожньою.");

        UniverseName = universeName;
        ChronologicalList = new List<Media>();
    }

    public void AddToTimeline(Media item)
    {
        if (item != null)
            ChronologicalList.Add(item);
    }

    public void CalculateUniverseProgress(int watchedCount)
    {
        if (watchedCount < 0 || watchedCount > ChronologicalList.Count)
        {
            Console.WriteLine("Неправильне значення кількості переглянутих елементів.");
            return;
        }
        double progress = (double)watchedCount / ChronologicalList.Count * 100;
        Console.WriteLine($"Ви подивилися {progress:F1}% всесвіту {UniverseName}.");
    }
}
