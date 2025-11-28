namespace Kavics.GittLib;

public class SemVer
{
    public static SemVer Default => new();

    public int Major { get; set; }
    public int Minor { get; set; }
    public int Patch { get; set; }
    public string? PreRelease { get; set; }

    public bool IsPreRelease => string.IsNullOrWhiteSpace(PreRelease);

    public static SemVer Parse(string src)
    {
        if (TryParse(src, out var result))
            return result;
        throw new FormatException("Invalid version format");
    }
    public static bool TryParse(string src, out SemVer result)
    {
        result = Default;
        if (string.IsNullOrWhiteSpace(src))
            return false;

        var segments = src.Trim().Split('-');
        if (segments.Length > 2)
            return false;

        if (!TryParseCore(segments[0], out int major, out int minor, out int patch))
            return false;

        result.Major = major;
        result.Minor = minor;
        result.Patch = patch;
        result.PreRelease = segments.Length == 2 ? segments[1] : null;
        return true;
    }
    public static string NextRelease(string semVer)
    {
        return Parse(semVer).NextRelease();
    }

    private static bool TryParseCore(string src, out int major, out int minor, out int patch)
    {
        major = minor = patch = 0;
        var segments = src.Split('.');
        if (segments.Length != 3)
            return false;
        if (!int.TryParse(segments[0], out major))
            return false;
        if (!int.TryParse(segments[1], out minor))
            return false;
        if (!int.TryParse(segments[2], out patch))
            return false;
        return true;
    }

    public string NextRelease()
    {
        return $"{Major}.{Minor}.{Patch}";
    }
    public override string ToString()
    {
        return IsPreRelease
            ? $"{Major}.{Minor}.{Patch}-{PreRelease}"
            : $"{Major}.{Minor}.{Patch}";
    }
}