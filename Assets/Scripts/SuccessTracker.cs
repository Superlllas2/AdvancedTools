public static class SuccessTracker
{
    public static int totalEpisodes = 0;
    public static int successfulEpisodes = 0;

    private static readonly SuccessWindow Last1000 = new SuccessWindow(1000);

    public static void RegisterSuccess() => Last1000.Add(1);
    public static void AddEpisodes() => successfulEpisodes++;

    public static float GlobalSuccessRate =>
        totalEpisodes == 0 ? 0f : (float)successfulEpisodes / totalEpisodes * 100f;

    public static float RecentSuccessRate => Last1000.GetRate();
}