using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VideoGameCatalogue.Application;
using VideoGameCatalogue.Application.Games;
using VideoGameCatalogue.Application.Games.DTOs;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Games.ValueObjects;
using VideoGameCatalogue.Domain.Ports;
using VideoGameCatalogue.Infrastructure;
using VideoGameCatalogue.Infrastructure.Persistence;

namespace VideoGameCatalogue.UnitTests.Infrastructure;

public sealed class DependencyInjectionAndSeederTests
{
    [Fact]
    public void AddApplication_RegistersExpectedServices()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        var provider = services.BuildServiceProvider();
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IVideoGameService));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_WithInMemory_RegistersDbContextAndRepository()
    {
        var services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "UseInMemoryDatabase", "true" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        services.AddInfrastructure(configuration);

        var repoDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IVideoGameRepository));
        repoDescriptor.Should().NotBeNull();
        repoDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_WithSqlServer_RegistersSqlServerDbContext()
    {
        var services = new ServiceCollection();
        var sqlSettings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", "Server=127.0.0.1;Database=TestDb;User Id=sa;Password=Pass123!;TrustServerCertificate=True" },
            { "UseInMemoryDatabase", "false" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(sqlSettings)
            .Build();

        services.AddInfrastructure(configuration);

        var repoDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IVideoGameRepository));
        repoDescriptor.Should().NotBeNull();
        repoDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public async Task DatabaseSeeder_SeedsGames_WhenEmpty_AndSkips_WhenAlreadySeeded()
    {
        var connectionString = "Server=127.0.0.1,1433;Database=VideoGameCatalogueSeederTestsDb;User Id=sa;Password=YourStrong@Password123!;TrustServerCertificate=True;MultipleActiveResultSets=true;Connect Timeout=15";
        var options = new DbContextOptionsBuilder<VideoGameCatalogueDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        using var context = new VideoGameCatalogueDbContext(options);
        await context.Database.EnsureCreatedAsync();
        await context.VideoGames.ExecuteDeleteAsync();

        // Act 1: Initial seed on empty database
        await DatabaseSeeder.SeedAsync(context);
        var countAfterFirstSeed = await context.VideoGames.CountAsync();
        countAfterFirstSeed.Should().Be(8);

        // Act 2: Second run should skip seeding
        await DatabaseSeeder.SeedAsync(context);
        var countAfterSecondSeed = await context.VideoGames.CountAsync();
        countAfterSecondSeed.Should().Be(8);

        await context.Database.EnsureDeletedAsync();
    }

    [Fact]
    public void GameDto_FromDomain_MapsAllPropertiesCorrectly()
    {
        var game = VideoGame.Create(
            GameTitle.Create("Metroid Dread").Value,
            Platform.Create("Nintendo Switch").Value,
            Genre.Create("Action-Adventure").Value,
            ReleaseYear.Create(2021).Value,
            Rating.Create("Teen").Value,
            "Samus on Planet ZDR").Value;

        var dto = GameDto.FromDomain(game);

        dto.Id.Should().Be(game.Id);
        dto.Title.Should().Be("Metroid Dread");
        dto.Platform.Should().Be("Nintendo Switch");
        dto.Genre.Should().Be("Action-Adventure");
        dto.ReleaseYear.Should().Be(2021);
        dto.Rating.Should().Be("Teen");
        dto.Description.Should().Be("Samus on Planet ZDR");
        dto.CreatedAtUtc.Should().Be(game.CreatedAtUtc);
        dto.UpdatedAtUtc.Should().Be(game.UpdatedAtUtc);
    }
}
