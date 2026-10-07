using System.Collections.Generic;
using Xunit;

public class ServicesTests
{
    [Fact]
    public void MediaSearch_AdvancedSearch_FindsMatches()
    {
        var searchService = new MediaSearch();
        var lib = new List<Media>();

        var a = new Movie("Alpha", 2010, 90, null);
        a.AddGenre("Comedy");

        var b = new Movie("Beta", 2015, 90, null);
        b.AddGenre("Comedy");
        lib.Add(a); lib.Add(b);

        var resultTitle = searchService.SearchByTitle(lib, "alpha");
        Assert.Single(resultTitle);

        var resultAdvanced = searchService.AdvancedSearch(lib, "Comedy", 2012, 2020);
        Assert.Single(resultAdvanced);
        Assert.Equal("Beta", resultAdvanced[0].Title);
    }

    [Fact]
    public void MediaRecommendation_GetTopRated_SortsByVibe()
    {
        var recService = new MediaRecommendation();
        var lib = new List<Media>();

        var a = new Movie("Alpha", 2010, 90, null);
        a.SetVibeRating(new VibeRating(5, 5, 5, 5, 5, 5));

        var b = new Movie("Beta", 2015, 90, null);
        b.SetVibeRating(new VibeRating(9, 9, 9, 9, 9, 9));

        lib.Add(a); lib.Add(b);

        var top = recService.GetTopRated(lib, 1);
        Assert.Single(top);
        Assert.Equal("Beta", top[0].Title);
    }

    [Fact]
    public void CaptureProcessing_MoveToWatchlist_TransfersData()
    {
        var capService = new CaptureProcessing();
        var capture = new CaptureEntry("Назва з тт", "TikTok");
        var targetMovie = new Movie("Real Movie", 2022, 120, null);
        var wl = new Watchlist("Plans", false);

        capService.ResolveCapture(capture, targetMovie);
        var moved = capService.MoveToWatchlist(capture, wl);

        Assert.True(moved);
        Assert.Single(wl.Entries);
        Assert.Equal(CaptureStatus.Archived, capture.Status);
    }
}