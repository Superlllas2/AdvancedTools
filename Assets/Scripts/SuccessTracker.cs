using System.Collections.Generic;

public static class SuccessTracker
{
    private static int window = 2000;
    private static int[] successes = new int[window];
    
    private static int index = 0;
    private static bool filled = false;

    public static void Add(int value)
    {
        successes[index] = value;
        index = (index + 1) % window;
        if (index == 0) filled = true;
    }

    public static float GetRate()
    {
        var count = filled ? window : index;
        
        if (count == 0) return 0f;
        
        var zeros = 0;
        var ones = 0;
        
        for (var i = 0; i < count; i++)
        {
            if (successes[i] == 0) zeros++;
            else if (successes[i] == 1) ones++;
        }

        if (zeros == 0) return 0f;

        return (float)ones / zeros * 100f;
    }
}