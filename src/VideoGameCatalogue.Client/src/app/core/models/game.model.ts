export interface Game {
  id: string;
  title: string;
  platform: string;
  genre: string;
  releaseYear: number;
  rating: string;
  description: string;
  imageId?: string | null;
  imageUrl?: string | null;
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
  imageId?: string | null;
}

export interface UpdateGameRequest {
  title: string;
  platform: string;
  genre: string;
  releaseYear: number;
  rating: string;
  description?: string;
  imageId?: string | null;
}

export interface ImageUploadResponse {
  imageId: string;
  url: string;
}

export interface CatalogueMetadata {
  platforms: string[];
  genres: string[];
  ratings: string[];
}
