import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { User } from '../models/user.models';
import { TreeNode } from 'primeng/api';
import { TreeWrapperComponent, TreeConfig } from '../shared/tree-wrapper/tree-wrapper.component';
import { TableWrapperComponent, TableColumn, TableConfig, TableLazyLoadEvent } from '../shared/table-wrapper/table-wrapper.component';
import { BasicTableComponent, BasicTableColumn, BasicTableConfig } from '../shared/basic-table/basic-table.component';
import { IntermediateTableComponent, IntermediateTableColumn, IntermediateTableConfig } from '../shared/intermediate-table/intermediate-table.component';
import { MinimalTableComponent, MinimalTableColumn } from '../shared/minimal-table/minimal-table.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, TreeWrapperComponent, TableWrapperComponent, BasicTableComponent, IntermediateTableComponent, MinimalTableComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  currentUser: User | null = null;
  
  // Tree Configuration
  vocabularyTreeData: TreeNode[] = [];
  selectedVocabularyNodes: TreeNode[] = [];
  treeConfig: TreeConfig = {
    selectionMode: 'checkbox',
    showHeader: true,
    showCounts: true,
    showControls: true,
    showStats: true,
    headerTitle: 'IELTS Vocabulary Categories',
    cssClass: 'w-full md:w-[30rem]',
    expandAll: false,
    minimal: false
  };

  // Table Configuration and Data
  vocabularyData: any[] = [];
  selectedVocabulary: any[] = [];
  tableColumns: TableColumn[] = [
    { 
      field: 'word', 
      header: 'Word', 
      type: 'text', 
      sortable: true, 
      filterable: true,
      filterType: 'text',
      width: '200px'
    },
    { 
      field: 'definition', 
      header: 'Definition', 
      type: 'text', 
      sortable: true, 
      filterable: true,
      filterType: 'text',
      width: '300px'
    },
    { 
      field: 'category', 
      header: 'Category', 
      type: 'enum', 
      sortable: true, 
      filterable: true,
      filterType: 'dropdown',
      filterOptions: [
        { label: 'Academic', value: 'academic' },
        { label: 'Business', value: 'business' },
        { label: 'General', value: 'general' },
        { label: 'Advanced', value: 'advanced' }
      ],
      width: '150px'
    },
    { 
      field: 'difficulty', 
      header: 'Difficulty', 
      type: 'enum', 
      sortable: true, 
      filterable: true,
      filterType: 'dropdown',
      filterOptions: [
        { label: 'Easy', value: 'easy' },
        { label: 'Medium', value: 'medium' },
        { label: 'Hard', value: 'hard' }
      ],
      width: '120px'
    },
    { 
      field: 'frequency', 
      header: 'Frequency', 
      type: 'number', 
      sortable: true, 
      filterable: true,
      filterType: 'number',
      width: '100px'
    },
    { 
      field: 'lastStudied', 
      header: 'Last Studied', 
      type: 'date', 
      sortable: true, 
      filterable: true,
      filterType: 'date',
      width: '140px'
    },
    { 
      field: 'mastered', 
      header: 'Mastered', 
      type: 'boolean', 
      sortable: true, 
      filterable: false,
      width: '100px'
    }
  ];

  tableConfig: TableConfig = {
    serverSide: false,
    paginator: true,
    rows: 10,
    rowsPerPageOptions: [5, 10, 15, 25],
    selectionMode: 'multiple',
    sortMode: 'multiple',
    globalFilterFields: ['word', 'definition', 'category'],
    resizableColumns: true,
    reorderableColumns: true,
    responsive: true,
    striped: true,
    showGridlines: true,
    exportable: true,
    caption: 'IELTS Vocabulary Management'
  };

  // Basic Table Configuration
  basicTableColumns: BasicTableColumn[] = [
    { field: 'word', header: 'Word', sortable: true, width: '200px' },
    { field: 'definition', header: 'Definition', sortable: true, width: '300px' },
    { field: 'category', header: 'Category', sortable: true, width: '150px' },
    { field: 'difficulty', header: 'Difficulty', sortable: true, width: '120px' }
  ];

  basicTableConfig: BasicTableConfig = {
    paginator: true,
    rows: 5,
    rowsPerPageOptions: [5, 10],
    sortable: true,
    caption: 'Basic Table - Sorting & Pagination Only'
  };

  // Intermediate Table Configuration
  intermediateTableColumns: IntermediateTableColumn[] = [
    { field: 'word', header: 'Word', sortable: true, width: '200px', type: 'text' },
    { field: 'definition', header: 'Definition', sortable: true, width: '250px', type: 'text' },
    { field: 'category', header: 'Category', sortable: true, width: '120px', type: 'enum' },
    { field: 'frequency', header: 'Frequency', sortable: true, width: '100px', type: 'number' },
    { field: 'mastered', header: 'Mastered', sortable: true, width: '100px', type: 'boolean' }
  ];

  intermediateTableConfig: IntermediateTableConfig = {
    paginator: true,
    rows: 8,
    rowsPerPageOptions: [5, 8, 15],
    selectionMode: 'multiple',
    globalFilter: true,
    globalFilterFields: ['word', 'definition', 'category'],
    exportable: true,
    caption: 'Intermediate Table - With Global Search & Selection'
  };

  selectedIntermediateItems: any[] = [];

  // Minimal Table Configuration
  minimalTableColumns: MinimalTableColumn[] = [
    { field: 'word', header: 'Word', width: '30%' },
    { field: 'definition', header: 'Definition', width: '50%' },
    { field: 'category', header: 'Category', width: '20%' }
  ];

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Get current user
    this.currentUser = this.authService.getCurrentUser();
    
    // Check if user is authenticated
    if (!this.currentUser) {
      this.router.navigate(['/auth/login']);
    }

    // Initialize demo data
    this.initializeTreeData();
    this.initializeVocabularyData();
  }

  initializeTreeData(): void {
    this.vocabularyTreeData = [
      {
        key: '0',
        label: 'IELTS Vocabulary Categories',
        data: 'Root Category',
        icon: 'pi pi-fw pi-folder',
        children: [
          {
            key: '0-0',
            label: 'Academic Topics',
            data: 'Academic Category',
            icon: 'pi pi-fw pi-folder',
            children: [
              { 
                key: '0-0-0', 
                label: 'Science & Technology', 
                icon: 'pi pi-fw pi-file-o', 
                data: '245 words',
                leaf: true 
              },
              { 
                key: '0-0-1', 
                label: 'Environment & Nature', 
                icon: 'pi pi-fw pi-file-o', 
                data: '189 words',
                leaf: true 
              },
              { 
                key: '0-0-2', 
                label: 'Education & Learning', 
                icon: 'pi pi-fw pi-file-o', 
                data: '156 words',
                leaf: true 
              },
              { 
                key: '0-0-3', 
                label: 'Research & Analysis', 
                icon: 'pi pi-fw pi-file-o', 
                data: '134 words',
                leaf: true 
              }
            ]
          },
          {
            key: '0-1',
            label: 'General Training',
            data: 'General Category',
            icon: 'pi pi-fw pi-folder',
            children: [
              { 
                key: '0-1-0', 
                label: 'Travel & Tourism', 
                icon: 'pi pi-fw pi-file-o', 
                data: '198 words',
                leaf: true 
              },
              { 
                key: '0-1-1', 
                label: 'Health & Lifestyle', 
                icon: 'pi pi-fw pi-file-o', 
                data: '167 words',
                leaf: true 
              },
              { 
                key: '0-1-2', 
                label: 'Arts & Culture', 
                icon: 'pi pi-fw pi-file-o', 
                data: '143 words',
                leaf: true 
              },
              { 
                key: '0-1-3', 
                label: 'Sports & Recreation', 
                icon: 'pi pi-fw pi-file-o', 
                data: '112 words',
                leaf: true 
              }
            ]
          },
          {
            key: '0-2',
            label: 'Business & Economy',
            data: 'Business Category',
            icon: 'pi pi-fw pi-folder',
            children: [
              { 
                key: '0-2-0', 
                label: 'Finance & Banking', 
                icon: 'pi pi-fw pi-file-o', 
                data: '176 words',
                leaf: true 
              },
              { 
                key: '0-2-1', 
                label: 'Management & Leadership', 
                icon: 'pi pi-fw pi-file-o', 
                data: '154 words',
                leaf: true 
              },
              { 
                key: '0-2-2', 
                label: 'Marketing & Sales', 
                icon: 'pi pi-fw pi-file-o', 
                data: '132 words',
                leaf: true 
              },
              { 
                key: '0-2-3', 
                label: 'Economics & Trade', 
                icon: 'pi pi-fw pi-file-o', 
                data: '148 words',
                leaf: true 
              }
            ]
          },
          {
            key: '0-3',
            label: 'Advanced Vocabulary',
            data: 'Advanced Category',
            icon: 'pi pi-fw pi-folder',
            children: [
              { 
                key: '0-3-0', 
                label: 'Formal Academic Writing', 
                icon: 'pi pi-fw pi-file-o', 
                data: '223 words',
                leaf: true 
              },
              { 
                key: '0-3-1', 
                label: 'Complex Sentence Structures', 
                icon: 'pi pi-fw pi-file-o', 
                data: '187 words',
                leaf: true 
              },
              { 
                key: '0-3-2', 
                label: 'Sophisticated Expressions', 
                icon: 'pi pi-fw pi-file-o', 
                data: '165 words',
                leaf: true 
              }
            ]
          }
        ]
      }
    ];
  }

  // Tree Event Handlers
  onSelectionChange(selectedNodes: TreeNode[]): void {
    this.selectedVocabularyNodes = selectedNodes;
    console.log('Selected vocabulary nodes:', selectedNodes.length);
  }

  onNodeSelect(node: TreeNode): void {
    console.log('Selected node:', node.label);
  }

  onNodeUnselect(node: TreeNode): void {
    console.log('Unselected node:', node.label);
  }

  onNodeExpand(node: TreeNode): void {
    console.log('Expanded node:', node.label);
  }

  onNodeCollapse(node: TreeNode): void {
    console.log('Collapsed node:', node.label);
  }

  get userFirstName(): string {
    if (!this.currentUser?.displayName) {
      return '';
    }
    const names = this.currentUser.displayName.split(' ');
    return names.length > 0 ? names[0] : '';
  }

  initializeVocabularyData(): void {
    this.vocabularyData = [
      {
        id: 1,
        word: 'accomplish',
        definition: 'To complete successfully; to achieve',
        category: 'academic',
        difficulty: 'medium',
        frequency: 85,
        lastStudied: new Date('2024-09-10'),
        mastered: true
      },
      {
        id: 2,
        word: 'paradigm',
        definition: 'A typical example or pattern of something; a model',
        category: 'advanced',
        difficulty: 'hard',
        frequency: 45,
        lastStudied: new Date('2024-09-12'),
        mastered: false
      },
      {
        id: 3,
        word: 'comprehensive',
        definition: 'Complete and including everything necessary',
        category: 'academic',
        difficulty: 'medium',
        frequency: 72,
        lastStudied: new Date('2024-09-08'),
        mastered: true
      },
      {
        id: 4,
        word: 'entrepreneur',
        definition: 'A person who starts and manages a business',
        category: 'business',
        difficulty: 'medium',
        frequency: 68,
        lastStudied: new Date('2024-09-11'),
        mastered: false
      },
      {
        id: 5,
        word: 'ubiquitous',
        definition: 'Present, appearing, or found everywhere',
        category: 'advanced',
        difficulty: 'hard',
        frequency: 32,
        lastStudied: new Date('2024-09-09'),
        mastered: false
      },
      {
        id: 6,
        word: 'facilitate',
        definition: 'To make an action or process easier or more achievable',
        category: 'academic',
        difficulty: 'medium',
        frequency: 79,
        lastStudied: new Date('2024-09-13'),
        mastered: true
      },
      {
        id: 7,
        word: 'diverse',
        definition: 'Showing a great deal of variety; very different',
        category: 'general',
        difficulty: 'easy',
        frequency: 91,
        lastStudied: new Date('2024-09-14'),
        mastered: true
      },
      {
        id: 8,
        word: 'innovation',
        definition: 'The action or process of innovating; new method or idea',
        category: 'business',
        difficulty: 'medium',
        frequency: 76,
        lastStudied: new Date('2024-09-07'),
        mastered: true
      },
      {
        id: 9,
        word: 'criterion',
        definition: 'A principle or standard by which something is judged',
        category: 'academic',
        difficulty: 'medium',
        frequency: 58,
        lastStudied: new Date('2024-09-06'),
        mastered: false
      },
      {
        id: 10,
        word: 'substantial',
        definition: 'Of considerable importance, size, or worth',
        category: 'general',
        difficulty: 'medium',
        frequency: 83,
        lastStudied: new Date('2024-09-15'),
        mastered: true
      },
      {
        id: 11,
        word: 'unprecedented',
        definition: 'Never done or known before; without previous example',
        category: 'advanced',
        difficulty: 'hard',
        frequency: 41,
        lastStudied: new Date('2024-09-05'),
        mastered: false
      },
      {
        id: 12,
        word: 'collaborate',
        definition: 'Work jointly on an activity or project',
        category: 'business',
        difficulty: 'easy',
        frequency: 88,
        lastStudied: new Date('2024-09-16'),
        mastered: true
      }
    ];
  }

  // Table Event Handlers
  onTableLazyLoad(event: TableLazyLoadEvent): void {
    console.log('Lazy load event:', event);
    // Handle server-side loading here
  }

  onTableSort(event: any): void {
    console.log('Sort event:', event);
    // Handle server-side sorting here
  }

  onTableFilter(event: any): void {
    console.log('Filter event:', event);
    // Handle server-side filtering here
  }

  onTableGlobalFilter(filterValue: string): void {
    console.log('Global filter:', filterValue);
    // Handle server-side global filtering here
  }

  onRowSelect(event: any): void {
    console.log('Row selected:', event.data);
  }

  onRowUnselect(event: any): void {
    console.log('Row unselected:', event.data);
  }

  onTableExport(format: string): void {
    console.log('Export format:', format);
    // Handle export functionality
  }

  // Intermediate Table Event Handlers
  onIntermediateRowSelect(event: any): void {
    console.log('Intermediate table row selected:', event.data);
  }

  onIntermediateRowUnselect(event: any): void {
    console.log('Intermediate table row unselected:', event.data);
  }

  logout(): void {
    this.authService.logout();
  }
}