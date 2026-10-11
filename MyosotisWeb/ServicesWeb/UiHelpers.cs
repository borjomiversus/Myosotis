namespace MyosotisWeb.ServicesWeb
{
    public static class UiHelpers
    {
        private const string Gradient = "linear-gradient(135deg, #332D57, #1A2235)";

        public static string CardBackground(string? posterUrl)
        {
            if (string.IsNullOrWhiteSpace(posterUrl))
                return $"background-image: {Gradient}";

            return $"background-image: url('{posterUrl}'), {Gradient}";
        }

        public static string FormatMinutes(int minutes)
        {
            if (minutes < 60)
                return $"{minutes}m";

            return $"{minutes / 60}h {minutes % 60:00}m";
        }
    }
}
