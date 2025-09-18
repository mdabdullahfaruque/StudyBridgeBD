import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminGuard } from '../../core/guards/auth.guards';
import { AdminLayoutComponent } from '../../shared/layouts/admin-layout/admin-layout.component';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard.component';

const routes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    canActivate: [AdminGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        component: AdminDashboardComponent,
        title: 'Admin Dashboard - StudyBridge'
      },
      {
        path: 'users',
        component: AdminDashboardComponent, // Placeholder until components are created
        title: 'User Management - StudyBridge'
      },
      {
        path: 'roles',
        component: AdminDashboardComponent, // Placeholder until components are created
        title: 'Role Management - StudyBridge'
      },
      {
        path: 'permissions',
        component: AdminDashboardComponent, // Placeholder until components are created
        title: 'Permission Management - StudyBridge'
      },
      {
        path: 'management',
        component: AdminDashboardComponent, // Placeholder until components are created
        title: 'Admin Management - StudyBridge'
      },
      {
        path: 'content',
        component: AdminDashboardComponent, // Temporary placeholder
        title: 'Content Management - StudyBridge'
      },
      {
        path: 'financials',
        component: AdminDashboardComponent, // Temporary placeholder
        title: 'Financial Management - StudyBridge'
      },
      {
        path: 'system',
        component: AdminDashboardComponent, // Temporary placeholder
        title: 'System Management - StudyBridge'
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }