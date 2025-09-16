import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';

export interface IntermediateTableColumn {
  field: string;
  header: string;
  sortable?: boolean;
  width?: string;
  type?: 'text' | 'number' | 'date' | 'boolean' | 'enum';
}

export interface IntermediateTableConfig {
  paginator?: boolean;
  rows?: number;
  rowsPerPageOptions?: number[];
  selectionMode?: 'single' | 'multiple';
  globalFilter?: boolean;
  globalFilterFields?: string[];
  caption?: string;
  exportable?: boolean;
}

@Component({
  selector: 'app-intermediate-table',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, InputTextModule, ButtonModule],
  template: `
    <div class="card">
      <!-- Header with Search -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-4">
        <h3 class="text-lg font-semibold text-gray-800" *ngIf="config.caption">
          {{ config.caption }}
        </h3>
        
        <div class="flex gap-2" *ngIf="config.globalFilter">
          <!-- Global Search -->
          <div class="relative">
            <i class="pi pi-search absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 text-sm"></i>
            <input 
              type="text" 
              pInputText 
              placeholder="Search..." 
              [(ngModel)]="globalFilterValue"
              (input)="applyGlobalFilter(table, $event)"
              class="pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          
          <!-- Export Button -->
          <button 
            pButton 
            type="button" 
            icon="pi pi-download" 
            class="p-button-outlined p-button-sm"
            (click)="exportCSV(table)"
            *ngIf="config.exportable"
            pTooltip="Export CSV">
          </button>
        </div>
      </div>

      <!-- Selection Info -->
      <div class="mb-4 text-sm text-gray-600" *ngIf="config.selectionMode && selectedItems.length > 0">
        {{ selectedItems.length }} item(s) selected
        <button 
          pButton 
          type="button" 
          label="Clear" 
          class="p-button-text p-button-sm ml-2"
          (click)="clearSelection()">
        </button>
      </div>

      <!-- Data Table -->
      <p-table 
        #table
        [value]="data" 
        [paginator]="config.paginator !== false"
        [rows]="config.rows || 10"
        [rowsPerPageOptions]="config.rowsPerPageOptions || [5, 10, 25]"
        [globalFilterFields]="config.globalFilterFields || []"
        [(selection)]="selectedItems"
        [selectionMode]="config.selectionMode"
        (onRowSelect)="onRowSelectEvent($event)"
        (onRowUnselect)="onRowUnselectEvent($event)"
        [showGridlines]="true"
        class="p-datatable-sm"
      >
        <!-- Header -->
        <ng-template pTemplate="header">
          <tr>
            <th *ngFor="let col of columns" 
                [pSortableColumn]="col.sortable !== false ? col.field : undefined"
                [style.width]="col.width">
              {{ col.header }}
              <p-sortIcon *ngIf="col.sortable !== false" [field]="col.field"></p-sortIcon>
            </th>
          </tr>
        </ng-template>

        <!-- Body -->
        <ng-template pTemplate="body" let-rowData>
          <tr [pSelectableRow]="rowData">
            <td *ngFor="let col of columns">
              <span [ngSwitch]="col.type">
                <i *ngSwitchCase="'boolean'" 
                   class="pi text-sm"
                   [ngClass]="{
                     'pi-check text-green-500': getFieldValue(rowData, col.field),
                     'pi-times text-red-500': !getFieldValue(rowData, col.field)
                   }">
                </i>
                <span *ngSwitchCase="'date'">
                  {{ formatDate(getFieldValue(rowData, col.field)) }}
                </span>
                <span *ngSwitchCase="'enum'" 
                      class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                  {{ getFieldValue(rowData, col.field) }}
                </span>
                <span *ngSwitchDefault>
                  {{ getFieldValue(rowData, col.field) }}
                </span>
              </span>
            </td>
          </tr>
        </ng-template>

        <!-- Empty State -->
        <ng-template pTemplate="emptymessage">
          <tr>
            <td [attr.colspan]="columns.length" class="text-center py-8">
              <div class="flex flex-col items-center text-gray-500">
                <i class="pi pi-inbox text-3xl mb-2"></i>
                <p class="font-medium">No data found</p>
                <p class="text-sm">Try adjusting your search</p>
              </div>
            </td>
          </tr>
        </ng-template>
      </p-table>
    </div>
  `,
  styles: [`
    .card {
      background-color: white;
      border-radius: 0.5rem;
      box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1);
      border: 1px solid #e5e7eb;
      padding: 1.5rem;
    }
  `]
})
export class IntermediateTableComponent {
  @Input() data: any[] = [];
  @Input() columns: IntermediateTableColumn[] = [];
  @Input() config: IntermediateTableConfig = {};
  @Input() selectedItems: any[] = [];

  @Output() selectedItemsChange = new EventEmitter<any[]>();
  @Output() onRowSelect = new EventEmitter<any>();
  @Output() onRowUnselect = new EventEmitter<any>();

  globalFilterValue = '';

  getFieldValue(obj: any, field: string): any {
    return field.split('.').reduce((o, p) => o && o[p], obj);
  }

  formatDate(value: any): string {
    return value ? new Date(value).toLocaleDateString() : '';
  }

  applyGlobalFilter(table: any, event: Event): void {
    const target = event.target as HTMLInputElement;
    table.filterGlobal(target.value, 'contains');
  }

  exportCSV(table: any): void {
    table.exportCSV();
  }

  clearSelection(): void {
    this.selectedItems = [];
    this.selectedItemsChange.emit(this.selectedItems);
  }

  onRowSelectEvent(event: any): void {
    this.selectedItemsChange.emit(this.selectedItems);
    this.onRowSelect.emit(event);
  }

  onRowUnselectEvent(event: any): void {
    this.selectedItemsChange.emit(this.selectedItems);
    this.onRowUnselect.emit(event);
  }
}