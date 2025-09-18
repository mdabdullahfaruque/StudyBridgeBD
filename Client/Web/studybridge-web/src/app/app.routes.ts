import { Routes } from '@angular/router';
import { AuthGuard, GuestGuard } from './core/guards/auth.guards';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./core/components/dashboard-redirect.component').then(m => m.DashboardRedirectComponent)
    // No guard here - let DashboardRedirectComponent handle the auth check and redirect
  },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.module').then(m => m.AuthModule),
    canActivate: [GuestGuard]
  },
  {
    path: 'public',
    loadChildren: () => import('./features/public/public.module').then(m => m.PublicModule),
    canActivate: [AuthGuard]
  },
  {
    path: 'admin',
    loadChildren: () => import('./features/admin/admin.module').then(m => m.AdminModule),
    canActivate: [AuthGuard]
  },
  {
    path: '**',
    redirectTo: '/auth'
  }
];
