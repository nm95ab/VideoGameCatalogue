Feature: Game Deletion
    As a catalogue manager
    I want to delete a game from the catalogue
    So that discontinued or erroneously added entries are removed

Scenario: Successfully delete an existing game
    Given an existing game in the catalogue
    When I delete the game by its ID
    Then the response status code should be 204
    And the deleted game should no longer exist when fetched by ID

Scenario: Delete a non-existent game ID returns 404
    When I delete a game with ID "00000000-0000-0000-0000-000000009999"
    Then the response status code should be 404
    And the error code should be "VideoGame.NotFound"
