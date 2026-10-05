using System;
using System.Collections.Generic;

public class Series : Media
{
    public int TotalSeasons => Seasons.Count;
    public int AverageEpisodeLength { get; private set; }
    public int UnwatchedEpisodes { get; private set; }
    public List<Season> Seasons { get; private set; }

    public Series(string title, int releaseYear, int averageEpisodeLength, int unwatchedEpisodes, Director? director = null)
        : base(title, releaseYear, director)
    {
        if (averageEpisodeLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(averageEpisodeLength), "Тривалість епізоду має бути більше 0.");
        if (unwatchedEpisodes < 0)
            throw new ArgumentOutOfRangeException(nameof(unwatchedEpisodes), "Кількість непереглянутих серій не може бути від'ємною.");

        AverageEpisodeLength = averageEpisodeLength;
        UnwatchedEpisodes = unwatchedEpisodes;
        Seasons = new List<Season>();
    }

    public void AddSeason(Season season)
    {
        if (season == null) throw new ArgumentNullException(nameof(season));
        Seasons.Add(season);
    }

    public override int CalculateTimeDebt()
    {
        return AverageEpisodeLength * UnwatchedEpisodes;
    }
    public bool IsCaughtUp()
    {
        return UnwatchedEpisodes == 0;
    }

    public string GetWatchProgressSummary()
    {
        return $"{TotalSeasons} сезонів, залишилось {UnwatchedEpisodes} серій ({CalculateTimeDebt()} хв)";
    }
}