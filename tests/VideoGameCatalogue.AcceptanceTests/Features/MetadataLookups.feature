Feature: Metadata Lookups
    As a client application
    I want to fetch supported platforms, genres, and ratings
    So that UI forms and validation options can be dynamically populated from the database

Scenario: Fetch all catalogue metadata lookup lists
    When I request the catalogue metadata
    Then the response status code should be 200
    And the metadata should contain platforms including "PC", "PlayStation 5", "Nintendo Switch"
    And the metadata should contain genres including "Action", "Role-Playing (RPG)", "Platformer"
    And the metadata should contain ratings including "Everyone", "Teen", "Mature 17+"
    And the metadata should contain eras including "16-Bit Golden Age", "3D Revolution", "Modern Era"
