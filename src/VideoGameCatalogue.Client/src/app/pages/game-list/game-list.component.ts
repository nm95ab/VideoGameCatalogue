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
import { Game, GamingEraInfo, PagedResult } from '../../core/models/game.model';

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
  private readonly pageChangeSubject = new Subject<void>();

  readonly games = signal<Game[]>([]);
  readonly totalCount = signal<number>(0);
  readonly totalPages = signal<number>(0);
  readonly isLoading = signal<boolean>(true);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  searchTerm = '';
  selectedPlatform = '';
  selectedGenre = '';
  selectedEra = '';

  readonly page = signal<number>(1);
  readonly pageSize = signal<number>(6);
  readonly pageSizeOptions = [6, 12, 24];

  readonly startItemIndex = computed(() => {
    if (this.totalCount() === 0) return 0;
    return (this.page() - 1) * this.pageSize() + 1;
  });

  readonly endItemIndex = computed(() => {
    return Math.min(this.page() * this.pageSize(), this.totalCount());
  });

  readonly platforms = signal<string[]>([]);
  readonly genres = signal<string[]>([]);
  readonly eras = signal<GamingEraInfo[]>([]);

  constructor() {
    this.setupReactivePipeline();
  }

  ngOnInit(): void {
    this.loadMetadata();
    this.loadGames();
  }

  /**
   * Configures the unified reactive query pipeline for searching, filtering, and server-side paging.
   */
  private setupReactivePipeline(): void {
    const debouncedSearch$ = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    );

    const immediateFilter$ = this.filterChangeSubject.asObservable();
    const pageChange$ = this.pageChangeSubject.asObservable();

    merge(debouncedSearch$, immediateFilter$, pageChange$)
      .pipe(
        tap(() => {
          this.isLoading.set(true);
          this.errorMessage.set(null);
        }),
        switchMap(() =>
          this.gameService.getGames(
            this.searchTerm,
            this.selectedPlatform,
            this.selectedGenre,
            this.page(),
            this.pageSize(),
            this.selectedEra
          ).pipe(
            catchError(() => {
              this.errorMessage.set('Failed to load video games. Ensure the backend API is running.');
              return of<PagedResult<Game>>({
                items: [],
                pageNumber: this.page(),
                pageSize: this.pageSize(),
                totalCount: 0,
                totalPages: 0,
                hasPreviousPage: false,
                hasNextPage: false
              });
            })
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((result) => {
        this.games.set(result.items);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
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
          if (meta.eras) {
            this.eras.set(meta.eras);
          }
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
    this.selectedEra = '';
    this.page.set(1);
    this.filterChangeSubject.next();
  }

  onPageChange(newPage: number): void {
    this.page.set(newPage);
    this.pageChangeSubject.next();
  }

  onPageSizeChange(newSize: number): void {
    this.pageSize.set(newSize);
    this.page.set(1);
    this.pageChangeSubject.next();
  }

  navigateToAdd(): void {
    this.router.navigate(['/games/new']);
  }

  navigateToEdit(id: string): void {
    this.router.navigate(['/games', id, 'edit']);
  }

  getImageUrl(imageId: string, directUrl?: string | null): string {
    return this.gameService.getImageUrl(imageId, directUrl);
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

  onImageError(event: Event, title: string): void {
    const target = event.target as HTMLElement;
    if (target && target.parentElement) {
      const placeholder = document.createElement('div');
      placeholder.className = 'rounded bg-primary-subtle text-primary border d-flex align-items-center justify-content-center fw-bold small shadow-sm flex-shrink-0';
      placeholder.style.width = '44px';
      placeholder.style.height = '44px';
      placeholder.title = title;
      placeholder.textContent = this.getInitials(title);
      target.parentElement.replaceChild(placeholder, target);
    }
  }
}
