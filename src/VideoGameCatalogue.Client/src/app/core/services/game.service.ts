import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CatalogueMetadata, CreateGameRequest, Game, UpdateGameRequest } from '../models/game.model';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5111/api/games';

  getGames(searchTerm?: string, platform?: string, genre?: string): Observable<Game[]> {
    let params = new HttpParams();

    if (searchTerm?.trim()) {
      params = params.set('search', searchTerm.trim());
    }

    if (platform?.trim()) {
      params = params.set('platform', platform.trim());
    }

    if (genre?.trim()) {
      params = params.set('genre', genre.trim());
    }

    return this.http.get<Game[]>(this.apiUrl, { params });
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
}
