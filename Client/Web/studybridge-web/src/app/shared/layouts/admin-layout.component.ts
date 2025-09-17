import { Component } from '@angular/core';
import { BaseLayoutComponent, LayoutConfig } from './base-layout/base-layout.component';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [BaseLayoutComponent],
  template: `
    <app-base-layout [config]="adminConfig">
    </app-base-layout>
  `
})
export class AdminLayoutComponent {
  adminConfig: LayoutConfig = {
    showHeader: true,
    showSidebar: true,
    showFooter: false,
    sidebarType: 'admin',
    containerClass: 'p-4 lg:p-6'
  };
}