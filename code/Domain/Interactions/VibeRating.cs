using System;

public class VibeRating
{
    public int Characters { get; }
    public int Plot { get; }
    public int Visuals { get; }
    public int Pacing { get; }
    public int Vibe { get; }
    public int Soundtrack { get; }

    public VibeRating(int characters, int plot, int visuals, int pacing, int vibe, int soundtrack)
    {
        Validate(characters, nameof(characters));
        Validate(plot, nameof(plot));
        Validate(visuals, nameof(visuals));
        Validate(pacing, nameof(pacing));
        Validate(vibe, nameof(vibe));
        Validate(soundtrack, nameof(soundtrack));

        Characters = characters;
        Plot = plot;
        Visuals = visuals;
        Pacing = pacing;
        Vibe = vibe;
        Soundtrack = soundtrack;
    }

    public double CalculateAverage()
    {
        return (Characters + Plot + Visuals + Pacing + Vibe + Soundtrack) / 6.0;
    }

    public bool IsHighlyRated(double threshold)
    {
        return CalculateAverage() >= threshold;
    }

    private static void Validate(int value, string parameterName)
    {
        if (value < 0 || value > 10)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                "Rating must be between 0 and 10.");
        }
    }
}