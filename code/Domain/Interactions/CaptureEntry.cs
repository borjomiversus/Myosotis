using System;

public enum CaptureStatus
{
    Unresolved,
    Resolved,
    Archived
}

// Сюди падає все, що зачепило користувача, ще до того, як стало структурованим Media.
public class CaptureEntry
{
    public string RawNote { get; private set; }
    public string? Source { get; private set; }
    public DateTime CapturedAt { get; private set; }
    public CaptureStatus Status { get; private set; }
    public Media? ResolvedMedia { get; private set; }

    public CaptureEntry(string rawNote, string? source)
    {
        if (string.IsNullOrWhiteSpace(rawNote))
            throw new ArgumentException("Нотатка не може бути порожньою.", nameof(rawNote));

        RawNote = rawNote;
        Source = source;
        CapturedAt = DateTime.Now;
        Status = CaptureStatus.Unresolved;
    }

    public void Resolve(Media identifiedMedia)
    {
        ResolvedMedia = identifiedMedia ?? throw new ArgumentNullException(nameof(identifiedMedia));
        Status = CaptureStatus.Resolved;
    }

    public void Archive() => Status = CaptureStatus.Archived;

    public string GetStatusSummary()
    {
        if (Status == CaptureStatus.Resolved)
        {
            return "Розпізнано: " + ResolvedMedia.Title + " (нотатка: " + RawNote + ")";
        }
        else if (Status == CaptureStatus.Archived)
        {
            return "[архів] " + RawNote;
        }
        else
        {
            return "Не розпізнано: " + RawNote + " (джерело: " + Source + ")";
        }
    }
}