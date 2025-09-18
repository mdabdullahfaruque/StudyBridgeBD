import { Routes } from '@angular/router';
import { RoleGuard } from '../../core/guards/auth.guards';
import { AdminLayoutComponent } from '../../shared/layouts/admin-layout/admin-layout.component';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard.component';
import { RoleListComponent } from './components/role-management/role-list.component';
import { UserListComponent } from './components/user-management/user-list.component';
import { MenuListComponent } from './components/menu-management/menu-list.component';

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
        component: UserListComponent,
        title: 'User Management'
      },
      {
        path: 'roles',
        component: RoleListComponent,
        title: 'Role Management'
      },
      {
        path: 'menus',
        component: MenuListComponent,
        title: 'Menu Management'
      },
      {
        path: 'permissions',
        component: AdminDashboardComponent,
        title: 'Permission Management'
      },
      {
        path: 'management',
        component: AdminDashboardComponent,
        title: 'Admin Management'
      },
      {
        path: 'content',
        component: AdminDashboardComponent,
        title: 'Content Management'
      },
      {
        path: 'financials',
        component: AdminDashboardComponent,
        title: 'Financial Management'
      },
      {
        path: 'system',
        component: AdminDashboardComponent,
        title: 'System Management'
      }
    ]
  }
];

export default ADMIN_ROUTES;
