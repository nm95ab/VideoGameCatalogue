import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NgbModal, NgbAlertModule } from '@ng-bootstrap/ng-bootstrap';
import { GameService } from '../../core/services/game.service';
import { Game } from '../../core/models/game.model';

@Component({
  selector: 'app-game-list',
  standalone: true,
  imports: [CommonModule, FormsModule, NgbAlertModule],
  templateUrl: './game-list.component.html',
  styleUrls: ['./game-list.component.scss']
})
export class GameListComponent implements OnInit {
  private readonly gameService = inject(GameService);
  private readonly router = inject(Router);
  private readonly modalService = inject(NgbModal);

  readonly games = signal<Game[]>([]);
  readonly isLoading = signal<boolean>(true);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  searchTerm = '';
  selectedPlatform = '';
  selectedGenre = '';

  readonly platforms = signal<string[]>([]);
  readonly genres = signal<string[]>([]);

  ngOnInit(): void {
    this.loadMetadata();
    this.loadGames();
  }

  loadMetadata(): void {
    this.gameService.getMetadata().subscribe({
      next: (meta) => {
        this.platforms.set(meta.platforms);
        this.genres.set(meta.genres);
      },
      error: () => {
        // Fallback gracefully
      }
    });
  }

  loadGames(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.gameService.getGames(this.searchTerm, this.selectedPlatform, this.selectedGenre).subscribe({
      next: (data) => {
        this.games.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set('Failed to load video games. Ensure the backend API is running.');
        this.isLoading.set(false);
      }
    });
  }

  onFilterChange(): void {
    this.loadGames();
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.selectedPlatform = '';
    this.selectedGenre = '';
    this.loadGames();
  }

  navigateToAdd(): void {
    this.router.navigate(['/games/new']);
  }

  navigateToEdit(id: string): void {
    this.router.navigate(['/games', id, 'edit']);
  }

  openDeleteModal(content: unknown, game: Game): void {
    this.modalService.open(content, { ariaLabelledBy: 'modal-title' }).result.then(
      (result) => {
        if (result === 'confirm') {
          this.executeDelete(game);
        }
      },
      () => {
        // dismissed
      }
    );
  }

  private executeDelete(game: Game): void {
    this.gameService.deleteGame(game.id).subscribe({
      next: () => {
        this.successMessage.set(`"${game.title}" was successfully deleted.`);
        this.loadGames();
      },
      error: () => {
        this.errorMessage.set(`Failed to delete "${game.title}".`);
      }
    });
  }
}
