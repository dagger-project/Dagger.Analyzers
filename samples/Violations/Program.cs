namespace Dagger.Analyzers.Samples.Violations;

public static class Examples
{
    public static async Task DoWork()
    {
        await Task.Yield();
    }

    public static void NotReallyAsync()
    {
    }

    public static async void FireAndForget()
    {
        await Task.Yield();
    }

    public static async Task ForgetToAwaitAsync()
    {
        Task.Delay(1);
        await Task.Yield();
    }

    public static string Cast(object value)
    {
        if (value is string)
        {
            var text = (string)value;
            return text;
        }

        return "other";
    }

    public static void BlockThread()
    {
        Thread.Sleep(1);
        _ = Task.FromResult(1).Result;
    }

    public static void Leak()
    {
        var stream = new MemoryStream();
        stream.WriteByte(1);
    }

    public static int CountItems(int[] items) => items.Count();

    public static int Busy(int n)
    {
        var score = 0;
        if (n > 0) { score++; }
        if (n > 1) { score++; }
        if (n > 2) { score++; }
        if (n > 3) { score++; }
        if (n > 4) { score++; }
        if (n > 5) { score++; }
        if (n > 6) { score++; }
        if (n > 7) { score++; }
        if (n > 8) { score++; }
        if (n > 9) { score++; }
        if (n > 10) { score++; }
        if (n > 11) { score++; }
        if (n > 12) { score++; }
        if (n > 13) { score++; }
        if (n > 14) { score++; }
        if (n > 15) { score++; }
        if (n > 16) { score++; }
        if (n > 17) { score++; }
        if (n > 18) { score++; }
        if (n > 19) { score++; }
        if (n > 20) { score++; }
        if (n > 21) { score++; }
        return score;
    }

    public static void DeeplyNested(bool outer, bool inner)
    {
        if (outer)
        {
            if (inner)
            {
                _ = inner.ToString();
            }
        }
    }
}
