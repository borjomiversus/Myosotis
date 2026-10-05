using System;
using System.Collections.Generic;

public class Season
{
    public int Number { get; private set; }
    public List<Episode> Episodes { get; private set; }

    public Season(int number)
    {
        if (number <= 0) throw new ArgumentOutOfRangeException(nameof(number));

        Number = number;
        Episodes = new List<Episode>();
    }

    public void AddEpisode(Episode episode)
    {
        if (episode == null) throw new ArgumentNullException(nameof(episode));
        Episodes.Add(episode);
    }

    public int GetTotalRuntime()
    {
        int total = 0;
        foreach (var episode in Episodes)
        {
            total += episode.RuntimeMinutes;
        }
        return total;
    }
}
