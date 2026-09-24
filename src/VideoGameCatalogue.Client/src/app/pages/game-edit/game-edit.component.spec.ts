import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { GameEditComponent } from './game-edit.component';
import { GameService } from '../../core/services/game.service';
import { Game } from '../../core/models/game.model';

describe('GameEditComponent', () => {
  let component: GameEditComponent;
  let fixture: ComponentFixture<GameEditComponent>;
  let mockGameService: Partial<GameService>;
  let mockRouter: Partial<Router>;
  let mockModalService: { open: ReturnType<typeof vi.fn> };

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

  const setupTestBed = async (routeParams: Record<string, string> = {}, serviceOverrides: Partial<GameService> = {}) => {
    mockGameService = {
      getMetadata: vi.fn().mockReturnValue(of({
        platforms: ['PC', 'PlayStation 5'],
        genres: ['Puzzle', 'Action'],
        ratings: ['Everyone', 'Everyone 10+']
      })),
      getGameById: vi.fn().mockReturnValue(of(existingGame)),
      createGame: vi.fn().mockReturnValue(of(existingGame)),
      updateGame: vi.fn().mockReturnValue(of(existingGame)),
      deleteGame: vi.fn().mockReturnValue(of(void 0)),
      ...serviceOverrides
    };

    mockRouter = {
      navigate: vi.fn()
    };

    mockModalService = {
      open: vi.fn().mockReturnValue({
        result: Promise.resolve('confirm')
      })
    };

    await TestBed.configureTestingModule({
      imports: [GameEditComponent],
      providers: [
        { provide: GameService, useValue: mockGameService },
        { provide: Router, useValue: mockRouter },
        { provide: NgbModal, useValue: mockModalService },
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

    it('should validate releaseYear with max as current year and min as 1950', () => {
      const yearControl = component.gameForm.get('releaseYear');
      const currentYear = new Date().getFullYear();

      yearControl?.setValue(currentYear);
      expect(yearControl?.valid).toBe(true);

      yearControl?.setValue(currentYear + 1);
      expect(yearControl?.valid).toBe(false);
      expect(yearControl?.hasError('max')).toBe(true);

      yearControl?.setValue(1949);
      expect(yearControl?.valid).toBe(false);
      expect(yearControl?.hasError('min')).toBe(true);
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

    it('should handle create error gracefully', () => {
      vi.mocked(mockGameService.createGame!).mockReturnValueOnce(
        throwError(() => ({ error: { detail: 'Database error occurred' } }))
      );

      component.gameForm.patchValue({
        title: 'Half-Life 2',
        platform: 'PC',
        genre: 'Action',
        releaseYear: 2004,
        rating: 'Mature 17+',
        description: 'FPS classic'
      });

      component.onSubmit();

      expect(component.isSaving()).toBe(false);
      expect(component.errorMessage()).toBe('Database error occurred');
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should not submit if form is invalid', () => {
      component.gameForm.patchValue({ title: '' });
      component.onSubmit();

      expect(mockGameService.createGame).not.toHaveBeenCalled();
      expect(component.gameForm.get('title')?.touched).toBe(true);
    });

    it('should navigate back on cancel', () => {
      component.onCancel();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/games']);
    });

    it('should not delete game if gameId is not set', async () => {
      mockModalService.open.mockReturnValue({
        result: Promise.resolve('confirm')
      });

      component.openDeleteModal({});
      await fixture.whenStable();

      expect(mockGameService.deleteGame).not.toHaveBeenCalled();
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

    it('should handle update error gracefully', () => {
      vi.mocked(mockGameService.updateGame!).mockReturnValueOnce(
        throwError(() => ({ error: { detail: 'Update failed' } }))
      );

      component.gameForm.patchValue({
        title: 'Portal 2 Reloaded'
      });

      component.onSubmit();

      expect(component.isSaving()).toBe(false);
      expect(component.errorMessage()).toBe('Update failed');
    });

    it('should handle error when loading game by id', async () => {
      TestBed.resetTestingModule();
      await setupTestBed(
        { id: 'non-existent' },
        { getGameById: vi.fn().mockReturnValue(throwError(() => new Error('Not found'))) }
      );

      expect(component.errorMessage()).toBe('Failed to load the video game details.');
      expect(component.isLoading()).toBe(false);
    });

    it('should correctly evaluate isFieldInvalid', () => {
      const titleControl = component.gameForm.get('title');
      titleControl?.setValue('');
      titleControl?.markAsTouched();

      expect(component.isFieldInvalid('title')).toBe(true);

      titleControl?.setValue('Valid Title');
      expect(component.isFieldInvalid('title')).toBe(false);

      expect(component.isFieldInvalid('nonExistentField')).toBe(false);
    });

    it('should display error message when metadata service fails', async () => {
      TestBed.resetTestingModule();
      await setupTestBed(
        {},
        { getMetadata: vi.fn().mockReturnValue(throwError(() => new Error('Service down'))) }
      );

      expect(component.errorMessage()).toContain('Failed to load catalogue lookup options');
      expect(component.platforms().length).toBe(0);
    });

    it('should delete game when modal confirms and navigate to catalogue', async () => {
      mockModalService.open.mockReturnValue({
        result: Promise.resolve('confirm')
      });
      vi.mocked(mockGameService.deleteGame!).mockReturnValue(of(void 0));

      component.openDeleteModal({});
      await fixture.whenStable();

      expect(mockGameService.deleteGame).toHaveBeenCalledWith(existingGame.id);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/games']);
    });

    it('should handle delete error when service fails', async () => {
      mockModalService.open.mockReturnValue({
        result: Promise.resolve('confirm')
      });
      vi.mocked(mockGameService.deleteGame!).mockReturnValue(throwError(() => new Error('Delete failed')));

      component.openDeleteModal({});
      await fixture.whenStable();

      expect(component.isDeleting()).toBe(false);
      expect(component.errorMessage()).toBe('Failed to delete video game.');
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should not delete game when modal is dismissed', async () => {
      mockModalService.open.mockReturnValue({
        result: Promise.reject('dismissed')
      });

      component.openDeleteModal({});
      await fixture.whenStable();

      expect(mockGameService.deleteGame).not.toHaveBeenCalled();
    });
  });
});

