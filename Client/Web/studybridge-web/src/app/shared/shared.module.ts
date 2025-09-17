import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';

// PrimeNG Modules - Common UI Components
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { ToolbarModule } from 'primeng/toolbar';
import { MenubarModule } from 'primeng/menubar';
import { AvatarModule } from 'primeng/avatar';
import { BadgeModule } from 'primeng/badge';
import { RippleModule } from 'primeng/ripple';
import { StyleClassModule } from 'primeng/styleclass';

// Shared Layout Components
import { BaseLayoutComponent } from './layouts/base-layout/base-layout.component';
import { AdminLayoutComponent } from './layouts/admin-layout.component';
import { PublicLayoutComponent } from './layouts/public-layout.component';
import { AuthLayoutComponent } from './layouts/auth-layout.component';
import { HeaderComponent } from './layouts/header/header.component';
import { SidebarComponent } from './layouts/sidebar/sidebar.component';

const PRIMENG_MODULES = [
  ButtonModule,
  InputTextModule,
  CardModule,
  DialogModule,
  ToastModule,
  ConfirmDialogModule,
  ProgressSpinnerModule,
  MessageModule,
  ToolbarModule,
  MenubarModule,
  AvatarModule,
  BadgeModule,
  RippleModule,
  StyleClassModule
];

const LAYOUT_COMPONENTS = [
  BaseLayoutComponent,
  AdminLayoutComponent,
  PublicLayoutComponent,
  AuthLayoutComponent,
  HeaderComponent,
  SidebarComponent
];

@NgModule({
  declarations: [
    // Shared components, pipes, and directives will be declared here when created
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule,
    HttpClientModule,
    ...PRIMENG_MODULES,
    ...LAYOUT_COMPONENTS
  ],
  exports: [
    // Export Angular modules
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule,
    HttpClientModule,
    
    // Export PrimeNG modules
    ...PRIMENG_MODULES,
    
    // Export Layout Components
    ...LAYOUT_COMPONENTS
    
    // Shared components, pipes, and directives will be exported here when created
  ],
  providers: [
    // Shared services will be provided here
  ]
})
export class SharedModule { }