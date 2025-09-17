import { NgModule } from '@angular/core';

// Shared Module
import { SharedModule } from '../../shared/shared.module';

// Public-specific PrimeNG Modules (if any)
import { DataViewModule } from 'primeng/dataview';
import { PaginatorModule } from 'primeng/paginator';
import { SkeletonModule } from 'primeng/skeleton';

// Public Routing
import { PublicRoutingModule } from './public-routing.module';

@NgModule({
  declarations: [
    // Non-standalone components would go here
  ],
  imports: [
    // Shared Module (contains common Angular modules and PrimeNG modules)
    SharedModule,
    
    // Public-specific PrimeNG Modules
    DataViewModule,
    PaginatorModule,
    SkeletonModule,
    
    // Feature Routing
    PublicRoutingModule
  ],
  providers: [
    // Public-specific services
  ],
  exports: [
    // Export components that might be used outside this module
  ]
})
export class PublicModule { }