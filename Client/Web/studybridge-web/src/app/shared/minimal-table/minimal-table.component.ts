import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';

export interface MinimalTableColumn {
  field: string;
  header: string;
  width?: string;
}

@Component({
  selector: 'app-minimal-table',
  standalone: true,
  imports: [CommonModule, TableModule],
  template: `
    <div class="simple-card">
      <!-- Title -->
      <h3 class="table-title" *ngIf="title">{{ title }}</h3>
      
      <!-- Ultra Simple Table -->
      <p-table [value]="data" [showGridlines]="true" class="p-datatable-sm">
        <!-- Header -->
        <ng-template pTemplate="header">
          <tr>
            <th *ngFor="let col of columns" [style.width]="col.width">
              {{ col.header }}
            </th>
          </tr>
        </ng-template>

        <!-- Body -->
        <ng-template pTemplate="body" let-rowData>
          <tr>
            <td *ngFor="let col of columns">
              {{ getFieldValue(rowData, col.field) }}
            </td>
          </tr>
        </ng-template>

        <!-- Empty State -->
        <ng-template pTemplate="emptymessage">
          <tr>
            <td [attr.colspan]="columns.length" class="empty-message">
              No data available
            </td>
          </tr>
        </ng-template>
      </p-table>
    </div>
  `,
  styles: [`
    .simple-card {
      background-color: white;
      border: 1px solid #e5e7eb;
      border-radius: 8px;
      padding: 1rem;
    }
    
    .table-title {
      font-size: 1.125rem;
      font-weight: 600;
      color: #374151;
      margin-bottom: 1rem;
      margin-top: 0;
    }
    
    .empty-message {
      text-align: center;
      padding: 2rem;
      color: #6b7280;
    }

    :host ::ng-deep .p-datatable .p-datatable-thead > tr > th {
      background-color: #f9fafb;
      color: #374151;
      font-weight: 600;
      padding: 0.75rem;
    }

    :host ::ng-deep .p-datatable .p-datatable-tbody > tr > td {
      padding: 0.75rem;
      border-bottom: 1px solid #e5e7eb;
    }

    :host ::ng-deep .p-datatable .p-datatable-tbody > tr:hover {
      background-color: #f9fafb;
    }
  `]
})
export class MinimalTableComponent {
  @Input() data: any[] = [];
  @Input() columns: MinimalTableColumn[] = [];
  @Input() title?: string;

  getFieldValue(obj: any, field: string): any {
    return field.split('.').reduce((o, p) => o && o[p], obj) || '';
  }
}