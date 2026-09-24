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

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
