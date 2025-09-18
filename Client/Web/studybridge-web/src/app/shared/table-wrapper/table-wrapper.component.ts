import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Table, TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';

export interface TableColumn {
  field: string;
  header: string;
  type?: 'text' | 'number' | 'date' | 'boolean' | 'enum' | 'status' | 'custom';
  sortable?: boolean;
  filterable?: boolean;
  filterType?: 'text' | 'dropdown' | 'date' | 'number' | 'multiselect';
  filterOptions?: any[];
  width?: string;
  frozen?: boolean;
  hidden?: boolean;
  customTemplate?: string;
}

export interface TableConfig {
  // Data and Loading
  serverSide?: boolean;
  loading?: boolean;
  
  // Pagination
  paginator?: boolean;
  rows?: number;
  rowsPerPageOptions?: number[];
  totalRecords?: number;
  
  // Selection
  selectionMode?: 'single' | 'multiple' | 'checkbox';
  
  // Sorting
  sortMode?: 'single' | 'multiple';
  
  // Filtering
  globalFilterFields?: string[];
  filterMode?: 'lenient' | 'strict';
  
  // Features
  resizableColumns?: boolean;
  reorderableColumns?: boolean;
  scrollable?: boolean;
  scrollHeight?: string;
  frozenColumns?: boolean;
  
  // Export
  exportable?: boolean;
  
  // Styling
  responsive?: boolean;
  striped?: boolean;
  size?: 'small' | 'normal' | 'large';
  
  // Custom
  showGridlines?: boolean;
  showHeader?: boolean;
  caption?: string;
}

export interface TableLazyLoadEvent {
  first?: number;
  rows?: number;
  sortField?: string;
  sortOrder?: number;
  multiSortMeta?: any[];
  filters?: any;
  globalFilter?: any;
}

@Component({
  selector: 'app-table-wrapper',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule,
    TableModule, 
    InputTextModule, 
    ButtonModule,
    TooltipModule
  ],
  templateUrl: './table-wrapper.component.html',
  styleUrl: './table-wrapper.component.scss'
})
export class TableWrapperComponent implements OnInit, OnChanges {
  @Input() data: any[] = [];
  @Input() columns: TableColumn[] = [];
  @Input() config: TableConfig = {};
  @Input() selectedItems: any[] = [];
  @Input() globalFilterValue: string = '';

  // Events
  @Output() selectedItemsChange = new EventEmitter<any[]>();
  @Output() onLazyLoad = new EventEmitter<TableLazyLoadEvent>();
  @Output() onSort = new EventEmitter<any>();
  @Output() onFilter = new EventEmitter<any>();
  @Output() onRowSelect = new EventEmitter<any>();
  @Output() onRowUnselect = new EventEmitter<any>();
  @Output() onGlobalFilter = new EventEmitter<string>();
  @Output() onExport = new EventEmitter<string>();

  // Internal properties
  loading = false;
  totalRecords = 0;
  first = 0;
  rows = 10;
  
  // Filters
  columnFilters: { [key: string]: any } = {};
  
  // Default config
  defaultConfig: TableConfig = {
    serverSide: false,
    loading: false,
    paginator: true,
    rows: 10,
    rowsPerPageOptions: [5, 10, 25, 50],
    totalRecords: 0,
    selectionMode: 'single',
    sortMode: 'single',
    filterMode: 'lenient',
    resizableColumns: true,
    reorderableColumns: true,
    scrollable: false,
    scrollHeight: '400px',
    frozenColumns: false,
    exportable: true,
    responsive: true,
    striped: true,
    size: 'normal',
    showGridlines: true,
    showHeader: true
  };

  ngOnInit(): void {
    this.initializeComponent();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['config'] || changes['data']) {
      this.initializeComponent();
    }
  }

  private initializeComponent(): void {
    // Merge default config with provided config
    this.config = { ...this.defaultConfig, ...this.config };
    
    // Set initial values
    this.loading = this.config.loading || false;
    this.rows = this.config.rows || 10;
    this.totalRecords = this.config.serverSide ? (this.config.totalRecords || 0) : this.data.length;
    
    // Initialize column filters
    this.initializeColumnFilters();
  }

  private initializeColumnFilters(): void {
    this.columns.forEach(column => {
      if (column.filterable) {
        this.columnFilters[column.field] = null;
      }
    });
  }

  // Lazy Loading (for server-side operations)
  onLazyLoadData(event: any): void {
    if (this.config.serverSide) {
      this.loading = true;
      this.first = event.first;
      this.rows = event.rows;
      
      const lazyEvent: TableLazyLoadEvent = {
        first: event.first,
        rows: event.rows,
        sortField: event.sortField,
        sortOrder: event.sortOrder,
        multiSortMeta: event.multiSortMeta,
        filters: event.filters,
        globalFilter: event.globalFilter
      };
      
      this.onLazyLoad.emit(lazyEvent);
    }
  }

  // Selection handling
  onSelectionChange(selection: any): void {
    this.selectedItems = Array.isArray(selection) ? selection : [selection];
    this.selectedItemsChange.emit(this.selectedItems);
  }

  onRowSelectEvent(event: any): void {
    this.onRowSelect.emit(event);
  }

  onRowUnselectEvent(event: any): void {
    this.onRowUnselect.emit(event);
  }

  // Global filter
  onGlobalFilterChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.globalFilterValue = target.value;
    
    if (this.config.serverSide) {
      this.onGlobalFilter.emit(this.globalFilterValue);
    }
  }

  applyGlobalFilter(table: Table): void {
    table.filterGlobal(this.globalFilterValue, 'contains');
  }

  // Column filtering
  onColumnFilter(event: any, field: string): void {
    this.columnFilters[field] = event.target.value;
    
    if (this.config.serverSide) {
      this.onFilter.emit({ field, value: event.target.value });
    }
  }

  // Sorting
  onSortChange(event: any): void {
    if (this.config.serverSide) {
      this.onSort.emit(event);
    }
  }

  // Export functionality
  exportCSV(table: Table): void {
    table.exportCSV();
    this.onExport.emit('csv');
  }

  exportExcel(table: Table): void {
    // Custom Excel export logic can be implemented here
    this.onExport.emit('excel');
  }

  // Utility methods
  clear(table: Table): void {
    table.clear();
    this.globalFilterValue = '';
    this.columnFilters = {};
    this.initializeColumnFilters();
  }

  refresh(): void {
    if (this.config.serverSide) {
      const refreshEvent: TableLazyLoadEvent = {
        first: this.first,
        rows: this.rows,
        filters: this.columnFilters,
        globalFilter: this.globalFilterValue
      };
      this.onLazyLoad.emit(refreshEvent);
    }
  }

  // Column utilities
  getVisibleColumns(): TableColumn[] {
    return this.columns.filter(col => !col.hidden);
  }

  getColumnWidth(column: TableColumn): string {
    return column.width || 'auto';
  }

  // Data formatting helpers
  formatCellValue(rowData: any, column: TableColumn): any {
    const value = this.getFieldValue(rowData, column.field);
    
    switch (column.type) {
      case 'date':
        return value ? new Date(value).toLocaleDateString() : '';
      case 'boolean':
        return value ? 'Yes' : 'No';
      case 'number':
        return typeof value === 'number' ? value.toLocaleString() : value;
      default:
        return value;
    }
  }

  getFieldValue(obj: any, field: string): any {
    return field.split('.').reduce((o, p) => o && o[p], obj);
  }

  // Filter helpers
  getFilterPlaceholder(column: TableColumn): string {
    switch (column.filterType) {
      case 'text':
        return `Search ${column.header}...`;
      case 'number':
        return 'Enter number...';
      case 'date':
        return 'Select date...';
      default:
        return `Filter ${column.header}...`;
    }
  }

  // Selection utilities
  getSelectionCount(): number {
    return this.selectedItems ? this.selectedItems.length : 0;
  }

  isAllSelected(): boolean {
    return this.selectedItems && this.data && this.selectedItems.length === this.data.length;
  }

  selectAll(): void {
    this.selectedItems = [...this.data];
    this.selectedItemsChange.emit(this.selectedItems);
  }

  clearSelection(): void {
    this.selectedItems = [];
    this.selectedItemsChange.emit(this.selectedItems);
  }

  // Template helper
  get Math() {
    return Math;
  }
}