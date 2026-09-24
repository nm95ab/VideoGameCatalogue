import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { GameEditComponent } from './game-edit.component';
import { GameService } from '../../core/services/game.service';
import { Game } from '../../core/models/game.model';

describe('GameEditComponent', () => {
  let component: GameEditComponent;
  let fixture: ComponentFixture<GameEditComponent>;
  let mockGameService: Partial<GameService>;
  let mockRouter: Partial<Router>;

  const existingGame: Game = {
    id: '11111111-1111-1111-1111-111111111111',
    title: 'Portal 2',
    platform: 'PC',
    genre: 'Puzzle',
    releaseYear: 2011,
    rating: 'Everyone 10+',
    description: 'Valve puzzle game',
    createdAtUtc: '2026-01-01T00:00:00Z',
    updatedAtUtc: null
  };

  const setupTestBed = async (routeParams: Record<string, string> = {}) => {
    mockGameService = {
      getMetadata: vi.fn().mockReturnValue(of({
        platforms: ['PC', 'PlayStation 5'],
        genres: ['Puzzle', 'Action'],
        ratings: ['Everyone', 'Everyone 10+']
      })),
      getGameById: vi.fn().mockReturnValue(of(existingGame)),
      createGame: vi.fn().mockReturnValue(of(existingGame)),
      updateGame: vi.fn().mockReturnValue(of(existingGame))
    };

    mockRouter = {
      navigate: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [GameEditComponent],
      providers: [
        { provide: GameService, useValue: mockGameService },
        { provide: Router, useValue: mockRouter },
        {
          provide: ActivatedRoute,
          useValue: {
            paramMap: of(convertToParamMap(routeParams))
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(GameEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  };

  describe('Create Mode', () => {
    beforeEach(async () => {
      await setupTestBed();
    });

    it('should create in non-edit mode', () => {
      expect(component).toBeTruthy();
      expect(component.isEditMode).toBe(false);
      expect(component.gameForm).toBeDefined();
    });

    it('should be invalid when title is empty', () => {
      component.gameForm.patchValue({
        title: '',
        platform: 'PC',
        genre: 'Puzzle',
        releaseYear: 2022,
        rating: 'Everyone'
      });

      expect(component.gameForm.valid).toBe(false);
      expect(component.gameForm.get('title')?.hasError('required')).toBe(true);
    });

    it('should submit valid new game and navigate to catalogue', () => {
      component.gameForm.patchValue({
        title: 'Half-Life 2',
        platform: 'PC',
        genre: 'Action',
        releaseYear: 2004,
        rating: 'Mature 17+',
        description: 'FPS classic'
      });

      component.onSubmit();

      expect(mockGameService.createGame).toHaveBeenCalledWith({
        title: 'Half-Life 2',
        platform: 'PC',
        genre: 'Action',
        releaseYear: 2004,
        rating: 'Mature 17+',
        description: 'FPS classic'
      });
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/games']);
    });

    it('should navigate back on cancel', () => {
      component.onCancel();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/games']);
    });
  });

  describe('Edit Mode', () => {
    beforeEach(async () => {
      await setupTestBed({ id: '11111111-1111-1111-1111-111111111111' });
    });

    it('should initialize in edit mode and populate form with game data', () => {
      expect(component.isEditMode).toBe(true);
      expect(component.gameId).toBe('11111111-1111-1111-1111-111111111111');
      expect(mockGameService.getGameById).toHaveBeenCalledWith('11111111-1111-1111-1111-111111111111');
      expect(component.gameForm.value.title).toBe('Portal 2');
      expect(component.gameForm.value.platform).toBe('PC');
      expect(component.gameForm.value.releaseYear).toBe(2011);
    });

    it('should submit updated details and navigate to catalogue', () => {
      component.gameForm.patchValue({
        title: 'Portal 2 Reloaded'
      });

      component.onSubmit();

      expect(mockGameService.updateGame).toHaveBeenCalledWith(
        '11111111-1111-1111-1111-111111111111',
        expect.objectContaining({
          title: 'Portal 2 Reloaded'
        })
      );
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/games']);
    });
  });
});
