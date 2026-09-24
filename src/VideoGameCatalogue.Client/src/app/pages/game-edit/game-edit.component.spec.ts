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
    imageId: 'existing-img-id.webp',
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
      uploadImage: vi.fn().mockReturnValue(of({ imageId: 'new-uploaded-id.webp', url: '/api/images/new-uploaded-id.webp' })),
      getImageUrl: vi.fn((id: string) => `http://localhost:5111/api/images/${id}`),
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
        description: 'FPS classic',
        imageId: null
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

  describe('Image Handling', () => {
    beforeEach(async () => {
      await setupTestBed({ id: existingGame.id });
    });

    it('should initialize with existing image in edit mode', () => {
      expect(component.currentImageId()).toBe('existing-img-id.webp');
      expect(component.imagePreviewUrl()).toBe('http://localhost:5111/api/images/existing-img-id.webp');
      expect(component.isImageRemoved()).toBe(false);
    });

    it('should handle onRemoveImage', () => {
      component.onRemoveImage();
      expect(component.imagePreviewUrl()).toBeNull();
      expect(component.selectedFile).toBeNull();
      expect(component.isImageRemoved()).toBe(true);
    });

    it('should reject file exceeding 10MB', () => {
      const largeFile = new File(['x'.repeat(100)], 'huge.png', { type: 'image/png' });
      Object.defineProperty(largeFile, 'size', { value: 11 * 1024 * 1024 });

      const event = { target: { files: [largeFile], value: 'huge.png' } } as unknown as Event;

      component.onFileSelected(event);
      expect(component.imageError()).toBe('Image size exceeds the 10 MB limit.');
      expect(component.selectedFile).toBeNull();
    });

    it('should reject invalid file types', () => {
      const invalidFile = new File(['text'], 'notes.txt', { type: 'text/plain' });
      const event = { target: { files: [invalidFile], value: 'notes.txt' } } as unknown as Event;

      component.onFileSelected(event);
      expect(component.imageError()).toBe('Please select a valid image file (JPEG, PNG, or WebP).');
      expect(component.selectedFile).toBeNull();
    });

    it('should accept valid image file and set preview', () => {
      const validFile = new File(['image-bytes'], 'cover.png', { type: 'image/png' });
      const event = { target: { files: [validFile], value: 'cover.png' } } as unknown as Event;

      component.onFileSelected(event);
      expect(component.selectedFile).toBe(validFile);
      expect(component.isImageRemoved()).toBe(false);
      expect(component.imageError()).toBeNull();
    });

    it('should upload image and update game with new imageId on submit', () => {
      const validFile = new File(['image-bytes'], 'cover.png', { type: 'image/png' });
      component.selectedFile = validFile;

      component.onSubmit();

      expect(mockGameService.uploadImage).toHaveBeenCalledWith(validFile);
      expect(mockGameService.updateGame).toHaveBeenCalledWith(
        existingGame.id,
        expect.objectContaining({ imageId: 'new-uploaded-id.webp' })
      );
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/games']);
    });

    it('should handle upload error on submit', () => {
      vi.mocked(mockGameService.uploadImage!).mockReturnValue(
        throwError(() => ({ error: { detail: 'Corrupt image' } }))
      );

      const validFile = new File(['image-bytes'], 'corrupt.png', { type: 'image/png' });
      component.selectedFile = validFile;

      component.onSubmit();

      expect(component.errorMessage()).toBe('Corrupt image');
      expect(component.isSaving()).toBe(false);
      expect(mockGameService.updateGame).not.toHaveBeenCalled();
    });

    it('should save game with null imageId when image was removed', () => {
      component.onRemoveImage();
      component.onSubmit();

      expect(mockGameService.updateGame).toHaveBeenCalledWith(
        existingGame.id,
        expect.objectContaining({ imageId: null })
      );
    });

    it('should compute getInitials correctly', () => {
      expect(component.getInitials('Final Fantasy')).toBe('FF');
      expect(component.getInitials('Halo')).toBe('HA');
      expect(component.getInitials('')).toBe('??');
    });
  });
});

