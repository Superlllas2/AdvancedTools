public class SuccessWindow
{
    private readonly int size;
    private int index = 0;
    private int count = 0;
    private int[] buffer;

    public SuccessWindow(int size)
    {
        this.size = size;
        buffer = new int[size];
    }

    public void Add(int value)
    {
        buffer[index] = value;
        index = (index + 1) % size;
        if (count < size) count++;
    }

    public float GetRate()
    {
        if (count == 0) return 0f;

        int sum = 0;
        for (int i = 0; i < count; i++)
            sum += buffer[i];

        return (float)sum / count * 100;
    }

    public bool IsFull => count == size;
    public int Length => count;
}