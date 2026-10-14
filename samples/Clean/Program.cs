namespace Dagger.Analyzers.Samples.Clean;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var count = await CountAsync(args);
        Console.WriteLine(Describe(count));
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
}
