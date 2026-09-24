Feature: Game Retrieval
    As a client application
    I want to retrieve a specific game by its ID
    So that I can display full game details or populate edit forms

Scenario: Successfully retrieve an existing game by ID
    Given an existing game in the catalogue
    When I request the game by its ID
    Then the response status code should be 200
    And the returned game should match the requested ID
    And the returned game should have a non-empty title and platform

Scenario: Retrieve a non-existent game ID returns 404
    When I request a game with ID "00000000-0000-0000-0000-000000009999"
    Then the response status code should be 404
    And the error code should be "VideoGame.NotFound"

Scenario: Retrieve a game with invalid GUID format returns 404
    When I request a game with invalid ID "not-a-valid-guid-value"
    Then the response status code should be 404
