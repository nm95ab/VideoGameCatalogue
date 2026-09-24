import { Component, OnInit, inject, signal, computed, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, merge, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, tap, catchError } from 'rxjs/operators';
import {
  NgbAlertModule,
  NgbPaginationModule,
  NgbProgressbarModule,
  NgbDropdownModule
} from '@ng-bootstrap/ng-bootstrap';
import { GameService } from '../../core/services/game.service';
import { Game } from '../../core/models/game.model';

@Component({
  selector: 'app-game-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbAlertModule,
    NgbPaginationModule,
    NgbProgressbarModule,
    NgbDropdownModule
  ],
  templateUrl: './game-list.component.html',
  styleUrls: ['./game-list.component.scss']
})
export class GameListComponent implements OnInit {
  private readonly gameService = inject(GameService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  private readonly searchSubject = new Subject<string>();
  private readonly filterChangeSubject = new Subject<void>();

  readonly games = signal<Game[]>([]);
  readonly isLoading = signal<boolean>(true);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  searchTerm = '';
  selectedPlatform = '';
  selectedGenre = '';

  readonly page = signal<number>(1);
  readonly pageSize = signal<number>(6);
  readonly pageSizeOptions = [6, 12, 24];

  readonly pagedGames = computed(() => {
    const list = this.games();
    const start = (this.page() - 1) * this.pageSize();
    return list.slice(start, start + this.pageSize());
  });

  readonly platforms = signal<string[]>([]);
  readonly genres = signal<string[]>([]);

  constructor() {
    this.setupReactivePipeline();
  }

  ngOnInit(): void {
    this.loadMetadata();
    this.loadGames();
  }

  /**
   * Configures the unified reactive query pipeline for searching and filtering games.
   *
   * Design Decisions & Architectural Highlights:
   * 1. Debounced Search: Throttles rapid keystrokes (`debounceTime(300)`) and ignores duplicate values (`distinctUntilChanged`).
   * 2. Immediate Filters: Dropdown selections (Platform/Genre) trigger immediate fetches without artificial delay.
   * 3. SwitchMap Cancellation: Automatically cancels pending in-flight HTTP requests when new filter/search criteria arrive,
   *    guaranteeing that slow, stale responses never overwrite newer search results (prevents out-of-order race conditions).
   * 4. Automatic Teardown: Uses `takeUntilDestroyed(this.destroyRef)` to eliminate memory leaks upon component destruction.
   */
  private setupReactivePipeline(): void {
    const debouncedSearch$ = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    );

    const immediateFilter$ = this.filterChangeSubject.asObservable();

    merge(debouncedSearch$, immediateFilter$)
      .pipe(
        tap(() => {
          this.isLoading.set(true);
          this.errorMessage.set(null);
        }),
        switchMap(() =>
          this.gameService.getGames(this.searchTerm, this.selectedPlatform, this.selectedGenre).pipe(
            catchError(() => {
              this.errorMessage.set('Failed to load video games. Ensure the backend API is running.');
              return of([] as Game[]);
            })
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((games) => {
        this.games.set(games);
        this.isLoading.set(false);
      });
  }

  loadMetadata(): void {
    this.gameService.getMetadata()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
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
    this.filterChangeSubject.next();
  }

  onSearchInput(term: string): void {
    this.searchTerm = term;
    this.page.set(1);
    this.searchSubject.next(term);
  }

  onFilterChange(): void {
    this.page.set(1);
    this.filterChangeSubject.next();
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.selectedPlatform = '';
    this.selectedGenre = '';
    this.page.set(1);
    this.filterChangeSubject.next();
  }

  navigateToAdd(): void {
    this.router.navigate(['/games/new']);
  }

  navigateToEdit(id: string): void {
    this.router.navigate(['/games', id, 'edit']);
  }
}
