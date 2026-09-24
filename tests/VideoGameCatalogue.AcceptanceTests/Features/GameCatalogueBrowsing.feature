Feature: Game Catalogue Browsing and Filtering
    As a video game enthusiast
    I want to browse and search games in the catalogue
    So that I can find titles by keyword, platform, or genre

Scenario: Browse all games in the seeded catalogue
    Given the catalogue is initialized with default seed data
    When I request all games
    Then the response status code should be 200
    And the catalogue should contain at least 7 games
    And each game in the catalogue should have a title, platform, genre, and release year

Scenario: Search games by keyword in title
    Given the catalogue is initialized with default seed data
    When I search games by keyword "Chrono"
    Then the response status code should be 200
    And the results should include a game with title "Chrono Trigger"
    And the results should not include a game with title "Halo: Combat Evolved"

Scenario: Filter games by platform
    Given the catalogue is initialized with default seed data
    When I filter games by platform "SNES"
    Then the response status code should be 200
    And all returned games should have platform "SNES"
    And the results should include a game with title "Super Mario World"

Scenario: Filter games by genre
    Given the catalogue is initialized with default seed data
    When I filter games by genre "Role-Playing (RPG)"
    Then the response status code should be 200
    And all returned games should have genre "Role-Playing (RPG)"
    And the results should include a game with title "The Witcher 3: Wild Hunt"

Scenario: Search games with combined criteria
    Given the catalogue is initialized with default seed data
    When I search games with keyword "Trigger", platform "SNES", and genre "Role-Playing (RPG)"
    Then the response status code should be 200
    And exactly 1 game should be returned
    And the returned game title should be "Chrono Trigger"

Scenario: Search games with non-matching term returns empty list
    Given the catalogue is initialized with default seed data
    When I search games by keyword "NonExistentGameXYZ999"
    Then the response status code should be 200
    And exactly 0 games should be returned
