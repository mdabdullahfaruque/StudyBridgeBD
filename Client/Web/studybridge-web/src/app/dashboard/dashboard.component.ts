import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { User } from '../models/user.models';
import { TreeNode } from 'primeng/api';
import { TreeWrapperComponent, TreeConfig } from '../shared/tree-wrapper/tree-wrapper.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, TreeWrapperComponent],
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
    headerTitle: 'IELTS Vocabulary Categories',
    cssClass: 'w-full md:w-[30rem]',
    expandAll: false
  };

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

    // Initialize PrimeNG Tree demo data
    this.initializeTreeData();
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

  logout(): void {
    this.authService.logout();
  }
}