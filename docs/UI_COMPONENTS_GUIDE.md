# 🎨 StudyBridge UI Components Guide

## 📋 Mandatory Reading - Critical Requirements

> **⚠️ CRITICAL: This document MUST be read and understood before working on ANY task in this project.**

### 🚫 Absolute Prohibitions

1. **NEVER create new Table components** - We have `app-table-wrapper` with comprehensive PrimeNG Table functionality
2. **NEVER create new Form components** - We have `app-dynamic-form` with full PrimeNG Form capabilities  
3. **NEVER create new Tree components** - We have `app-tree-wrapper` with complete PrimeNG Tree implementation
4. **NEVER use any UI library other than PrimeNG** - All components must use PrimeNG modules

### 🏗️ Established UI Component Architecture

StudyBridge follows a **PrimeNG-first** approach with comprehensive wrapper components that provide consistent functionality across the entire application.

## 📊 Table Component - `app-table-wrapper`

**Location**: `/src/app/shared/table-wrapper/table-wrapper.component.ts`

### Purpose
Comprehensive PrimeNG Table wrapper for ALL table usage in the application.

### Key Features
- **Server-side & Client-side data**: Configurable lazy loading with `TableLazyLoadEvent`
- **Advanced Pagination**: Customizable page sizes, total record tracking, navigation controls
- **Multi-level Sorting**: Single and multiple column sorting with sort indicators
- **Comprehensive Filtering**: 
  - Global search across all columns
  - Column-specific filters (text, dropdown, date, number, multiselect)
  - Filter type configuration per column
- **Selection Modes**: Single, multiple, and checkbox selection with event handling
- **Export Functionality**: CSV and Excel export capabilities
- **Column Management**: 
  - Resizable and reorderable columns
  - Frozen columns support
  - Hidden/visible column toggling
  - Custom column widths
- **Responsive Design**: Mobile-friendly with configurable breakpoints
- **Accessibility**: Full keyboard navigation and screen reader support

### Core Interfaces

```typescript
export interface TableColumn {
  field: string;                    // Data property path
  header: string;                   // Display header text
  type?: 'text' | 'number' | 'date' | 'boolean' | 'enum' | 'custom';
  sortable?: boolean;               // Enable sorting for this column
  filterable?: boolean;             // Enable filtering for this column
  filterType?: 'text' | 'dropdown' | 'date' | 'number' | 'multiselect';
  filterOptions?: any[];            // Options for dropdown/multiselect filters
  width?: string;                   // Column width (CSS units)
  frozen?: boolean;                 // Freeze column to left/right
  hidden?: boolean;                 // Hide column initially
  customTemplate?: string;          // Custom template reference
}

export interface TableConfig {
  // Data Management
  serverSide?: boolean;             // Enable server-side operations
  loading?: boolean;                // Show loading spinner
  
  // Pagination
  paginator?: boolean;              // Enable pagination
  rows?: number;                    // Records per page
  rowsPerPageOptions?: number[];    // Page size options
  totalRecords?: number;            // Total record count (server-side)
  
  // Selection
  selectionMode?: 'single' | 'multiple' | 'checkbox';
  
  // Sorting & Filtering
  sortMode?: 'single' | 'multiple';
  globalFilterFields?: string[];    // Fields for global search
  filterMode?: 'lenient' | 'strict';
  
  // Features
  resizableColumns?: boolean;       // Allow column resizing
  reorderableColumns?: boolean;     // Allow column reordering
  scrollable?: boolean;             // Enable scrolling
  scrollHeight?: string;            // Fixed height for scrolling
  exportable?: boolean;             // Enable export buttons
  
  // Styling
  responsive?: boolean;             // Mobile responsiveness
  striped?: boolean;                // Alternating row colors
  size?: 'small' | 'normal' | 'large';
  showGridlines?: boolean;          // Show cell borders
}
```

### Usage Example

```typescript
// Component Setup
columns: TableColumn[] = [
  { 
    field: 'name', 
    header: 'Name', 
    type: 'text', 
    sortable: true, 
    filterable: true 
  },
  { 
    field: 'email', 
    header: 'Email', 
    type: 'text', 
    sortable: true, 
    filterable: true,
    filterType: 'text'
  },
  { 
    field: 'createdAt', 
    header: 'Created Date', 
    type: 'date', 
    sortable: true 
  }
];

config: TableConfig = {
  serverSide: true,
  paginator: true,
  rows: 10,
  rowsPerPageOptions: [5, 10, 25, 50],
  selectionMode: 'multiple',
  exportable: true,
  resizableColumns: true
};

// Template Usage
<app-table-wrapper
  [data]="users"
  [columns]="columns"
  [config]="config"
  [selectedItems]="selectedUsers"
  (selectedItemsChange)="onSelectionChange($event)"
  (onLazyLoad)="loadUsersLazy($event)"
  (onRowSelect)="onUserSelect($event)"
  (onExport)="handleExport($event)">
</app-table-wrapper>
```

## 📝 Form Component - `app-dynamic-form`

**Location**: `/src/app/shared/dynamic-form/dynamic-form.component.ts`

### Purpose
Comprehensive PrimeNG Form wrapper for ALL form usage in the application.

### Key Features
- **Dynamic Field Generation**: Create forms from configuration objects
- **Comprehensive Field Types**: Text, email, password, number, textarea, select, multiselect, checkbox, radio, date, toggle, hidden
- **Advanced Validation**: Built-in and custom validators with real-time feedback
- **Conditional Logic**: Show/hide and enable/disable fields based on other field values
- **Multiple Layouts**: Vertical, horizontal, grid, and inline layouts
- **Responsive Design**: Configurable grid columns and mobile adaptation
- **Accessibility**: Full ARIA support and keyboard navigation

### Core Interfaces

```typescript
export interface FormField {
  key: string;                      // Unique field identifier
  type: 'text' | 'email' | 'password' | 'number' | 'textarea' | 'select' | 'multiselect' | 'checkbox' | 'radio' | 'date' | 'toggle' | 'hidden';
  label?: string;                   // Display label
  placeholder?: string;             // Input placeholder
  required?: boolean;               // Mark as required
  disabled?: boolean;               // Disable field
  readonly?: boolean;               // Make field readonly
  options?: Array<{label: string; value: any}>; // Options for select/radio
  validation?: FormFieldValidation; // Validation rules
  conditionalLogic?: FormFieldConditionalLogic; // Conditional behavior
  defaultValue?: any;               // Initial value
  helpText?: string;                // Help text below field
  colSpan?: number;                 // Grid column span
}

export interface FormConfig {
  title?: string;                   // Form title
  description?: string;             // Form description
  layout?: 'vertical' | 'horizontal' | 'grid' | 'inline';
  columns?: number;                 // Grid columns (for grid layout)
  showSubmitButton?: boolean;       // Show submit button
  showResetButton?: boolean;        // Show reset button
  submitButtonText?: string;        // Submit button text
  validationMode?: 'onChange' | 'onSubmit' | 'onBlur';
  actionButtonAlignment?: 'left' | 'center' | 'right' | 'space-between';
}
```

### Usage Example

```typescript
// Component Setup
formFields: FormField[] = [
  {
    key: 'firstName',
    type: 'text',
    label: 'First Name',
    required: true,
    validation: { required: true, minLength: 2 }
  },
  {
    key: 'email',
    type: 'email',
    label: 'Email Address',
    required: true,
    validation: { required: true, email: true }
  },
  {
    key: 'role',
    type: 'select',
    label: 'User Role',
    options: [
      { label: 'Admin', value: 'admin' },
      { label: 'User', value: 'user' }
    ]
  }
];

formConfig: FormConfig = {
  layout: 'grid',
  columns: 2,
  showSubmitButton: true,
  submitButtonText: 'Create User',
  validationMode: 'onChange'
};

// Template Usage
<app-dynamic-form
  [fields]="formFields"
  [config]="formConfig"
  [initialData]="userData"
  (formSubmit)="onFormSubmit($event)"
  (fieldChange)="onFieldChange($event)">
</app-dynamic-form>
```

## 🌳 Tree Component - `app-tree-wrapper`

**Location**: `/src/app/shared/tree-wrapper/tree-wrapper.component.ts`

### Purpose
Comprehensive PrimeNG Tree wrapper for ALL tree/hierarchical data usage in the application.

### Key Features
- **Selection Modes**: Single, multiple, and checkbox selection
- **Dynamic Counts**: Real-time selected/total count display on parent nodes
- **Expand/Collapse**: Individual node and bulk expand/collapse operations
- **Node Statistics**: Total nodes, selected nodes, parent nodes, leaf nodes
- **State Preservation**: Maintains expanded state during updates
- **Event Handling**: Complete node lifecycle events (select, expand, collapse)

### Core Interfaces

```typescript
export interface TreeConfig {
  selectionMode?: 'single' | 'multiple' | 'checkbox';
  showHeader?: boolean;             // Show tree header with title and controls
  showCounts?: boolean;             // Show selection counts on parent nodes
  showControls?: boolean;           // Show expand/collapse all buttons
  showStats?: boolean;              // Show statistics panel
  headerTitle?: string;             // Custom header title
  expandAll?: boolean;              // Initially expand all nodes
  minimal?: boolean;                // Minimal styling mode
}

export interface TreeCounts {
  totalNodes: number;               // Total number of nodes
  selectedNodes: number;            // Number of selected nodes
  parentNodes: number;              // Number of parent nodes
  leafNodes: number;                // Number of leaf nodes
}
```

### Usage Example

```typescript
// Component Setup
treeConfig: TreeConfig = {
  selectionMode: 'checkbox',
  showHeader: true,
  showCounts: true,
  showControls: true,
  headerTitle: 'Permission Tree',
  expandAll: false
};

treeData: TreeNode[] = [
  {
    key: 'user-management',
    label: 'User Management',
    children: [
      { key: 'user-create', label: 'Create User' },
      { key: 'user-edit', label: 'Edit User' },
      { key: 'user-delete', label: 'Delete User' }
    ]
  }
];

// Template Usage
<app-tree-wrapper
  [treeData]="treeData"
  [config]="treeConfig"
  [selectedNodes]="selectedPermissions"
  (selectedNodesChange)="onPermissionSelectionChange($event)"
  (nodeSelect)="onNodeSelect($event)"
  (nodeExpand)="onNodeExpand($event)">
</app-tree-wrapper>
```

## 🎯 Implementation Rules

### ✅ DO
1. **Always use existing wrapper components** for tables, forms, and trees
2. **Configure components through interfaces** rather than creating new ones
3. **Follow PrimeNG conventions** for all UI interactions
4. **Use the comprehensive event system** provided by wrapper components
5. **Leverage built-in accessibility features** of PrimeNG components
6. **Test with provided configurations** before requesting new features

### ❌ DON'T
1. **Never create duplicate UI components** - use existing wrappers
2. **Never bypass wrapper components** to use PrimeNG directly in features
3. **Never introduce non-PrimeNG UI libraries** without architectural approval
4. **Never ignore existing event systems** - use provided outputs
5. **Never create custom styling** that conflicts with PrimeNG theming

## 🔧 Development Workflow

1. **Analyze Requirements**: Determine if table, form, or tree functionality is needed
2. **Check Existing Wrappers**: Review capabilities of `app-table-wrapper`, `app-dynamic-form`, or `app-tree-wrapper`
3. **Configure Components**: Use provided interfaces to customize behavior
4. **Implement Event Handlers**: Use component outputs for interactions
5. **Test Thoroughly**: Ensure all configurations work as expected
6. **Document Usage**: Update component documentation if new patterns are used

## 📚 References

- **Table Component**: `app-table-wrapper` - Comprehensive PrimeNG Table wrapper
- **Form Component**: `app-dynamic-form` - Full PrimeNG Form capabilities  
- **Tree Component**: `app-tree-wrapper` - Complete PrimeNG Tree implementation
- **PrimeNG Documentation**: [https://primeng.org/](https://primeng.org/)
- **Angular Documentation**: [https://angular.dev/](https://angular.dev/)

---

> **⚠️ Remember**: These components are the foundation of StudyBridge's UI architecture. Following these guidelines ensures consistency, maintainability, and optimal user experience across the entire application.