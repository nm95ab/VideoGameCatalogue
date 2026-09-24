using Microsoft.EntityFrameworkCore;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Games.ValueObjects;
using VideoGameCatalogue.Domain.Lookups;

namespace VideoGameCatalogue.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    private static readonly string[] InitialPlatforms =
    [
        "PC",
        "PlayStation 5",
        "PlayStation 4",
        "PlayStation 3",
        "PlayStation 2",
        "PlayStation",
        "Xbox Series X/S",
        "Xbox One",
        "Xbox 360",
        "Xbox",
        "Nintendo Switch",
        "Wii",
        "GameCube",
        "Nintendo 64",
        "SNES",
        "NES",
        "Game Boy",
        "Game Boy Advance",
        "Nintendo DS",
        "Sega Genesis",
        "Sega Dreamcast",
        "Arcade"
    ];

    private static readonly string[] InitialGenres =
    [
        "Action",
        "Action-Adventure",
        "Role-Playing (RPG)",
        "Strategy",
        "Platformer",
        "Shooter",
        "Simulation",
        "Sports",
        "Racing",
        "Fighting",
        "Puzzle",
        "Survival Horror"
    ];

    private static readonly string[] InitialRatings =
    [
        "Everyone",
        "Everyone 10+",
        "Teen",
        "Mature 17+",
        "Adults Only 18+",
        "Rating Pending"
    ];

    public static async Task SeedAsync(VideoGameCatalogueDbContext context, CancellationToken cancellationToken = default)
    {
        await MigrateSchemaAsync(context, cancellationToken);
        await SeedLookupsAsync(context, cancellationToken);
        await SeedGamesAsync(context, cancellationToken);
    }

    private const string SchemaMigrationSql = @"
        IF EXISTS (SELECT * FROM sys.tables WHERE name = 'VideoGames')
        BEGIN
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('VideoGames') AND name = 'ImageId')
            BEGIN
                ALTER TABLE [VideoGames] ADD [ImageId] NVARCHAR(100) NULL;
            END

            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_VideoGames_Platform' AND object_id = OBJECT_ID('VideoGames'))
            BEGIN
                CREATE NONCLUSTERED INDEX [IX_VideoGames_Platform] ON [VideoGames] ([Platform]);
            END

            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_VideoGames_Genre' AND object_id = OBJECT_ID('VideoGames'))
            BEGIN
                CREATE NONCLUSTERED INDEX [IX_VideoGames_Genre] ON [VideoGames] ([Genre]);
            END

            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_VideoGames_Title' AND object_id = OBJECT_ID('VideoGames'))
            BEGIN
                CREATE NONCLUSTERED INDEX [IX_VideoGames_Title] ON [VideoGames] ([Title]);
            END

            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_VideoGames_Platform_Genre_Title' AND object_id = OBJECT_ID('VideoGames'))
            BEGIN
                CREATE NONCLUSTERED INDEX [IX_VideoGames_Platform_Genre_Title] ON [VideoGames] ([Platform], [Genre], [Title]);
            END
        END";

    public static async Task MigrateSchemaAsync(VideoGameCatalogueDbContext context, CancellationToken cancellationToken = default)
    {
        if (context.Database.IsRelational())
        {
            await context.Database.ExecuteSqlRawAsync(SchemaMigrationSql, cancellationToken);
        }
    }

    public static void MigrateSchema(VideoGameCatalogueDbContext context)
    {
        if (context.Database.IsRelational())
        {
            context.Database.ExecuteSqlRaw(SchemaMigrationSql);
        }
    }

    private static async Task SeedLookupsAsync(VideoGameCatalogueDbContext context, CancellationToken cancellationToken)
    {
        var hasChanges = false;

        var existingPlatforms = await context.Platforms
            .Select(p => p.Name)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        var missingPlatforms = InitialPlatforms
            .Where(p => !existingPlatforms.Contains(p))
            .Select(p => new PlatformLookup(p))
            .ToList();

        if (missingPlatforms.Count > 0)
        {
            await context.Platforms.AddRangeAsync(missingPlatforms, cancellationToken);
            hasChanges = true;
        }

        var existingGenres = await context.Genres
            .Select(g => g.Name)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        var missingGenres = InitialGenres
            .Where(g => !existingGenres.Contains(g))
            .Select(g => new GenreLookup(g))
            .ToList();

        if (missingGenres.Count > 0)
        {
            await context.Genres.AddRangeAsync(missingGenres, cancellationToken);
            hasChanges = true;
        }

        var existingRatings = await context.Ratings
            .Select(r => r.Name)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        var missingRatings = InitialRatings
            .Where(r => !existingRatings.Contains(r))
            .Select(r => new RatingLookup(r))
            .ToList();

        if (missingRatings.Count > 0)
        {
            await context.Ratings.AddRangeAsync(missingRatings, cancellationToken);
            hasChanges = true;
        }

        if (hasChanges)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task SeedGamesAsync(VideoGameCatalogueDbContext context, CancellationToken cancellationToken)
    {
        var existingTitles = await context.VideoGames
            .Select(g => g.Title.Value)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, cancellationToken);

        var allGames = GetPopularGames();
        var missingGames = allGames
            .Where(g => !existingTitles.Contains(g.Title.Value))
            .ToList();

        if (missingGames.Count > 0)
        {
            await context.VideoGames.AddRangeAsync(missingGames, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static List<VideoGame> GetPopularGames()
    {
        return
        [
            CreateGame("The Witcher 3: Wild Hunt", "PC", "Role-Playing (RPG)", 2015, "Mature 17+", "Geralt of Rivia searches for his adopted daughter Ciri in a massive, war-torn fantasy open world."),
            CreateGame("The Legend of Zelda: Breath of the Wild", "Nintendo Switch", "Action-Adventure", 2017, "Everyone 10+", "Link wakes up after a century to discover Hyrule in ruins and must defeat Calamity Ganon in a vast open world."),
            CreateGame("Elden Ring", "PlayStation 5", "Action", 2022, "Mature 17+", "A dark fantasy action-RPG set in the Lands Between, created by Hidetaka Miyazaki and George R. R. Martin."),
            CreateGame("Super Mario World", "SNES", "Platformer", 1990, "Everyone", "Mario and Luigi venture into Dinosaur Land to rescue Princess Peach and defeat Bowser with Yoshi's help."),
            CreateGame("Chrono Trigger", "SNES", "Role-Playing (RPG)", 1995, "Everyone", "Legendary time-travel RPG developed by the 'Dream Team' of Hironobu Sakaguchi, Yuji Horii, and Akira Toriyama."),
            CreateGame("Portal 2", "PC", "Puzzle", 2011, "Everyone 10+", "Innovative puzzle-platformer set in the Aperture Science laboratories featuring Chell, GLaDOS, and Wheatley."),
            CreateGame("Halo: Combat Evolved", "Xbox", "Shooter", 2001, "Mature 17+", "Master Chief and Cortana battle the alien Covenant alliance across a mysterious ancient ringworld."),
            CreateGame("Red Dead Redemption 2", "PlayStation 4", "Action-Adventure", 2018, "Mature 17+", "Arthur Morgan and the Van der Linde gang struggle to survive in the twilight of the American Wild West."),
            CreateGame("Tetris", "Game Boy", "Puzzle", 1989, "Everyone", "The iconic tile-matching puzzle game created by Alexey Pajitnov that defined handheld gaming worldwide."),
            CreateGame("Super Mario Bros.", "NES", "Platformer", 1985, "Everyone", "Shigeru Miyamoto's landmark platformer that revitalized the home console industry and introduced the Mushroom Kingdom."),
            CreateGame("Super Mario 64", "Nintendo 64", "Platformer", 1996, "Everyone", "Pioneering 3D platformer that established the camera and movement paradigms for three-dimensional video games."),
            CreateGame("The Legend of Zelda: Ocarina of Time", "Nintendo 64", "Action-Adventure", 1998, "Everyone", "Epic time-travel adventure often acclaimed as one of the greatest and most influential video games ever created."),
            CreateGame("The Legend of Zelda: A Link to the Past", "SNES", "Action-Adventure", 1991, "Everyone", "The definitive 16-bit top-down adventure featuring dual Light and Dark Worlds across the kingdom of Hyrule."),
            CreateGame("Grand Theft Auto V", "PlayStation 4", "Action-Adventure", 2013, "Mature 17+", "Record-shattering open-world epic following Michael, Franklin, and Trevor through modern Southern California."),
            CreateGame("Grand Theft Auto: San Andreas", "PlayStation 2", "Action-Adventure", 2004, "Mature 17+", "CJ returns home to Los Santos in the early 90s, climbing through gang wars and corrupt systems across three cities."),
            CreateGame("Grand Theft Auto: Vice City", "PlayStation 2", "Action-Adventure", 2002, "Mature 17+", "Tommy Vercetti builds a criminal empire in neon-soaked, 1980s Miami-inspired Vice City."),
            CreateGame("Minecraft", "PC", "Simulation", 2011, "Everyone 10+", "The best-selling sandbox phenomenon allowing players to build, mine, explore, and survive in procedurally generated worlds."),
            CreateGame("Half-Life 2", "PC", "Shooter", 2004, "Mature 17+", "Gordon Freeman leads a dystopian resistance against the trans-dimensional Combine using the Gravity Gun."),
            CreateGame("Half-Life", "PC", "Shooter", 1998, "Mature 17+", "Revolutionary seamless narrative shooter following theoretical physicist Gordon Freeman through the Black Mesa incident."),
            CreateGame("Final Fantasy VII", "PlayStation", "Role-Playing (RPG)", 1997, "Teen", "Cloud Strife and eco-terrorist group AVALANCHE fight the Shinra megacorporation and the rogue warrior Sephiroth."),
            CreateGame("Final Fantasy VI", "SNES", "Role-Playing (RPG)", 1994, "Everyone", "Ensemble steampunk fantasy RPG revolving around Terra Branford and an empire seeking to weaponize ancient espers."),
            CreateGame("Final Fantasy X", "PlayStation 2", "Role-Playing (RPG)", 2001, "Teen", "Tidus and summoner Yuna embark on a sacred pilgrimage to defeat the monstrous entity Sin in the world of Spira."),
            CreateGame("The Elder Scrolls V: Skyrim", "PC", "Role-Playing (RPG)", 2011, "Mature 17+", "Open-world fantasy epic where the Dragonborn learns the Thu'um and fights Alduin the World-Eater across Skyrim."),
            CreateGame("The Elder Scrolls III: Morrowind", "PC", "Role-Playing (RPG)", 2002, "Teen", "Deep, alien open-world RPG set on Vvardenfell where the Nerevarine fulfills ancient Dunmer prophecies."),
            CreateGame("Dark Souls", "PlayStation 3", "Role-Playing (RPG)", 2011, "Mature 17+", "Uncompromising dark fantasy RPG that spawned the Soulslike genre with inter-connected level design and deep lore."),
            CreateGame("Bloodborne", "PlayStation 4", "Action", 2015, "Mature 17+", "Fast-paced Lovecraftian gothic action-RPG through the diseased streets of Yharnam during the Hunt."),
            CreateGame("BioShock", "PC", "Shooter", 2007, "Mature 17+", "Atmospheric shooter exploring the underwater Objectivist dystopia of Rapture created by Andrew Ryan."),
            CreateGame("Mass Effect 2", "Xbox 360", "Role-Playing (RPG)", 2010, "Mature 17+", "Commander Shepard recruits an elite galaxy-spanning crew for a perilous suicide mission against the Collectors."),
            CreateGame("The Last of Us", "PlayStation 3", "Action-Adventure", 2013, "Mature 17+", "Harrowing post-pandemic journey across America following Joel and Ellie against infected and ruthless survivors."),
            CreateGame("The Last of Us Part II", "PlayStation 4", "Action-Adventure", 2020, "Mature 17+", "Intense emotional exploration of revenge, grief, and empathy across the overgrown ruins of Seattle."),
            CreateGame("God of War", "PlayStation 4", "Action-Adventure", 2018, "Mature 17+", "Kratos and his young son Atreus embark on a mythic Norse odyssey to scatter his wife's ashes atop the highest peak."),
            CreateGame("God of War Ragnarök", "PlayStation 5", "Action-Adventure", 2022, "Mature 17+", "Epic culmination of the Norse saga as Kratos and Atreus confront the Aesir gods during the twilight of the realms."),
            CreateGame("Metal Gear Solid", "PlayStation", "Action-Adventure", 1998, "Mature 17+", "Tactical espionage action masterpiece where Solid Snake infiltrates Shadow Moses to stop nuclear terrorists."),
            CreateGame("Metal Gear Solid 3: Snake Eater", "PlayStation 2", "Action-Adventure", 2004, "Mature 17+", "Cold War wilderness stealth prequel following Naked Snake on a mission to eliminate his defected mentor, The Boss."),
            CreateGame("Resident Evil 4", "GameCube", "Survival Horror", 2005, "Mature 17+", "Over-the-shoulder action-horror revolution where Leon S. Kennedy rescues the president's daughter in rural Spain."),
            CreateGame("Resident Evil 2", "PlayStation", "Survival Horror", 1998, "Mature 17+", "Dual-protagonist survival horror epic during the catastrophic viral outbreak in Raccoon City."),
            CreateGame("Silent Hill 2", "PlayStation 2", "Survival Horror", 2001, "Mature 17+", "Psychological horror masterpiece exploring guilt, trauma, and grief in the fog-drenched town of Silent Hill."),
            CreateGame("Super Metroid", "SNES", "Action-Adventure", 1994, "Everyone", "Atmospheric 2D exploration classic that defined the Metroidvania genre on planet Zebes."),
            CreateGame("Metroid Prime", "GameCube", "Shooter", 2002, "Teen", "Flawless first-person translation of Metroid mechanics onto the ruined Chozo world of Tallon IV."),
            CreateGame("Castlevania: Symphony of the Night", "PlayStation", "Action-Adventure", 1997, "Teen", "Alucard explores Dracula's ever-shifting castle in the non-linear action platformer that birthed 'Igavania'."),
            CreateGame("Street Fighter II", "Arcade", "Fighting", 1991, "Teen", "The competitive fighting game cornerstone that popularized combos, special inputs, and global arcade tournaments."),
            CreateGame("Super Smash Bros. Melee", "GameCube", "Fighting", 2001, "Teen", "Electrifyingly fast Nintendo crossover fighter celebrated for its emergent physics, wave-dashing, and competitive scene."),
            CreateGame("Super Smash Bros. Ultimate", "Nintendo Switch", "Fighting", 2018, "Everyone 10+", "The monumental celebration of gaming history bringing together every fighter in series history onto one roster."),
            CreateGame("Tekken 3", "PlayStation", "Fighting", 1997, "Teen", "The pinnacle of 3D polygon fighting with side-stepping, 60fps responsiveness, and characters like Jin Kazama."),
            CreateGame("DOOM", "PC", "Shooter", 1993, "Mature 17+", "id Software's groundbreaking shareware FPS that unleashed fast-paced demon slaying and deathmatching upon the world."),
            CreateGame("DOOM Eternal", "PC", "Shooter", 2020, "Mature 17+", "High-speed combat puzzle FPS demanding resource management, glory kills, and relentless aggression against Hell."),
            CreateGame("World of Warcraft", "PC", "Role-Playing (RPG)", 2004, "Teen", "Cultural juggernaut MMORPG that brought millions of players into Azeroth for raiding, dungeon crawling, and guild play."),
            CreateGame("Diablo II", "PC", "Role-Playing (RPG)", 2000, "Mature 17+", "Addictive isometric action-RPG setting the gold standard for loot grinding, skill trees, and dark gothic ambiance."),
            CreateGame("StarCraft", "PC", "Strategy", 1998, "Teen", "Terran, Zerg, and Protoss battle in the masterfully balanced RTS that became a national esport in South Korea."),
            CreateGame("Age of Empires II: The Age of Kings", "PC", "Strategy", 1999, "Teen", "Beloved historical real-time strategy encompassing medieval civilizations, castle sieges, and empire management."),
            CreateGame("Civilization IV", "PC", "Strategy", 2005, "Everyone 10+", "Turn-based grand strategy masterpiece tracking human history from ancient times to the space race with Baba Yetu."),
            CreateGame("Civilization VI", "PC", "Strategy", 2016, "Everyone 10+", "Unstacked city districts and climate dynamics elevate the legendary 4X franchise to new strategic depths."),
            CreateGame("Baldur's Gate II: Shadows of Amn", "PC", "Role-Playing (RPG)", 2000, "Teen", "BioWare's sprawling AD&D 2nd Edition epic featuring villain Jon Irenicus and unparalleled companion depth."),
            CreateGame("Baldur's Gate 3", "PC", "Role-Playing (RPG)", 2023, "Mature 17+", "Larian Studios' generational D&D 5E role-playing masterpiece with astonishing narrative reactivity and turn-based combat."),
            CreateGame("Disco Elysium", "PC", "Role-Playing (RPG)", 2019, "Mature 17+", "Literary detective RPG with intricate psychological dialog trees and zero combat in the decaying city of Revachol."),
            CreateGame("Persona 5 Royal", "PlayStation 4", "Role-Playing (RPG)", 2019, "Mature 17+", "Stylish Tokyo high school life-sim meets dungeon crawling as the Phantom Thieves steal corrupted hearts."),
            CreateGame("Persona 4 Golden", "PC", "Role-Playing (RPG)", 2012, "Mature 17+", "Murder mystery JRPG in the rural town of Inaba blending social links, weather forecasts, and Midnight Channel dungeons."),
            CreateGame("Pokémon Red and Blue", "Game Boy", "Role-Playing (RPG)", 1996, "Everyone", "Satoshi Tajiri's creature-collecting phenomenon that sparked a global multimedia franchise with 151 original monsters."),
            CreateGame("Pokémon Gold and Silver", "Game Boy", "Role-Playing (RPG)", 1999, "Everyone", "Monumental sequel introducing Johto, breeding, day-and-night cycles, and a surprise journey back through Kanto."),
            CreateGame("Sonic the Hedgehog 2", "Sega Genesis", "Platformer", 1992, "Everyone", "High-speed 16-bit perfection introducing Tails, the Spin Dash, and the legendary Chemical Plant Zone."),
            CreateGame("Sonic the Hedgehog", "Sega Genesis", "Platformer", 1991, "Everyone", "Blisteringly fast platformer with attitude that put the Sega Genesis on the map and rivaled Nintendo."),
            CreateGame("Donkey Kong Country", "SNES", "Platformer", 1994, "Everyone", "Rare's groundbreaking pre-rendered Silicon Graphics visuals paired with David Wise's atmospheric soundtrack."),
            CreateGame("Donkey Kong Country 2: Diddy's Kong Quest", "SNES", "Platformer", 1995, "Everyone", "Flawlessly tuned pirate-themed platforming with Diddy and Dixie Kong rescuing Donkey Kong from King K. Rool."),
            CreateGame("Mega Man 2", "NES", "Platformer", 1988, "Everyone", "Capcom's action platforming pinnacle with iconic robot masters, the Metal Blade, and legendary chiptune tracks."),
            CreateGame("Mega Man X", "SNES", "Platformer", 1993, "Everyone", "Mature evolution of Mega Man introducing dashing, wall-kicking, armor upgrades, and the enigmatic Zero."),
            CreateGame("GoldenEye 007", "Nintendo 64", "Shooter", 1997, "Teen", "Rare's genre-defining spy shooter that proved competitive four-player split-screen FPS belonged on consoles."),
            CreateGame("Deus Ex", "PC", "Role-Playing (RPG)", 2000, "Mature 17+", "Ion Storm's immersive sim granting unprecedented player agency to shoot, hack, sneak, or talk through conspiracies."),
            CreateGame("System Shock 2", "PC", "Shooter", 1999, "Mature 17+", "Chilling cyberpunk horror immersive sim pitting the player against rogue artificial intelligence SHODAN."),
            CreateGame("Thief II: The Metal Age", "PC", "Action-Adventure", 2000, "Mature 17+", "Looking Glass Studios' definitive stealth simulator celebrating light, shadows, rope arrows, and sound propagation."),
            CreateGame("Shadow of the Colossus", "PlayStation 2", "Action-Adventure", 2005, "Teen", "Fumito Ueda's poetic adventure where Wander scales sixteen gargantuan beasts to resurrect a lost maiden."),
            CreateGame("ICO", "PlayStation 2", "Action-Adventure", 2001, "Teen", "Minimalist masterpiece emphasizing hand-holding, castle architecture, and emotional connection with Yorda."),
            CreateGame("Okami", "PlayStation 2", "Action-Adventure", 2006, "Teen", "Clover Studio's sumi-e watercolor wonder where sun goddess Amaterasu restores life using the Celestial Brush."),
            CreateGame("Undertale", "PC", "Role-Playing (RPG)", 2015, "Everyone 10+", "Toby Fox's subversively warm indie RPG where you can defeat, spare, or befriend every monster in the Underground."),
            CreateGame("Hades", "PC", "Action", 2020, "Teen", "Supergiant's rogue-lite masterpiece merging razor-sharp hack-and-slash combat with episodic Greek myth narrative."),
            CreateGame("Hollow Knight", "PC", "Action-Adventure", 2017, "Everyone 10+", "Team Cherry's sprawling hand-drawn Metroidvania through Hallownest, a vast forgotten bug kingdom."),
            CreateGame("Celeste", "Nintendo Switch", "Platformer", 2018, "Everyone 10+", "Precision platforming triumph addressing mental health and anxiety as Madeline scales the mystical Celeste Mountain."),
            CreateGame("Journey", "PlayStation 3", "Action-Adventure", 2012, "Everyone", "thatgamecompany's wordless emotional pilgrimage across golden sands with anonymous, harmonious co-op companions."),
            CreateGame("Team Fortress 2", "PC", "Shooter", 2007, "Mature 17+", "Valve's charismatic class-based team shooter featuring iconic 1960s spy-tech cartoon aesthetics and rocket jumping."),
            CreateGame("Counter-Strike: Global Offensive", "PC", "Shooter", 2012, "Mature 17+", "Tactical five-versus-five bomb defusal shooter that became one of the biggest competitive esports in gaming history."),
            CreateGame("Left 4 Dead 2", "PC", "Shooter", 2009, "Mature 17+", "Cooperative zombie survival perfection driven by the dynamic AI Director through southern swamps and cityscapes."),
            CreateGame("Call of Duty 4: Modern Warfare", "Xbox 360", "Shooter", 2007, "Mature 17+", "Infinity Ward's revolutionary FPS that standardized modern military shooter campaigns and perk-based multiplayer."),
            CreateGame("Call of Duty: Modern Warfare 2", "Xbox 360", "Shooter", 2009, "Mature 17+", "Blockbuster sequel featuring explosive set-pieces, iconic Spec Ops missions, and frenetic multiplayer on Terminal."),
            CreateGame("Uncharted 2: Among Thieves", "PlayStation 3", "Action-Adventure", 2009, "Teen", "Naughty Dog's cinematic tour de force sending Nathan Drake across Nepal and the Himalayas in pursuit of Shambhala."),
            CreateGame("Uncharted 4: A Thief's End", "PlayStation 4", "Action-Adventure", 2016, "Teen", "Grown-up, visually breathtaking pirate treasure hunt resolving Nathan Drake's life-long obsession with adventure."),
            CreateGame("Batman: Arkham City", "PlayStation 3", "Action-Adventure", 2011, "Teen", "Rocksteady's ultimate Batman simulator combining freeflow combat, predator stealth, and open-air gliding over Gotham."),
            CreateGame("Batman: Arkham Asylum", "PlayStation 3", "Action-Adventure", 2009, "Teen", "Tense Metroidvania-style superhero adventure trapping Batman inside the notorious asylum with the Joker."),
            CreateGame("Horizon Zero Dawn", "PlayStation 4", "Action-Adventure", 2017, "Teen", "Aloy unravels the mystery of her origins in a breathtaking post-post-apocalyptic Earth ruled by animalistic machines."),
            CreateGame("Marvel's Spider-Man", "PlayStation 4", "Action-Adventure", 2018, "Teen", "Insomniac's love letter to Peter Parker featuring exhilarating web-swinging physics across Manhattan."),
            CreateGame("Star Wars: Knights of the Old Republic", "Xbox", "Role-Playing (RPG)", 2003, "Teen", "BioWare's Star Wars RPG classic set 4,000 years before the Empire with the legendary Darth Revan twist."),
            CreateGame("Fallout: New Vegas", "PC", "Role-Playing (RPG)", 2010, "Mature 17+", "Obsidian's branching Mojave wasteland RPG exploring competing sociopolitical factions in post-apocalyptic Nevada."),
            CreateGame("Fallout 3", "PC", "Role-Playing (RPG)", 2008, "Mature 17+", "Bethesda's transition of the Fallout universe to a first-person Capital Wasteland using the iconic V.A.T.S. system."),
            CreateGame("The Sims", "PC", "Simulation", 2000, "Teen", "Will Wright's landmark suburban life simulator that captivated mainstream audiences worldwide."),
            CreateGame("Pac-Man", "Arcade", "Action", 1980, "Everyone", "Toru Iwatani's universally beloved maze chase game that established the first worldwide video game mascot."),
            CreateGame("Space Invaders", "Arcade", "Shooter", 1978, "Everyone", "Tomohiro Nishikado's seminal fixed shooter that initiated the golden age of arcade video games."),
            CreateGame("Galaga", "Arcade", "Shooter", 1981, "Everyone", "Namco's definitive fixed shooter featuring tractor-beam dual-fighter mechanics and challenging alien swarm stages."),
            CreateGame("Donkey Kong", "Arcade", "Platformer", 1981, "Everyone", "Nintendo's breakthrough platformer designed by Shigeru Miyamoto introducing Jumpman, Pauline, and Donkey Kong."),
            CreateGame("Super Mario Galaxy", "Wii", "Platformer", 2007, "Everyone", "Mind-bending spherical gravity platformer set in the cosmos with an orchestral score and boundless creativity."),
            CreateGame("Super Mario Odyssey", "Nintendo Switch", "Platformer", 2017, "Everyone 10+", "Globe-trotting sandbox platformer where Mario captures dozens of creatures and objects using Cappy."),
            CreateGame("Mario Kart 8 Deluxe", "Nintendo Switch", "Racing", 2017, "Everyone", "The undisputed pinnacle of arcade kart racing featuring anti-gravity tracks and decades of Nintendo racers."),
            CreateGame("Mario Kart 64", "Nintendo 64", "Racing", 1996, "Everyone", "Four-player couch multiplayer pioneer featuring Rainbow Road, blue shells, and unforgettable Balloon Battles."),
            CreateGame("Gran Turismo 3: A-Spec", "PlayStation 2", "Racing", 2001, "Everyone", "Polyphony Digital's automotive showcase that pushed the PS2 hardware and redefined realistic driving simulations."),
            CreateGame("Forza Horizon 5", "Xbox Series X/S", "Racing", 2021, "Everyone", "Playground Games' joyful, vibrant open-world celebration of driving set across the diverse landscapes of Mexico."),
            CreateGame("Tony Hawk's Pro Skater 2", "PlayStation", "Sports", 2000, "Teen", "The ultimate skateboarding arcade game featuring manual combos, create-a-park, and an era-defining soundtrack."),
            CreateGame("Overwatch", "PC", "Shooter", 2016, "Teen", "Blizzard's vibrant hero shooter marrying MOBA-style character abilities with high-tempo first-person action.")
        ];
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
