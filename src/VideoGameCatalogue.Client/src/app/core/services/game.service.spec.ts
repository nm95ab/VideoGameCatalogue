import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { GameService } from './game.service';
import { CreateGameRequest, Game, UpdateGameRequest } from '../models/game.model';

describe('GameService', () => {
  let service: GameService;
  let httpTesting: HttpTestingController;

  const mockGames: Game[] = [
    {
      id: '11111111-1111-1111-1111-111111111111',
      title: 'Chrono Trigger',
      platform: 'SNES',
      genre: 'Role-Playing (RPG)',
      releaseYear: 1995,
      rating: 'Everyone',
      description: 'Legendary JRPG',
      createdAtUtc: '2026-01-01T00:00:00Z',
      updatedAtUtc: null
    }
  ];

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        GameService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(GameService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get games with query params', () => {
    service.getGames('Chrono', 'SNES', 'RPG').subscribe(games => {
      expect(games.length).toBe(1);
      expect(games[0].title).toBe('Chrono Trigger');
    });

    const req = httpTesting.expectOne(request =>
      request.url === 'http://localhost:5111/api/games' &&
      request.params.get('search') === 'Chrono' &&
      request.params.get('platform') === 'SNES' &&
      request.params.get('genre') === 'RPG'
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockGames);
  });

  it('should get game by ID', () => {
    service.getGameById('11111111-1111-1111-1111-111111111111').subscribe(game => {
      expect(game.title).toBe('Chrono Trigger');
    });

    const req = httpTesting.expectOne('http://localhost:5111/api/games/11111111-1111-1111-1111-111111111111');
    expect(req.request.method).toBe('GET');
    req.flush(mockGames[0]);
  });

  it('should create game', () => {
    const newGame: CreateGameRequest = {
      title: 'Elden Ring',
      platform: 'PlayStation 5',
      genre: 'Action',
      releaseYear: 2022,
      rating: 'Mature 17+',
      description: 'Masterpiece'
    };

    service.createGame(newGame).subscribe(created => {
      expect(created.title).toBe('Elden Ring');
    });

    const req = httpTesting.expectOne('http://localhost:5111/api/games');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(newGame);
    req.flush({ ...newGame, id: '22222222-2222-2222-2222-222222222222', createdAtUtc: '2026-01-01T00:00:00Z', updatedAtUtc: null });
  });

  it('should update game', () => {
    const updateGame: UpdateGameRequest = {
      title: 'Chrono Trigger Remastered',
      platform: 'PC',
      genre: 'Role-Playing (RPG)',
      releaseYear: 2023,
      rating: 'Teen'
    };

    service.updateGame('11111111-1111-1111-1111-111111111111', updateGame).subscribe(updated => {
      expect(updated.title).toBe('Chrono Trigger Remastered');
    });

    const req = httpTesting.expectOne('http://localhost:5111/api/games/11111111-1111-1111-1111-111111111111');
    expect(req.request.method).toBe('PUT');
    req.flush({ ...mockGames[0], ...updateGame });
  });

  it('should delete game', () => {
    service.deleteGame('11111111-1111-1111-1111-111111111111').subscribe();

    const req = httpTesting.expectOne('http://localhost:5111/api/games/11111111-1111-1111-1111-111111111111');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should fetch metadata', () => {
    const mockMeta = {
      platforms: ['PC', 'SNES'],
      genres: ['RPG', 'Action'],
      ratings: ['Everyone', 'Teen']
    };

    service.getMetadata().subscribe(meta => {
      expect(meta.platforms).toContain('PC');
      expect(meta.genres).toContain('RPG');
    });

    const req = httpTesting.expectOne('http://localhost:5111/api/games/metadata');
    expect(req.request.method).toBe('GET');
    req.flush(mockMeta);
  });

  it('should upload image', () => {
    const mockFile = new File(['dummy-content'], 'test.png', { type: 'image/png' });
    const mockResponse = { imageId: 'img-123.webp', url: '/api/images/img-123.webp' };

    service.uploadImage(mockFile).subscribe(res => {
      expect(res.imageId).toBe('img-123.webp');
      expect(res.url).toBe('/api/images/img-123.webp');
    });

    const req = httpTesting.expectOne('http://localhost:5111/api/images');
    expect(req.request.method).toBe('POST');
    expect(req.request.body instanceof FormData).toBe(true);
    req.flush(mockResponse);
  });

  it('should delete image', () => {
    service.deleteImage('img-123.webp').subscribe();

    const req = httpTesting.expectOne('http://localhost:5111/api/images/img-123.webp');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should return image URL', () => {
    const url = service.getImageUrl('img-123.webp');
    expect(url).toBe('http://localhost:5111/api/images/img-123.webp');
  });
});
