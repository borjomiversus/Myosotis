using System;

public enum CaptureStatus
{
    Unresolved,
    Resolved,
    Archived
}

public enum CaptureKind
{
    Thought,
    Quote,
    Scene,
    Note
}

// Розбір таймкоду "mm:ss" або "h:mm:ss" у кількість секунд.
public static class TimecodeParser
{
    public static bool TryParse(string? text, out int seconds)
    {
        seconds = 0;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        string[] parts = text.Trim().Split(':');
        if (parts.Length < 2 || parts.Length > 3)
            return false;

        foreach (string part in parts)
        {
            int number;
            if (!int.TryParse(part, out number) || number < 0)
                return false;
            seconds = seconds * 60 + number;
        }
        return true;
    }
}

public class CaptureEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string RawNote { get; private set; }
    public string? Source { get; private set; }
    public DateTime CapturedAt { get; private set; }
    public CaptureStatus Status { get; private set; }
    public Media? ResolvedMedia { get; private set; }
    public CaptureKind Kind { get; private set; }
    public string? Timecode { get; private set; }   
    public string? Annotation { get; private set; }
    public List<string> Tags { get; private set; }

    public CaptureEntry(string rawNote, string? source, CaptureKind kind = CaptureKind.Thought)
    {
        if (string.IsNullOrWhiteSpace(rawNote))
            throw new ArgumentException("Нотатка не може бути порожньою.", nameof(rawNote));

        RawNote = rawNote;
        Source = source;
        Kind = kind;
        CapturedAt = DateTime.Now;
        Status = CaptureStatus.Unresolved;
        Tags = new List<string>();
    }

    public void Resolve(Media identifiedMedia)
    {
        ResolvedMedia = identifiedMedia ?? throw new ArgumentNullException(nameof(identifiedMedia));
        Status = CaptureStatus.Resolved;
    }

    public void Archive() => Status = CaptureStatus.Archived;

    public void SetKind(CaptureKind kind)
    {
        Kind = kind;
    }

    public bool TrySetTimecode(string? timecode)
    {
        int seconds;
        if (!TimecodeParser.TryParse(timecode, out seconds))
            return false;

        Timecode = timecode!.Trim();
        return true;
    }

    public void SetAnnotation(string? text)
    {
        Annotation = string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }

    public void AddTag(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return;

        string clean = tag.Trim().TrimStart('#').Trim().ToLowerInvariant();
        if (clean == "" || Tags.Contains(clean)) return;

        Tags.Add(clean);
    }


    public string GetStatusSummary()
    {
        if (Status == CaptureStatus.Resolved)
            return "Розпізнано: " + ResolvedMedia!.Title + " (нотатка: " + RawNote + ")";
        if (Status == CaptureStatus.Archived)
            return "[архів] " + RawNote;
        return "Не розпізнано: " + RawNote + " (джерело: " + Source + ")";
    }
}






