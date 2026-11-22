public static class TimecodeHelper
{
    public static string FormatInput(string? rawInput)
    {
        var raw = rawInput ?? "";
        var digits = new string(raw.Where(char.IsDigit).ToArray());

        if (digits.Length > 6)
        {
            digits = digits.Substring(0, 6);
        }

        if (digits.Length >= 5)
        {
            return $"{digits.Substring(0, 2)}:{digits.Substring(2, 2)}:{digits.Substring(4)}";
        }
        else if (digits.Length == 4)
        {
            return $"{digits.Substring(0, 2)}:{digits.Substring(2, 2)}";
        }
        else if (digits.Length == 3)
        {
            return $"{digits.Substring(0, 2)}:{digits.Substring(2)}";
        }
        else
        {
            return digits;
        }
    }

    public static string FormatMinutesAsTimecode(int totalMinutes)
    {
        int hours = totalMinutes / 60;
        int mins = totalMinutes % 60;
        return hours > 0 ? $"{hours:D2}:{mins:D2}:00" : $"{mins:D2}:00";
    }

    public static bool TryParseSeconds(string tc, out int totalSeconds, out string error)
    {
        totalSeconds = 0;
        error = "";

        var parts = tc.Split(':');
        if (parts.Length is < 2 or > 3)
        {
            error = "Enter timecode as MM:SS or HH:MM:SS.";
            return false;
        }

        if (parts.Length == 2)
        {
            if (!int.TryParse(parts[0], out int mins) || !int.TryParse(parts[1], out int secs) || secs >= 60 || mins < 0 || secs < 0)
            {
                error = "Invalid timecode. Seconds must be between 00 and 59.";
                return false;
            }
            totalSeconds = mins * 60 + secs;
            return true;
        }
        else
        {
            if (!int.TryParse(parts[0], out int hours) || !int.TryParse(parts[1], out int mins) || !int.TryParse(parts[2], out int secs)
                || hours < 0 || mins < 0 || mins >= 60 || secs < 0 || secs >= 60)
            {
                error = "Invalid timecode. Minutes and seconds must be between 00 and 59.";
                return false;
            }
            totalSeconds = hours * 3600 + mins * 60 + secs;
            return true;
        }
    }
}