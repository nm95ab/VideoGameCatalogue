import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { GameListComponent } from './game-list.component';
import { GameService } from '../../core/services/game.service';
import { Game, PagedResult } from '../../core/models/game.model';

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

  const samplePagedResult: PagedResult<Game> = {
    items: sampleGames,
    pageNumber: 1,
    pageSize: 6,
    totalCount: 1,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false
  };

  beforeEach(async () => {
    mockGameService = {
      getGames: vi.fn().mockReturnValue(of(samplePagedResult)),
      getMetadata: vi.fn().mockReturnValue(of({ platforms: ['SNES'], genres: ['Platformer'], ratings: ['Everyone'] })),
      getImageUrl: vi.fn((id: string, directUrl?: string | null) => directUrl || `http://localhost:5111/api/images/${id}`)
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
    expect(component.totalCount()).toBe(1);
    expect(component.totalPages()).toBe(1);
    expect(component.isLoading()).toBe(false);
  });

  it('should filter games when search term changes', () => {
    fixture.detectChanges();
    component.searchTerm = 'Mario';
    component.onFilterChange();
    expect(mockGameService.getGames).toHaveBeenCalledWith('Mario', '', '', 1, 6);
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
    expect(mockGameService.getGames).toHaveBeenCalledWith('', '', '', 1, 6);
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
      expect(mockGameService.getGames).toHaveBeenCalledWith('Mario', '', '', 1, 6);
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
    const slowObservable$ = new Observable<PagedResult<Game>>((subscriber) => {
      return () => {
        subscriber1Cancelled = true;
      };
    });

    vi.mocked(mockGameService.getGames!).mockReturnValueOnce(slowObservable$);
    component.onFilterChange();

    expect(subscriber1Cancelled).toBe(false);

    // Trigger second request immediately
    vi.mocked(mockGameService.getGames!).mockReturnValueOnce(of(samplePagedResult));
    component.onFilterChange();

    expect(subscriber1Cancelled).toBe(true);
    expect(component.games().length).toBe(1);
  });

  it('should display error message when getGames fails', () => {
    vi.mocked(mockGameService.getGames!).mockReturnValueOnce(throwError(() => new Error('Network error')));
    component.loadGames();

    expect(component.errorMessage()).toBe('Failed to load video games. Ensure the backend API is running.');
    expect(component.isLoading()).toBe(false);
  });

  it('should handle getMetadata error gracefully without crashing', () => {
    vi.mocked(mockGameService.getMetadata!).mockReturnValueOnce(throwError(() => new Error('Metadata error')));
    component.loadMetadata();

    expect(component.platforms().length).toBe(0);
    expect(component.genres().length).toBe(0);
  });

  it('should fetch new page when page changes', () => {
    fixture.detectChanges();
    vi.mocked(mockGameService.getGames!).mockClear();

    component.onPageChange(2);

    expect(component.page()).toBe(2);
    expect(mockGameService.getGames).toHaveBeenCalledWith('', '', '', 2, 6);
  });

  it('should reset page to 1 when search or filters change', () => {
    fixture.detectChanges();
    component.page.set(2);

    component.onSearchInput('Zelda');
    expect(component.page()).toBe(1);

    component.page.set(3);
    component.onFilterChange();
    expect(component.page()).toBe(1);

    component.page.set(4);
    component.resetFilters();
    expect(component.page()).toBe(1);
  });

  it('should update pageSize and reset page to 1 when pageSize changes', () => {
    fixture.detectChanges();
    vi.mocked(mockGameService.getGames!).mockClear();

    component.onPageSizeChange(12);

    expect(component.pageSize()).toBe(12);
    expect(component.page()).toBe(1);
    expect(mockGameService.getGames).toHaveBeenCalledWith('', '', '', 1, 12);
  });

  it('should compute startItemIndex and endItemIndex correctly', () => {
    fixture.detectChanges();
    expect(component.startItemIndex()).toBe(1);
    expect(component.endItemIndex()).toBe(1);

    component.totalCount.set(25);
    component.page.set(2);
    component.pageSize.set(10);
    expect(component.startItemIndex()).toBe(11);
    expect(component.endItemIndex()).toBe(20);

    component.page.set(3);
    expect(component.startItemIndex()).toBe(21);
    expect(component.endItemIndex()).toBe(25);

    component.totalCount.set(0);
    expect(component.startItemIndex()).toBe(0);
  });

  describe('Thumbnails and Initials', () => {
    it('should calculate initials correctly', () => {
      expect(component.getInitials('Chrono Trigger')).toBe('CT');
      expect(component.getInitials('Doom')).toBe('DO');
      expect(component.getInitials('   ')).toBe('??');
      expect(component.getInitials('')).toBe('??');
    });

    it('should delegate getImageUrl to gameService with directUrl', () => {
      const url = component.getImageUrl('test-123.webp', 'https://cdn.example.com/test-123.webp');
      expect(url).toBe('https://cdn.example.com/test-123.webp');
      expect(mockGameService.getImageUrl).toHaveBeenCalledWith('test-123.webp', 'https://cdn.example.com/test-123.webp');
    });

    it('should handle onImageError by replacing img with initials placeholder', () => {
      const parent = document.createElement('div');
      const img = document.createElement('img');
      parent.appendChild(img);

      const mockEvent = { target: img } as unknown as Event;
      component.onImageError(mockEvent, 'Zelda Ocarina');

      expect(parent.children.length).toBe(1);
      const placeholder = parent.firstElementChild as HTMLElement;
      expect(placeholder.tagName).toBe('DIV');
      expect(placeholder.textContent).toBe('ZO');
      expect(placeholder.title).toBe('Zelda Ocarina');
    });
  });
});
