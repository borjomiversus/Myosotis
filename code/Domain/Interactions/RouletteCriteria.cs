using System;
using System.Collections.Generic;

public class RouletteCriteria
{
    public int MaxMinutes { get; private set; }
    public string? Tag { get; private set; }
    public RouletteCriteria(int maxMinutes, string? tag = null)
    {
        if (maxMinutes < 0) throw new ArgumentOutOfRangeException(nameof(maxMinutes));
        MaxMinutes = maxMinutes;
        Tag = tag;
    }
}

