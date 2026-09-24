import { Component, OnInit, inject, signal, computed, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NgbAlertModule, NgbTooltipModule, NgbProgressbarModule, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { GameService } from '../../core/services/game.service';
import { CreateGameRequest, GamingEraInfo, UpdateGameRequest } from '../../core/models/game.model';

@Component({
  selector: 'app-game-edit',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    NgbAlertModule,
    NgbTooltipModule,
    NgbProgressbarModule
  ],
  templateUrl: './game-edit.component.html',
  styleUrls: ['./game-edit.component.scss']
})
export class GameEditComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly gameService = inject(GameService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly modalService = inject(NgbModal);
  private readonly destroyRef = inject(DestroyRef);

  gameForm!: FormGroup;
  gameId: string | null = null;
  isEditMode = false;

  readonly isLoading = signal<boolean>(false);
  readonly isSaving = signal<boolean>(false);
  readonly isDeleting = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);

  selectedFile: File | null = null;
  readonly imagePreviewUrl = signal<string | null>(null);
  readonly currentImageId = signal<string | null>(null);
  readonly isImageRemoved = signal<boolean>(false);
  readonly imageError = signal<string | null>(null);

  readonly platforms = signal<string[]>([]);
  readonly genres = signal<string[]>([]);
  readonly ratings = signal<string[]>([]);
  readonly eras = signal<GamingEraInfo[]>([]);
  readonly currentYear = new Date().getFullYear();
  readonly currentReleaseYear = signal<number>(new Date().getFullYear());

  readonly eraInsight = computed(() => {
    const year = this.currentReleaseYear();
    if (!year || year < 1950 || year > this.currentYear) return null;
    const match = this.eras().find(e => year >= e.startYear && (!e.endYear || year <= e.endYear));
    const ageInYears = this.currentYear - year;
    const decade = `${Math.floor(year / 10) * 10}s`;
    return { era: match, ageInYears, decade };
  });

  ngOnInit(): void {
    this.initForm();
    this.loadMetadata();

    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const id = params.get('id');
        this.gameId = id;
        this.isEditMode = !!id;

        if (this.isEditMode && id) {
          this.loadGame(id);
        } else {
          this.initForm();
        }
      });
  }

  private initForm(): void {
    this.currentImageId.set(null);
    this.isImageRemoved.set(false);
    this.selectedFile = null;
    this.imagePreviewUrl.set(null);
    this.imageError.set(null);

    this.gameForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(150)]],
      platform: ['', [Validators.required, Validators.maxLength(50)]],
      genre: ['', [Validators.required, Validators.maxLength(50)]],
      releaseYear: [this.currentYear, [Validators.required, Validators.min(1950), Validators.max(this.currentYear)]],
      rating: ['', [Validators.required, Validators.maxLength(30)]],
      description: ['', [Validators.maxLength(2000)]]
    });

    this.currentReleaseYear.set(this.currentYear);
    this.gameForm.get('releaseYear')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(val => {
        const num = Number(val);
        if (!isNaN(num) && num > 0) {
          this.currentReleaseYear.set(num);
        }
      });
  }

  private loadMetadata(): void {
    this.gameService.getMetadata()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (meta) => {
          this.platforms.set(meta.platforms);
          this.genres.set(meta.genres);
          this.ratings.set(meta.ratings);
          if (meta.eras) {
            this.eras.set(meta.eras);
          }

          // Pre-select defaults if creating new
          if (!this.isEditMode) {
            if (!this.gameForm.value.platform && meta.platforms.length > 0) {
              this.gameForm.patchValue({ platform: meta.platforms[0] });
            }
            if (!this.gameForm.value.genre && meta.genres.length > 0) {
              this.gameForm.patchValue({ genre: meta.genres[0] });
            }
            if (!this.gameForm.value.rating && meta.ratings.length > 0) {
              this.gameForm.patchValue({ rating: meta.ratings[0] });
            }
          }
        },
        error: () => {
          this.errorMessage.set('Failed to load catalogue lookup options. Please ensure the backend API is running.');
        }
      });
  }

  private loadGame(id: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.gameService.getGameById(id).subscribe({
      next: (game) => {
        this.gameForm.patchValue({
          title: game.title,
          platform: game.platform,
          genre: game.genre,
          releaseYear: game.releaseYear,
          rating: game.rating,
          description: game.description
        });
        this.currentReleaseYear.set(game.releaseYear);
        this.currentImageId.set(game.imageId || null);
        this.isImageRemoved.set(false);
        this.selectedFile = null;
        this.imageError.set(null);
        if (game.imageId) {
          this.imagePreviewUrl.set(this.gameService.getImageUrl(game.imageId, game.imageUrl));
        } else {
          this.imagePreviewUrl.set(null);
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load the video game details.');
        this.isLoading.set(false);
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.gameForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      return;
    }
    const file = input.files[0];
    this.imageError.set(null);

    const validTypes = ['image/jpeg', 'image/png', 'image/webp'];
    if (!validTypes.includes(file.type)) {
      this.imageError.set('Please select a valid image file (JPEG, PNG, or WebP).');
      input.value = '';
      return;
    }

    const maxSize = 10 * 1024 * 1024; // 10 MB
    if (file.size > maxSize) {
      this.imageError.set('Image size exceeds the 10 MB limit.');
      input.value = '';
      return;
    }

    this.selectedFile = file;
    this.isImageRemoved.set(false);

    const reader = new FileReader();
    reader.onload = () => {
      this.imagePreviewUrl.set(reader.result as string);
    };
    reader.readAsDataURL(file);
  }

  onRemoveImage(): void {
    this.selectedFile = null;
    this.imagePreviewUrl.set(null);
    this.isImageRemoved.set(true);
    this.imageError.set(null);
  }

  getInitials(title: string): string {
    if (!title?.trim()) {
      return '??';
    }
    const words = title.trim().split(/\s+/).filter(w => w.length > 0);
    if (words.length === 1) {
      return words[0].substring(0, Math.min(2, words[0].length)).toUpperCase();
    }
    return (words[0][0] + words[1][0]).toUpperCase();
  }

  onSubmit(): void {
    if (this.gameForm.invalid) {
      this.gameForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);

    if (this.selectedFile) {
      this.gameService.uploadImage(this.selectedFile).subscribe({
        next: (res) => {
          this.saveGameWithImage(res.imageId);
        },
        error: (err) => {
          this.errorMessage.set(err.error?.detail || err.error?.title || 'Failed to upload game image.');
          this.isSaving.set(false);
        }
      });
    } else {
      const imageId = this.isImageRemoved() ? null : this.currentImageId();
      this.saveGameWithImage(imageId);
    }
  }

  private saveGameWithImage(imageId: string | null): void {
    const formValues = this.gameForm.value;

    if (this.isEditMode && this.gameId) {
      const request: UpdateGameRequest = {
        title: formValues.title,
        platform: formValues.platform,
        genre: formValues.genre,
        releaseYear: Number(formValues.releaseYear),
        rating: formValues.rating,
        description: formValues.description,
        imageId: imageId
      };

      this.gameService.updateGame(this.gameId, request).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.router.navigate(['/games']);
        },
        error: (err) => {
          this.errorMessage.set(err.error?.detail || err.error?.title || 'Failed to update video game.');
          this.isSaving.set(false);
        }
      });
    } else {
      const request: CreateGameRequest = {
        title: formValues.title,
        platform: formValues.platform,
        genre: formValues.genre,
        releaseYear: Number(formValues.releaseYear),
        rating: formValues.rating,
        description: formValues.description,
        imageId: imageId
      };

      this.gameService.createGame(request).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.router.navigate(['/games']);
        },
        error: (err) => {
          this.errorMessage.set(err.error?.detail || err.error?.title || 'Failed to create video game.');
          this.isSaving.set(false);
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/games']);
  }

  openDeleteModal(content: unknown): void {
    this.modalService.open(content, { ariaLabelledBy: 'modal-title', centered: true }).result.then(
      (result) => {
        if (result === 'confirm' && this.gameId) {
          this.executeDelete(this.gameId);
        }
      },
      () => {
        // dismissed
      }
    );
  }

  private executeDelete(id: string): void {
    this.isDeleting.set(true);
    this.errorMessage.set(null);

    this.gameService.deleteGame(id).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.router.navigate(['/games']);
      },
      error: () => {
        this.errorMessage.set('Failed to delete video game.');
        this.isDeleting.set(false);
      }
    });
  }
}
