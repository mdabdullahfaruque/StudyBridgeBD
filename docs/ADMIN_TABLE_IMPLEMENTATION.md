# Admin Table Implementation Guide

This document outlines the standardized process for implementing data tables in the admin section of StudyBridge. Follow this pattern for consistent, maintainable admin interfaces.

## 📋 Overview

All admin tables use the centralized `app-table-wrapper` component that provides:
- Comprehensive PrimeNG Table functionality
- Consistent UI/UX across all admin pages
- Built-in filtering, sorting, and pagination
- Export capabilities
- Responsive design

## 🏗️ Implementation Pattern

### 1. Component Structure

Create admin list components following this structure:

```
src/app/features/admin/components/{feature}-management/
├── {feature}-list.component.ts
├── {feature}-list.component.html
├── {feature}-list.component.scss
└── {feature}-create.component.ts (if needed)
```

### 2. Component Implementation

#### TypeScript Component Template

```typescript
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

// PrimeNG Table Wrapper - MANDATORY per UI Components Guide
import { TableWrapperComponent, TableColumn, TableConfig } from '../../../../shared/table-wrapper/table-wrapper.component';

// API Service and Models
import { YourApiService } from '../../../../shared/services/your-api.service';
import { YourDto, ApiResponse } from '../../../../shared/models/api.models';

// Toast Service for notifications
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-{feature}-list',
  standalone: true,
  imports: [
    CommonModule,
    TableWrapperComponent  // Using existing PrimeNG table wrapper as required
  ],
  templateUrl: './{feature}-list.component.html',
  styleUrl: './{feature}-list.component.scss'
})
export class FeatureListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Table data and configuration
  items: YourDto[] = [];
  processedItems: any[] = [];
  selectedItems: YourDto[] = [];
  loading = false;

  // Table column configuration
  columns: TableColumn[] = [
    {
      field: 'name',
      header: 'Name',
      type: 'text',
      sortable: true,
      filterable: true,
      filterType: 'text',
      width: '200px'
    },
    {
      field: 'statusDisplay',
      header: 'Status',
      type: 'status',
      sortable: true,
      filterable: true,
      filterType: 'dropdown',
      filterOptions: [
        { label: 'Active', value: 'Active' },
        { label: 'Inactive', value: 'Inactive' }
      ],
      width: '120px'
    }
    // Add more columns as needed
  ];

  // Table configuration
  tableConfig: TableConfig = {
    serverSide: false,
    loading: false,
    paginator: true,
    rows: 10,
    rowsPerPageOptions: [5, 10, 25, 50],
    selectionMode: 'multiple',
    sortMode: 'single',
    globalFilterFields: ['name', 'description'],
    filterMode: 'lenient',
    resizableColumns: true,
    reorderableColumns: true,
    exportable: true,
    responsive: true,
    striped: true,
    size: 'normal',
    showGridlines: true,
    showHeader: true
  };

  constructor(
    private apiService: YourApiService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.processedItems = [];
    this.loadItems();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Load items from API
   */
  private loadItems(): void {
    this.loading = true;
    this.tableConfig = { ...this.tableConfig, loading: true };

    this.apiService.getItems()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: ApiResponse<any>) => {
          if (response.success && response.data) {
            // Handle nested structure where items are in response.data.items
            if (response.data.items && Array.isArray(response.data.items)) {
              this.items = response.data.items;
            } else if (Array.isArray(response.data)) {
              this.items = response.data;
            } else {
              console.error('Unexpected data structure:', response.data);
              this.handleError('Failed to load items', 'Unexpected data structure received from API');
              return;
            }
            
            this.processItemsForDisplay();
            this.toastService.success('Success', `Loaded ${this.items.length} items`);
          } else {
            this.handleError('Failed to load items', response.message || 'Unknown error from API');
          }
        },
        error: (error) => {
          this.handleError('Error loading items', error.message || 'Unknown error occurred');
        },
        complete: () => {
          this.loading = false;
          this.tableConfig = { ...this.tableConfig, loading: false };
        }
      });
  }

  /**
   * Process items data for display in the table
   */
  private processItemsForDisplay(): void {
    if (!Array.isArray(this.items)) {
      console.warn('Items data is not an array, initializing empty array:', this.items);
      this.processedItems = [];
      return;
    }

    this.processedItems = this.items.map(item => ({
      ...item,
      statusDisplay: this.getStatusDisplay(item.isActive),
      // Add other computed fields as needed
    }));
  }

  /**
   * Handle API errors with user feedback
   */
  private handleError(title: string, message?: string): void {
    this.loading = false;
    this.tableConfig = { ...this.tableConfig, loading: false };
    this.toastService.error(title, message || 'Please try again later');
  }

  // Event handlers
  onSelectionChange(selectedItems: YourDto[]): void {
    this.selectedItems = selectedItems;
  }

  onRowSelect(event: any): void {
    const selectedItem = event.data as YourDto;
    console.log('Selected item:', selectedItem);
  }

  // Helper methods
  getStatusDisplay(isActive: boolean): string {
    return isActive ? 'Active' : 'Inactive';
  }
}
```

#### HTML Template Pattern

```html
<div class="feature-list-container p-6">
  <!-- Page Header -->
  <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
    <div>
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">Feature Management</h1>
      <p class="text-gray-600 dark:text-gray-300">Manage system features</p>
    </div>
    
    <!-- Action Buttons -->
    <div class="flex gap-2">
      <button 
        type="button"
        class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors"
        (click)="createNewItem()"
        title="Create New Item">
        <i class="pi pi-plus mr-2"></i>
        New Item
      </button>
      
      <button 
        type="button"
        class="px-4 py-2 bg-gray-600 hover:bg-gray-700 text-white rounded-lg font-medium transition-colors"
        (click)="refreshItems()"
        title="Refresh Items List">
        <i class="pi pi-refresh mr-2"></i>
        Refresh
      </button>
    </div>
  </div>

  <!-- Selection Summary -->
  <div *ngIf="selectedItems.length > 0" class="mb-4 p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg border border-blue-200 dark:border-blue-800">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
      <div class="flex items-center gap-2">
        <i class="pi pi-info-circle text-blue-600"></i>
        <span class="text-blue-800 dark:text-blue-200 font-medium">
          {{ selectedItems.length }} item{{ selectedItems.length === 1 ? '' : 's' }} selected
        </span>
      </div>
      
      <div class="flex gap-2">
        <button 
          type="button"
          class="px-3 py-1.5 bg-red-600 hover:bg-red-700 text-white rounded font-medium transition-colors text-sm"
          (click)="selectedItems = []"
          title="Clear Selection">
          <i class="pi pi-times mr-1"></i>
          Clear Selection
        </button>
      </div>
    </div>
  </div>

  <!-- MANDATORY: Using app-table-wrapper -->
  <app-table-wrapper
    [data]="processedItems"
    [columns]="columns"
    [config]="tableConfig"
    [selectedItems]="selectedItems"
    (selectedItemsChange)="onSelectionChange($event)"
    (onRowSelect)="onRowSelect($event)"
    class="items-table">
  </app-table-wrapper>

  <!-- Loading State -->
  <div *ngIf="loading" class="fixed inset-0 bg-black bg-opacity-25 flex items-center justify-center z-50">
    <div class="bg-white dark:bg-gray-800 p-6 rounded-lg shadow-xl flex items-center gap-3">
      <i class="pi pi-spinner animate-spin text-blue-600 text-xl"></i>
      <span class="text-gray-700 dark:text-gray-300 font-medium">Loading items...</span>
    </div>
  </div>

  <!-- Empty State -->
  <div *ngIf="!loading && processedItems.length === 0" class="text-center py-12">
    <div class="inline-flex items-center justify-center w-16 h-16 bg-gray-100 dark:bg-gray-700 rounded-full mb-4">
      <i class="pi pi-list text-2xl text-gray-400"></i>
    </div>
    <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">No items found</h3>
    <p class="text-gray-500 dark:text-gray-400 mb-6">Get started by creating your first item.</p>
    <button 
      type="button"
      class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors"
      (click)="createNewItem()">
      <i class="pi pi-plus mr-2"></i>
      Create First Item
    </button>
  </div>
</div>
```

### 3. Column Types and Configuration

#### Supported Column Types

```typescript
type ColumnType = 'text' | 'number' | 'date' | 'boolean' | 'status' | 'enum' | 'custom';
```

#### Column Configuration Examples

```typescript
// Text column
{
  field: 'name',
  header: 'Name',
  type: 'text',
  sortable: true,
  filterable: true,
  filterType: 'text',
  width: '200px'
}

// Status column with badges
{
  field: 'statusDisplay',
  header: 'Status',
  type: 'status',
  sortable: true,
  filterable: true,
  filterType: 'dropdown',
  filterOptions: [
    { label: 'Active', value: 'Active' },
    { label: 'Inactive', value: 'Inactive' }
  ],
  width: '120px'
}

// Number column
{
  field: 'count',
  header: 'Count',
  type: 'number',
  sortable: true,
  width: '100px'
}

// Date column
{
  field: 'createdAt',
  header: 'Created',
  type: 'date',
  sortable: true,
  width: '150px'
}

// Boolean column
{
  field: 'isActive',
  header: 'Active',
  type: 'boolean',
  sortable: true,
  width: '80px'
}
```

### 4. API Response Handling

#### Standard API Response Structure

```typescript
interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}
```

#### Handle Nested Response Data

```typescript
// Backend may return nested structure
if (response.data.items && Array.isArray(response.data.items)) {
  this.items = response.data.items;
} else if (Array.isArray(response.data)) {
  this.items = response.data;
} else {
  // Handle unexpected structure
  this.handleError('Failed to load items', 'Unexpected data structure received from API');
}
```

### 5. Data Processing

#### Transform Raw Data for Display

```typescript
private processItemsForDisplay(): void {
  if (!Array.isArray(this.items)) {
    console.warn('Items data is not an array:', this.items);
    this.processedItems = [];
    return;
  }

  this.processedItems = this.items.map(item => ({
    ...item,
    statusDisplay: this.getStatusDisplay(item.isActive),
    typeDisplay: this.getTypeDisplay(item.type),
    formattedDate: this.formatDate(item.createdAt),
    // Add other computed fields
  }));
}
```

### 6. Routing Integration

#### Add to Admin Routes

```typescript
// admin.routes.ts
{
  path: 'features',
  loadComponent: () => import('./components/feature-management/feature-list.component')
    .then(m => m.FeatureListComponent),
  canActivate: [AuthGuard],
  data: { 
    requiredPermission: 'features.view',
    breadcrumb: 'Features'
  }
}
```

#### Update Admin Menu

```typescript
// admin-sidebar.component.ts
{
  label: 'Features',
  icon: 'pi pi-cog',
  routerLink: '/admin/features',
  permission: 'features.view'
}
```

## 🔧 Troubleshooting

### Common Issues and Solutions

1. **All data showing in first column**
   - Ensure column fields match processed data properties
   - Check that `getFieldValue()` returns correct values
   - Verify table template structure

2. **API response structure mismatch**
   - Log the actual response structure: `console.log('API Response:', response)`
   - Handle both nested and flat response formats
   - Add proper error handling for unexpected structures

3. **Column filtering not working**
   - Verify `filterType` matches column data type
   - Ensure `filterOptions` are properly configured
   - Check that filterable fields exist in data

4. **Performance issues with large datasets**
   - Enable server-side pagination: `serverSide: true`
   - Implement lazy loading in API service
   - Limit initial data load

## 📚 Reference Implementation

See the complete implementation in:
- `src/app/features/admin/components/role-management/role-list.component.ts`
- `src/app/shared/table-wrapper/table-wrapper.component.ts`

## 🎯 Next Steps

Use this pattern to implement:
1. User Management Table (`/admin/users`)
2. Permission Management Table (`/admin/permissions`) 
3. Audit Log Table (`/admin/audit`)
4. System Settings Table (`/admin/settings`)

Each implementation should follow this exact pattern for consistency and maintainability.