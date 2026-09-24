import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CatalogueMetadata, CreateGameRequest, Game, ImageUploadResponse, PagedResult, UpdateGameRequest } from '../models/game.model';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = typeof window !== 'undefined' && window.location?.hostname
    ? `http://${window.location.hostname}:5111/api`
    : 'http://localhost:5111/api';
  private readonly apiUrl = `${this.baseUrl}/games`;
  private readonly imagesUrl = `${this.baseUrl}/images`;

  getGames(searchTerm?: string, platform?: string, genre?: string, page: number = 1, pageSize: number = 6): Observable<PagedResult<Game>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm?.trim()) {
      params = params.set('search', searchTerm.trim());
    }

    if (platform?.trim()) {
      params = params.set('platform', platform.trim());
    }

    if (genre?.trim()) {
      params = params.set('genre', genre.trim());
    }

    return this.http.get<PagedResult<Game>>(this.apiUrl, { params });
  }

  getGameById(id: string): Observable<Game> {
    return this.http.get<Game>(`${this.apiUrl}/${id}`);
  }

  createGame(request: CreateGameRequest): Observable<Game> {
    return this.http.post<Game>(this.apiUrl, request);
  }

  updateGame(id: string, request: UpdateGameRequest): Observable<Game> {
    return this.http.put<Game>(`${this.apiUrl}/${id}`, request);
  }

  deleteGame(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getMetadata(): Observable<CatalogueMetadata> {
    return this.http.get<CatalogueMetadata>(`${this.apiUrl}/metadata`);
  }

  uploadImage(file: File): Observable<ImageUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ImageUploadResponse>(this.imagesUrl, formData);
  }

  deleteImage(imageId: string): Observable<void> {
    return this.http.delete<void>(`${this.imagesUrl}/${imageId}`);
  }

  getImageUrl(imageId: string, directUrl?: string | null): string {
    if (directUrl?.trim()) {
      const trimmed = directUrl.trim();
      if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) {
        return trimmed;
      }
      if (trimmed.startsWith('/')) {
        const origin = this.baseUrl.replace(/\/api$/, '');
        return `${origin}${trimmed}`;
      }
    }
    return `${this.imagesUrl}/${imageId}`;
  }
}
