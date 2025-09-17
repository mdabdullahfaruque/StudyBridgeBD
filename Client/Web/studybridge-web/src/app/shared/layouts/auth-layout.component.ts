import { Component } from '@angular/core';
import { BaseLayoutComponent, LayoutConfig } from './base-layout/base-layout.component';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [BaseLayoutComponent],
  template: `
    <app-base-layout [config]="authConfig">
    </app-base-layout>
  `
})
export class AuthLayoutComponent {
  authConfig: LayoutConfig = {
    showHeader: false,
    showSidebar: false,
    showFooter: true,
    sidebarType: 'minimal',
    containerClass: 'min-h-screen flex items-center justify-center p-4'
  };
}