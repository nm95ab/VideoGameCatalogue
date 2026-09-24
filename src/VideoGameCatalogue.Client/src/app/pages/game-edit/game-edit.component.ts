import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgbAlertModule, NgbTooltipModule, NgbProgressbarModule, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { GameService } from '../../core/services/game.service';
import { CreateGameRequest, UpdateGameRequest } from '../../core/models/game.model';

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

  gameForm!: FormGroup;
  gameId: string | null = null;
  isEditMode = false;

  readonly isLoading = signal<boolean>(false);
  readonly isSaving = signal<boolean>(false);
  readonly isDeleting = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);

  readonly platforms = signal<string[]>([]);
  readonly genres = signal<string[]>([]);
  readonly ratings = signal<string[]>([]);

  ngOnInit(): void {
    this.initForm();
    this.loadMetadata();

    this.route.paramMap.subscribe(params => {
      this.gameId = params.get('id');
      this.isEditMode = !!this.gameId;

      if (this.isEditMode && this.gameId) {
        this.loadGame(this.gameId);
      }
    });
  }

  private initForm(): void {
    const currentYear = new Date().getFullYear();

    this.gameForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(150)]],
      platform: ['', [Validators.required, Validators.maxLength(50)]],
      genre: ['', [Validators.required, Validators.maxLength(50)]],
      releaseYear: [currentYear, [Validators.required, Validators.min(1950), Validators.max(currentYear + 2)]],
      rating: ['', [Validators.required, Validators.maxLength(30)]],
      description: ['', [Validators.maxLength(2000)]]
    });
  }

  private loadMetadata(): void {
    this.gameService.getMetadata().subscribe({
      next: (meta) => {
        this.platforms.set(meta.platforms);
        this.genres.set(meta.genres);
        this.ratings.set(meta.ratings);

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
        // Fallback default choices
        this.platforms.set(['PC', 'PlayStation 5', 'Xbox Series X/S', 'Nintendo Switch']);
        this.genres.set(['Action', 'Role-Playing (RPG)', 'Adventure', 'Strategy', 'Shooter']);
        this.ratings.set(['Everyone', 'Teen', 'Mature 17+']);
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

  onSubmit(): void {
    if (this.gameForm.invalid) {
      this.gameForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);

    const formValues = this.gameForm.value;

    if (this.isEditMode && this.gameId) {
      const request: UpdateGameRequest = {
        title: formValues.title,
        platform: formValues.platform,
        genre: formValues.genre,
        releaseYear: Number(formValues.releaseYear),
        rating: formValues.rating,
        description: formValues.description
      };

      this.gameService.updateGame(this.gameId, request).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.router.navigate(['/games']);
        },
        error: (err) => {
          this.errorMessage.set(err.error?.detail || 'Failed to update video game.');
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
        description: formValues.description
      };

      this.gameService.createGame(request).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.router.navigate(['/games']);
        },
        error: (err) => {
          this.errorMessage.set(err.error?.detail || 'Failed to create video game.');
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
