import { NgModule } from '@angular/core';

// Shared Module
import { SharedModule } from '../../shared/shared.module';

// Admin-specific PrimeNG Modules
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { TreeModule } from 'primeng/tree';
import { CheckboxModule } from 'primeng/checkbox';
import { PanelModule } from 'primeng/panel';
import { ProgressBarModule } from 'primeng/progressbar';

// Admin Components
// import { AdminLayoutComponent } from './admin-layout/admin-layout.component';
import { AdminDashboardComponent } from './components/admin-dashboard/admin-dashboard.component';

// Admin Services
// import { AdminService } from './services/admin.service';

// Admin Routing
import { AdminRoutingModule } from './admin-routing.module';

@NgModule({
  declarations: [
    // Non-standalone components would go here
  ],
  imports: [
    // Shared Module (contains common Angular modules and PrimeNG modules)
    SharedModule,
    
    // Admin-specific PrimeNG Modules
    TableModule,
    TagModule,
    SelectModule,
    TreeModule,
    CheckboxModule,
    PanelModule,
    ProgressBarModule,
    
    // Feature Routing
    AdminRoutingModule
  ],
  providers: [
    // AdminService - will be added when service is created
    // Other admin-specific services
  ],
  exports: [
    // Export components that might be used outside this module
  ]
})
export class AdminModule { }