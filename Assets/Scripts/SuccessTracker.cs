public static class SuccessTracker
{
    private static readonly SuccessWindow Last1000 = new SuccessWindow(1000);

    public static int TotalEpisodes { get; private set; } = 0;
    public static int SuccessfulEpisodes { get; private set; } = 0;

    public static void RecordEpisodeResult(bool succeeded)
    {
        TotalEpisodes++;
        if (succeeded) SuccessfulEpisodes++;

        Last1000.Add(succeeded ? 1 : 0);
    }

    public static float GlobalSuccessRate =>
        TotalEpisodes == 0 ? 0f : (float)SuccessfulEpisodes / TotalEpisodes * 100f;

    public static float RecentSuccessRate => Last1000.Length == 0 ? 0f : Last1000.GetRate();
}
