import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'games',
    pathMatch: 'full'
  },
  {
    path: 'games',
    loadComponent: () => import('./pages/game-list/game-list.component').then(m => m.GameListComponent)
  },
  {
    path: 'games/new',
    loadComponent: () => import('./pages/game-edit/game-edit.component').then(m => m.GameEditComponent)
  },
  {
    path: 'games/:id/edit',
    loadComponent: () => import('./pages/game-edit/game-edit.component').then(m => m.GameEditComponent)
  },
  {
    path: '**',
    redirectTo: 'games'
  }
];
