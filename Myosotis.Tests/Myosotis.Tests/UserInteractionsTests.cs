using System;
using Xunit;

public class UserInteractionsTests
{
    [Fact]
    public void CaptureEntry_ResolveAndArchive_ChangesStatus()
    {
        var c = new CaptureEntry("Нотатка", "TikTok");
        Assert.Equal(CaptureStatus.Unresolved, c.Status);

        var found = new Movie("Found Movie", 2019, 100, null);
        c.Resolve(found);

        Assert.Equal(CaptureStatus.Resolved, c.Status);
        Assert.Equal(found, c.ResolvedMedia);

        c.Archive();
        Assert.Equal(CaptureStatus.Archived, c.Status);
    }

    [Fact]
    public void Watchlist_AddAndRemove_UpdatesTimeDebt()
    {
        var w = new Watchlist("Test List", isPrivate: false);
        var m = new Movie("WL Movie", 2010, 60, null);

        w.AddEntry(m, "нотатка");
        Assert.Single(w.Entries);
        Assert.Equal(60, w.GetTotalTimeDebt());

        var removed = w.RemoveEntry(m);
        Assert.True(removed);
        Assert.Empty(w.Entries);
    }

    [Fact]
    public void UserMediaState_ChangeStatus_TracksTime()
    {
        var m = new Movie("Status Test", 2020, 90, null);
        var state = new UserMediaState(m, WatchStatus.Planned);

        Assert.False(state.IsActive());

        state.ChangeStatus(WatchStatus.Watching);
        Assert.True(state.IsActive());
        Assert.NotNull(state.StartedAt);

        state.ChangeStatus(WatchStatus.Watched);
        Assert.NotNull(state.CompletedAt);
    }

    [Fact]
    public void WatchCheckpoint_ResumeWatching_DeactivatesCheckpoint()
    {
        var s = new Series("Paused Series", 2020, 40, 3, null);
        var wc = new WatchCheckpoint(s, 1, 5, "12:34", "Нудно");

        Assert.True(wc.IsActive);
        Assert.Contains("S1E5", wc.GetStatusDescription());

        var result = wc.ResumeWatching();
        Assert.False(wc.IsActive);
        Assert.Contains("Paused Series", result);
    }

    [Fact]
    public void User_MonthlyStats_CalculatesOnlyForTargetMonth()
    {
        var u = new User("test_user");
        var m1 = new Movie("Jan", 2020, 100, null);
        m1.AddGenre("Comedy");

        u.WatchHistory.Add(new WatchHistoryEntry(m1, new DateTime(2026, 1, 10)));

        Assert.Equal(100, u.GetMonthlyStats(2026, 1));
        Assert.Equal(0, u.GetMonthlyStats(2026, 2));

        var breakdown = u.GetMonthlyGenreStats(2026, 1);
        Assert.True(breakdown.ContainsKey("Comedy"));
    }
}