import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PublicLayoutComponent } from '../../shared/layouts/public-layout/public-layout.component';

const routes: Routes = [
  {
    path: '',
    component: PublicLayoutComponent,
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./components/dashboard/dashboard.component').then(m => m.DashboardComponent),
        title: 'Dashboard - StudyBridge'
      },
      {
        path: 'vocabulary',
        loadComponent: () => import('./components/dashboard/dashboard.component').then(m => m.DashboardComponent), // Placeholder until vocabulary component is created
        title: 'Vocabulary - StudyBridge'
      },
      {
        path: 'learning',
        loadComponent: () => import('./components/dashboard/dashboard.component').then(m => m.DashboardComponent), // Placeholder until learning component is created
        title: 'Learning - StudyBridge'
      }
      // More routes will be added when components are organized
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PublicRoutingModule { }