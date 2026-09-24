Feature: Game Thumbnail Management
    As a catalogue manager
    I want to upload, retrieve, associate, and delete thumbnail images for games
    So that the catalogue displays visual representations of games without degrading performance

Scenario: Upload valid image creates thumbnail and returns image ID
    When I upload a valid sample image "cover.png"
    Then the response status code should be 201
    And the image upload response should contain a valid image ID and URL

Scenario: Retrieve uploaded thumbnail image
    Given an uploaded image "avatar.png"
    When I request the image by its ID
    Then the response status code should be 200
    And the response content type should be "image/webp"

Scenario: Reject invalid image upload with non-image data
    When I upload an invalid image file "document.txt" with text content
    Then the response status code should be 400
    And the error code should be "Image.InvalidFormat"

Scenario: Create game with uploaded thumbnail image
    Given an uploaded image "gameart.png"
    When I create a game with the uploaded image ID
    Then the response status code should be 201
    And the created game should reference the image ID
    And the created game should have an image URL

Scenario: Delete uploaded image from storage
    Given an uploaded image "temporary.png"
    When I delete the image by its ID
    Then the response status code should be 204
    And requesting the deleted image should return 404
