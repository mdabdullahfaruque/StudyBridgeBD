import { Component } from '@angular/core';
import { BaseLayoutComponent, LayoutConfig } from './base-layout/base-layout.component';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [BaseLayoutComponent],
  template: `
    <app-base-layout [config]="publicConfig">
    </app-base-layout>
  `
})
export class PublicLayoutComponent {
  publicConfig: LayoutConfig = {
    showHeader: true,
    showSidebar: true,
    showFooter: true,
    sidebarType: 'public',
    containerClass: 'p-4 lg:p-6'
  };
}