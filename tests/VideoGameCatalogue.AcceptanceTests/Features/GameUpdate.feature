Feature: Game Update
    As a catalogue manager
    I want to update the details of an existing video game
    So that changes and corrections are saved in the system

Scenario: Successfully update an existing game
    Given an existing game in the catalogue
    When I update the game with details:
        | Field       | Value                              |
        | Title       | Chrono Trigger (Definitive Remake) |
        | Platform    | PC                                 |
        | Genre       | Role-Playing (RPG)                 |
        | ReleaseYear | 2024                               |
        | Rating      | Everyone 10+                       |
        | Description | Upgraded HD textures and orchestra |
    Then the response status code should be 200
    And the updated game title should be "Chrono Trigger (Definitive Remake)"
    And the updated game platform should be "PC"
    And the updated game release year should be 2024
    And the updated game updatedAtUtc timestamp should be present

Scenario: Reject update when release year is beyond current year
    Given an existing game in the catalogue
    When I update the game with release year 2099
    Then the response status code should be 400
    And the error code should be "ReleaseYear.Invalid"

Scenario: Reject update when title is empty
    Given an existing game in the catalogue
    When I update the game with an empty title
    Then the response status code should be 400
    And the error code should be "GameTitle.Empty"

Scenario: Update a non-existent game ID returns 404
    When I update a game with ID "00000000-0000-0000-0000-000000009999" with valid details
    Then the response status code should be 404
    And the error code should be "VideoGame.NotFound"
