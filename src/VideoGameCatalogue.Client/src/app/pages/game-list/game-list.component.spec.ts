import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { GameListComponent } from './game-list.component';
import { GameService } from '../../core/services/game.service';
import { Game } from '../../core/models/game.model';

describe('GameListComponent', () => {
  let component: GameListComponent;
  let fixture: ComponentFixture<GameListComponent>;
  let mockGameService: Partial<GameService>;
  let mockRouter: Partial<Router>;

  const sampleGames: Game[] = [
    {
      id: '11111111-1111-1111-1111-111111111111',
      title: 'Super Mario World',
      platform: 'SNES',
      genre: 'Platformer',
      releaseYear: 1990,
      rating: 'Everyone',
      description: 'Classic Mario',
      createdAtUtc: '2026-01-01T00:00:00Z',
      updatedAtUtc: null
    }
  ];

  beforeEach(async () => {
    mockGameService = {
      getGames: vi.fn().mockReturnValue(of(sampleGames)),
      getMetadata: vi.fn().mockReturnValue(of({ platforms: ['SNES'], genres: ['Platformer'], ratings: ['Everyone'] })),
      deleteGame: vi.fn().mockReturnValue(of(undefined))
    };

    mockRouter = {
      navigate: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [GameListComponent],
      providers: [
        { provide: GameService, useValue: mockGameService },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(GameListComponent);
    component = fixture.componentInstance;
  });

  it('should create and load games on init', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
    expect(component.games().length).toBe(1);
    expect(component.games()[0].title).toBe('Super Mario World');
    expect(component.isLoading()).toBe(false);
  });

  it('should filter games when search term changes', () => {
    fixture.detectChanges();
    component.searchTerm = 'Mario';
    component.onFilterChange();
    expect(mockGameService.getGames).toHaveBeenCalledWith('Mario', '', '');
  });

  it('should reset filters', () => {
    fixture.detectChanges();
    component.searchTerm = 'Mario';
    component.selectedPlatform = 'SNES';
    component.selectedGenre = 'Platformer';

    component.resetFilters();

    expect(component.searchTerm).toBe('');
    expect(component.selectedPlatform).toBe('');
    expect(component.selectedGenre).toBe('');
    expect(mockGameService.getGames).toHaveBeenCalledWith('', '', '');
  });

  it('should navigate to add page', () => {
    component.navigateToAdd();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/games/new']);
  });

  it('should navigate to edit page', () => {
    component.navigateToEdit('11111111-1111-1111-1111-111111111111');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/games', '11111111-1111-1111-1111-111111111111', 'edit']);
  });

  it('should debounce rapid search inputs and make only one call after 300ms', () => {
    vi.useFakeTimers();
    try {
      fixture.detectChanges();
      vi.mocked(mockGameService.getGames!).mockClear();

      component.onSearchInput('M');
      component.onSearchInput('Ma');
      component.onSearchInput('Mar');
      component.onSearchInput('Mario');

      expect(mockGameService.getGames).not.toHaveBeenCalled();

      vi.advanceTimersByTime(299);
      expect(mockGameService.getGames).not.toHaveBeenCalled();

      vi.advanceTimersByTime(1);
      expect(mockGameService.getGames).toHaveBeenCalledTimes(1);
      expect(mockGameService.getGames).toHaveBeenCalledWith('Mario', '', '');
    } finally {
      vi.useRealTimers();
    }
  });

  it('should not trigger redundant search if term is not distinct', () => {
    vi.useFakeTimers();
    try {
      fixture.detectChanges();
      vi.mocked(mockGameService.getGames!).mockClear();

      component.onSearchInput('Mario');
      vi.advanceTimersByTime(300);
      expect(mockGameService.getGames).toHaveBeenCalledTimes(1);

      component.onSearchInput('Mario');
      vi.advanceTimersByTime(300);
      expect(mockGameService.getGames).toHaveBeenCalledTimes(1);
    } finally {
      vi.useRealTimers();
    }
  });

  it('should cancel previous pending request via switchMap when a new search occurs', () => {
    fixture.detectChanges();

    let subscriber1Cancelled = false;
    const slowObservable$ = new Observable<Game[]>((subscriber) => {
      return () => {
        subscriber1Cancelled = true;
      };
    });

    vi.mocked(mockGameService.getGames!).mockReturnValueOnce(slowObservable$);
    component.onFilterChange();

    expect(subscriber1Cancelled).toBe(false);

    // Trigger second request immediately
    vi.mocked(mockGameService.getGames!).mockReturnValueOnce(of(sampleGames));
    component.onFilterChange();

    expect(subscriber1Cancelled).toBe(true);
    expect(component.games().length).toBe(1);
  });
});
