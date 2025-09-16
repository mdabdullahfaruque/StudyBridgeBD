import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';

export interface BasicTableColumn {
  field: string;
  header: string;
  sortable?: boolean;
  width?: string;
  type?: 'text' | 'number' | 'date' | 'boolean';
}

export interface BasicTableConfig {
  paginator?: boolean;
  rows?: number;
  rowsPerPageOptions?: number[];
  sortable?: boolean;
  caption?: string;
}

@Component({
  selector: 'app-basic-table',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule],
  template: `
    <div class="card">
      <!-- Header -->
      <div class="flex justify-between items-center mb-4" *ngIf="config.caption">
        <h3 class="text-lg font-semibold text-gray-800">{{ config.caption }}</h3>
      </div>

      <!-- Simple Data Table -->
      <p-table 
        [value]="data" 
        [paginator]="config.paginator || false"
        [rows]="config.rows || 10"
        [rowsPerPageOptions]="config.rowsPerPageOptions || [5, 10, 25]"
        [showGridlines]="true"
        class="p-datatable-sm"
      >
        <!-- Header -->
        <ng-template pTemplate="header">
          <tr>
            <th *ngFor="let col of columns" 
                [pSortableColumn]="config.sortable !== false && col.sortable !== false ? col.field : undefined"
                [style.width]="col.width">
              {{ col.header }}
              <p-sortIcon *ngIf="config.sortable !== false && col.sortable !== false" [field]="col.field"></p-sortIcon>
            </th>
          </tr>
        </ng-template>

        <!-- Body -->
        <ng-template pTemplate="body" let-rowData>
          <tr>
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
                <p>No data available</p>
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
export class BasicTableComponent {
  @Input() data: any[] = [];
  @Input() columns: BasicTableColumn[] = [];
  @Input() config: BasicTableConfig = {};

  getFieldValue(obj: any, field: string): any {
    return field.split('.').reduce((o, p) => o && o[p], obj);
  }

  formatDate(value: any): string {
    return value ? new Date(value).toLocaleDateString() : '';
  }
}