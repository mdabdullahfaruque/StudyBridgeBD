import { Routes } from '@angular/router';
import { RoleGuard } from '../guards/auth.guards';
import { AdminLayoutComponent } from './admin-layout/admin-layout.component';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    canActivate: [RoleGuard],
    data: { roles: ['Admin', 'SuperAdmin', 'Administrator'] },
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        component: AdminDashboardComponent,
        title: 'Admin Dashboard'
      },
      {
        path: 'users',
        loadComponent: () => import('./user-management/user-list/user-list.component').then(m => m.UserListComponent),
        title: 'User Management'
      },
      {
        path: 'roles',
        component: AdminDashboardComponent, // Temporary placeholder
        title: 'Role Management'
      },
      {
        path: 'permissions',
        component: AdminDashboardComponent, // Temporary placeholder
        title: 'Permission Management'
      },
      {
        path: 'content',
        component: AdminDashboardComponent, // Temporary placeholder
        title: 'Content Management'
      },
      {
        path: 'financials',
        component: AdminDashboardComponent, // Temporary placeholder
        title: 'Financial Management'
      },
      {
        path: 'system',
        component: AdminDashboardComponent, // Temporary placeholder
        title: 'System Management'
      }
    ]
  }
];

export default ADMIN_ROUTES;