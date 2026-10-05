public class CaptureProcessing
{
    public void ResolveCapture(CaptureEntry capture, Media identifiedMedia)
    {
        if (capture == null || identifiedMedia == null) return;
        capture.Resolve(identifiedMedia);
    }

    public bool MoveToWatchlist(CaptureEntry capture, Watchlist watchlist)
    {
        if (capture.Status != CaptureStatus.Resolved || capture.ResolvedMedia == null)
        {
            return false;
        }
        watchlist.AddEntry(capture.ResolvedMedia, capture.RawNote);
        capture.Archive();
        return true;
    }
}