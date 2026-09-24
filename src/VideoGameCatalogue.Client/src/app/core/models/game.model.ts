export interface Game {
  id: string;
  title: string;
  platform: string;
  genre: string;
  releaseYear: number;
  rating: string;
  description: string;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}

export interface CreateGameRequest {
  title: string;
  platform: string;
  genre: string;
  releaseYear: number;
  rating: string;
  description?: string;
}

export interface UpdateGameRequest {
  title: string;
  platform: string;
  genre: string;
  releaseYear: number;
  rating: string;
  description?: string;
}

export interface CatalogueMetadata {
  platforms: string[];
  genres: string[];
  ratings: string[];
}
