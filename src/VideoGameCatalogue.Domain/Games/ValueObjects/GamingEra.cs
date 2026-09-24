namespace VideoGameCatalogue.Domain.Games.ValueObjects;

/// <summary>
/// Domain Value Object encapsulating a historical gaming console generation and era.
/// <para>
/// Provides domain classifications, timeline boundaries, and presentation metadata,
/// projecting rich historical meaning from a normalized release year without database duplication.
/// </para>
/// </summary>
public readonly record struct GamingEra(
    string Key,
    string Generation,
    string Name,
    int StartYear,
    int? EndYear,
    string Icon,
    string BadgeClass,
    string Description)
{
    public static readonly GamingEra EarlyArcade = new(
        "early-arcade",
        "Early Era",
        "Early Arcade & 2nd Gen",
        1950,
        1982,
        "🕹️",
        "bg-secondary-subtle text-secondary-emphasis border border-secondary-subtle",
        "Atari 2600, Intellivision, and early arcade pioneers; dawn of commercial video games.");

    public static readonly GamingEra ThirdGen = new(
        "3rd-gen",
        "3rd Gen",
        "8-Bit Era",
        1983,
        1986,
        "🕹️",
        "bg-info-subtle text-info-emphasis border border-info-subtle",
        "NES and Master System; revitalized the industry with side-scrolling and RPG classics.");

    public static readonly GamingEra FourthGen = new(
        "4th-gen",
        "4th Gen",
        "16-Bit Golden Age",
        1987,
        1992,
        "👾",
        "bg-primary-subtle text-primary-emphasis border border-primary-subtle",
        "SNES, Sega Genesis; golden age of 2D pixel art, mode-7, and landmark JRPGs.");

    public static readonly GamingEra FifthGen = new(
        "5th-gen",
        "5th Gen",
        "3D Revolution",
        1993,
        1997,
        "🎮",
        "bg-danger-subtle text-danger-emphasis border border-danger-subtle",
        "PlayStation, N64, Saturn; leap to full 3D polygonal worlds, CD audio, and analog sticks.");

    public static readonly GamingEra SixthGen = new(
        "6th-gen",
        "6th Gen",
        "128-Bit Era",
        1998,
        2004,
        "💿",
        "bg-warning-subtle text-warning-emphasis border border-warning-subtle",
        "PS2, GameCube, Xbox, Dreamcast; cinematic narratives, open worlds, and console online play.");

    public static readonly GamingEra SeventhGen = new(
        "7th-gen",
        "7th Gen",
        "HD Generation",
        2005,
        2012,
        "📺",
        "bg-dark-subtle text-dark-emphasis border border-dark-subtle",
        "PS3, Xbox 360, Wii; high-definition gaming, wireless controllers, and digital storefronts.");

    public static readonly GamingEra EighthGen = new(
        "8th-gen",
        "8th Gen",
        "Connected Era",
        2013,
        2019,
        "🌐",
        "bg-info-subtle text-info border border-info",
        "PS4, Xbox One, Nintendo Switch; seamless social sharing, 4K HDR, and hybrid handheld gaming.");

    public static readonly GamingEra NinthGen = new(
        "9th-gen",
        "9th Gen",
        "Modern Era",
        2020,
        null,
        "⚡",
        "bg-success-subtle text-success-emphasis border border-success-subtle",
        "PS5, Xbox Series X/S, Modern PC; ultra-fast NVMe SSDs, real-time ray tracing, and 120 FPS.");

    public static readonly IReadOnlyList<GamingEra> All =
    [
        EarlyArcade,
        ThirdGen,
        FourthGen,
        FifthGen,
        SixthGen,
        SeventhGen,
        EighthGen,
        NinthGen
    ];

    public string DisplayTitle =>
        $"{Generation}: {Name} ({StartYear}–{(EndYear.HasValue ? EndYear.Value.ToString() : "Present")})";

    public bool Matches(int year) =>
        year >= StartYear && (!EndYear.HasValue || year <= EndYear.Value);

    public static GamingEra ForYear(int year)
    {
        foreach (var era in All)
        {
            if (era.Matches(year))
                return era;
        }

        return NinthGen;
    }

    public static GamingEra? FromKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var clean = key.Trim();

        // Support aliases like "16-bit", "8-bit", "3d-revolution", "hd-era", "modern"
        if (string.Equals(clean, "16-bit", StringComparison.OrdinalIgnoreCase))
            return FourthGen;
        if (string.Equals(clean, "8-bit", StringComparison.OrdinalIgnoreCase))
            return ThirdGen;
        if (string.Equals(clean, "3d-revolution", StringComparison.OrdinalIgnoreCase))
            return FifthGen;
        if (string.Equals(clean, "hd-era", StringComparison.OrdinalIgnoreCase))
            return SeventhGen;
        if (string.Equals(clean, "modern", StringComparison.OrdinalIgnoreCase))
            return NinthGen;

        foreach (var era in All)
        {
            if (string.Equals(era.Key, clean, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(era.Generation, clean, StringComparison.OrdinalIgnoreCase))
            {
                return era;
            }
        }

        return null;
    }
}
