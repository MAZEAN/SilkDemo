namespace Centauri.Utils.Misc;

public static class PathResolver
{
    private static readonly string Root = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

    public static string Resolve(string relativePath) =>
        Path.Combine(Root, relativePath);
}