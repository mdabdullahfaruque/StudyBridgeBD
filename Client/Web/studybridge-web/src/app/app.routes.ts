import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guards';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'auth/login',
    loadComponent: () => import('./auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'auth/register',
    loadComponent: () => import('./auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [AuthGuard]
  },

  {
    path: 'admin',
    loadChildren: () => import('./admin/admin.routes'),
    canActivate: [AuthGuard]
  },
  {
    path: '**',
    redirectTo: '/auth/login'
  }
];
