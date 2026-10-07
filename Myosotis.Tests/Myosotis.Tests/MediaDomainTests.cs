using System;
using Xunit;

public class MediaDomainTests
{
    [Fact]
    public void Movie_AgeRestriction_ChecksAppropriateness()
    {
        var m = new Movie("Test", 2010, 100, null);
        m.ApplyAgeRestriction(16, "R");

        Assert.True(m.IsAgeAppropriate(18));
        Assert.False(m.IsAgeAppropriate(10));
    }

    [Fact]
    public void Series_CalculateTimeDebt_ReturnsTotalUnwatched()
    {
        var s = new Series("Series Test", 2018, 45, 6, null);

        Assert.Equal(270, s.CalculateTimeDebt());
        Assert.False(s.IsCaughtUp());
    }

    [Fact]
    public void Season_GetTotalRuntime_SumsEpisodes()
    {
        var season = new Season(1);
        season.AddEpisode(new Episode(1, "Pilot", 45));
        season.AddEpisode(new Episode(2, "Second", 50));

        Assert.Equal(2, season.Episodes.Count);
        Assert.Equal(95, season.GetTotalRuntime());
    }

    [Fact]
    public void VibeRating_CalculateAverage_ReturnsCorrectDouble()
    {
        var v = new VibeRating(8, 8, 8, 8, 8, 8);

        Assert.Equal(8.0, v.CalculateAverage(), 3);
        Assert.True(v.IsHighlyRated(7));
        Assert.Throws<ArgumentOutOfRangeException>(() => new VibeRating(11, 5, 5, 5, 5, 5));
    }

    [Fact]
    public void Review_GetDisplayText_HandlesSpoilers()
    {
        var author = new User("reviewer");
        var m = new Movie("Reviewed Movie", 2020, 100, null);
        var r = new Review(author, m, "Головний герой помирає", isSpoiler: true);

        Assert.DoesNotContain("помирає", r.GetDisplayText());
        Assert.Contains("помирає", r.GetDisplayText(revealSpoilers: true));
    }

    [Fact]
    public void FranchiseTimeLine_AddToTimeline_TracksProgress()
    {
        var f = new FranchiseTimeLine("Test Universe");
        f.AddToTimeline(new Movie("F1", 2001, 100, null));
        f.AddToTimeline(new Movie("F2", 2003, 100, null));

        Assert.Equal(2, f.ChronologicalList.Count);
    }
}