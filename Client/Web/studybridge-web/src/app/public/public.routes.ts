import { Routes } from '@angular/router';
import { AuthGuard } from '../guards/auth.guards';

export const publicRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./public-layout/public-layout.component').then(m => m.PublicLayoutComponent),
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./public-dashboard/public-dashboard.component').then(m => m.PublicDashboardComponent)
      },
      {
        path: 'vocabulary',
        loadComponent: () => import('./vocabulary/public-vocabulary.component').then(m => m.PublicVocabularyComponent)
      },
      {
        path: 'learning',
        loadComponent: () => import('./learning/public-learning.component').then(m => m.PublicLearningComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./profile/public-profile.component').then(m => m.PublicProfileComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./settings/public-settings.component').then(m => m.PublicSettingsComponent)
      }
    ]
  }
];

export default publicRoutes;