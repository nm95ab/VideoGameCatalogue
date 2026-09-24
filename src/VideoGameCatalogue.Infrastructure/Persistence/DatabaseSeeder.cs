using Microsoft.EntityFrameworkCore;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Games.ValueObjects;

namespace VideoGameCatalogue.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(VideoGameCatalogueDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.VideoGames.AnyAsync(cancellationToken))
            return;

        var games = new List<VideoGame>
        {
            CreateGame(
                "The Witcher 3: Wild Hunt",
                "PC",
                "Role-Playing (RPG)",
                2015,
                "Mature 17+",
                "Geralt of Rivia searches for his adopted daughter Ciri in a massive, war-torn fantasy open world."),
            CreateGame(
                "The Legend of Zelda: Breath of the Wild",
                "Nintendo Switch",
                "Action-Adventure",
                2017,
                "Everyone 10+",
                "Link wakes up after a century to discover Hyrule in ruins and must defeat Calamity Ganon."),
            CreateGame(
                "Elden Ring",
                "PlayStation 5",
                "Action",
                2022,
                "Mature 17+",
                "A dark fantasy action-RPG created by Hidetaka Miyazaki and George R. R. Martin."),
            CreateGame(
                "Super Mario World",
                "SNES",
                "Platformer",
                1990,
                "Everyone",
                "Mario and Luigi venture into Dinosaur Land to rescue Princess Peach and defeat Bowser."),
            CreateGame(
                "Chrono Trigger",
                "SNES",
                "Role-Playing (RPG)",
                1995,
                "Everyone",
                "Legendary time-travel RPG developed by the 'Dream Team' of Hironobu Sakaguchi, Yuji Horii, and Akira Toriyama."),
            CreateGame(
                "Portal 2",
                "PC",
                "Puzzle",
                2011,
                "Everyone 10+",
                "Innovative puzzle-platformer set in the Aperture Science laboratories with GLaDOS and Wheatley."),
            CreateGame(
                "Halo: Combat Evolved",
                "Xbox Series X/S",
                "Shooter",
                2001,
                "Mature 17+",
                "Master Chief and Cortana battle the alien Covenant alliance across a mysterious ringworld."),
            CreateGame(
                "Red Dead Redemption 2",
                "PlayStation 4",
                "Action-Adventure",
                2018,
                "Mature 17+",
                "Arthur Morgan and the Van der Linde gang struggle to survive in the twilight of the Wild West era.")
        };

        await context.VideoGames.AddRangeAsync(games, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static VideoGame CreateGame(
        string title,
        string platform,
        string genre,
        int releaseYear,
        string rating,
        string description)
    {
        return VideoGame.Create(
            GameTitle.Create(title).Value,
            Platform.Create(platform).Value,
            Genre.Create(genre).Value,
            ReleaseYear.Create(releaseYear).Value,
            Rating.Create(rating).Value,
            description).Value;
    }
}
