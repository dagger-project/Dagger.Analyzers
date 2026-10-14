namespace Dagger.Analyzers.Samples.Clean;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var count = await CountAsync(args);
        var tally = new Count(count) { Label = "items" };
        _ = new Count.Draft { Title = tally.Label };
        Console.WriteLine(Describe(tally.Value));
        return 0;
    }

    private static async Task<int> CountAsync(string[] args)
    {
        await Task.Yield();
        return args.Length;
    }

    public static string Describe(object value) => value switch
    {
        string text => text,
        int number => number.ToString(System.Globalization.CultureInfo.InvariantCulture),
        null => "empty",
        _ => "other",
    };

    private sealed record Count(int Value)
    {
        public string Label { get; init; } = "";

        public sealed class Draft
        {
            public string Title { get; set; } = "";
        }
    }
}
