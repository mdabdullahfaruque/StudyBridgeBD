import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

// PrimeNG Modules for Auth
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DividerModule } from 'primeng/divider';

// Auth Components
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';

// Auth Routing
import { AuthRoutingModule } from './auth-routing.module';

@NgModule({
  declarations: [
    // Components will be imported here when converted from standalone
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule,
    
    // PrimeNG Modules
    InputTextModule,
    PasswordModule,
    ButtonModule,
    CardModule,
    CheckboxModule,
    ToastModule,
    ProgressSpinnerModule,
    DividerModule,
    
    // Feature Routing
    AuthRoutingModule
  ],
  providers: [
    // Auth-specific services and guards
  ],
  exports: [
    // Export components that might be used outside this module
  ]
})
export class AuthModule { }