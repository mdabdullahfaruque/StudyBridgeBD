import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { SharedRoutingModule } from './shared-routing-module';

// Re-export standalone components
export { HeaderComponent } from './components/header/header';
export { LoadingComponent } from './components/loading/loading';
export { ButtonComponent } from './components/button/button';

const MODULES = [
  CommonModule,
  RouterModule,
  ReactiveFormsModule,
  FormsModule
];

@NgModule({
  imports: [
    ...MODULES,
    SharedRoutingModule
  ],
  exports: [
    ...MODULES
  ]
})
export class SharedModule { }
