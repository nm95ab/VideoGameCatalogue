Feature: Game Creation
    As a catalogue manager
    I want to add new video games with valid details
    So that the collection remains comprehensive and accurate

Scenario: Successfully add a new game to the catalogue
    Given I have game details:
        | Field       | Value                         |
        | Title       | Metroid Prime Remastered      |
        | Platform    | Nintendo Switch               |
        | Genre       | Action-Adventure              |
        | ReleaseYear | 2023                          |
        | Rating      | Teen                          |
        | Description | Intergalactic bounty hunter   |
    When I submit the request to create the game
    Then the response status code should be 201
    And the created game should have a valid ID
    And the created game title should be "Metroid Prime Remastered"
    And the game should be retrievable by its ID

Scenario: Reject creation when title is empty
    Given I have game details:
        | Field       | Value             |
        | Title       |                   |
        | Platform    | PC                |
        | Genre       | Action            |
        | ReleaseYear | 2022              |
        | Rating      | Everyone          |
        | Description | Missing title     |
    When I submit the request to create the game
    Then the response status code should be 400
    And the error code should be "GameTitle.Empty"

Scenario: Reject creation when title exceeds 150 characters
    Given I have a game with a title that exceeds 150 characters
    When I submit the request to create the game
    Then the response status code should be 400
    And the error code should be "GameTitle.TooLong"

Scenario: Reject creation when release year is before 1950
    Given I have game details:
        | Field       | Value             |
        | Title       | Ancient Game      |
        | Platform    | PC                |
        | Genre       | Strategy          |
        | ReleaseYear | 1945              |
        | Rating      | Everyone          |
        | Description | Before 1950       |
    When I submit the request to create the game
    Then the response status code should be 400
    And the error code should be "ReleaseYear.Invalid"

Scenario: Reject creation when release year is in the future beyond current year
    Given I have game details:
        | Field       | Value             |
        | Title       | Future Odyssey    |
        | Platform    | PC                |
        | Genre       | Action            |
        | ReleaseYear | 2099              |
        | Rating      | Everyone          |
        | Description | From the future   |
    When I submit the request to create the game
    Then the response status code should be 400
    And the error code should be "ReleaseYear.Invalid"

Scenario: Reject creation when platform is empty
    Given I have game details:
        | Field       | Value             |
        | Title       | Valid Title       |
        | Platform    |                   |
        | Genre       | Action            |
        | ReleaseYear | 2022              |
        | Rating      | Everyone          |
        | Description | Missing platform  |
    When I submit the request to create the game
    Then the response status code should be 400
    And the error code should be "Platform.Empty"

Scenario: Reject creation when genre is empty
    Given I have game details:
        | Field       | Value             |
        | Title       | Valid Title       |
        | Platform    | PC                |
        | Genre       |                   |
        | ReleaseYear | 2022              |
        | Rating      | Everyone          |
        | Description | Missing genre     |
    When I submit the request to create the game
    Then the response status code should be 400
    And the error code should be "Genre.Empty"

Scenario: Reject creation when rating is empty
    Given I have game details:
        | Field       | Value             |
        | Title       | Valid Title       |
        | Platform    | PC                |
        | Genre       | Action            |
        | ReleaseYear | 2022              |
        | Rating      |                   |
        | Description | Missing rating    |
    When I submit the request to create the game
    Then the response status code should be 400
    And the error code should be "Rating.Empty"
