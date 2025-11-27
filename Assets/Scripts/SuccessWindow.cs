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
        int sum = 0;
        for (int i = 0; i < count; i++)
            sum += buffer[i];

        return (float)sum / count;
    }

    public bool IsFull => count == size;
}