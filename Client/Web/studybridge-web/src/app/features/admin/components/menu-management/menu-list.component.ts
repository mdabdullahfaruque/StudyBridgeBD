import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

// Services and Models
import { MenuApiService } from '../../../../shared/services/menu-api.service';
import { MenuDto, CreateMenuRequest, UpdateMenuRequest, MenuType } from '../../../../shared/models/api.models';

interface MenuWithLevel extends MenuDto {
  level: number;
  hasChildren: boolean;
  isExpanded?: boolean;
}

@Component({
  selector: 'app-menu-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './menu-list.component.html',
  styleUrls: ['./menu-list.component.scss']
})
export class MenuListComponent implements OnInit {
  private menuApiService = inject(MenuApiService);
  private fb = inject(FormBuilder);
  private router = inject(Router);

  // State signals
  menuItems = signal<MenuDto[]>([]);
  loading = signal<boolean>(false);
  searchTerm = signal<string>('');
  
  // Flattened menus with level information for table display
  flattenedMenus = computed(() => {
    return this.flattenMenusWithLevel(this.filteredMenus(), new Set());
  });
  
  // Filtered menus based on search
  filteredMenus = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.buildMenuHierarchy(this.menuItems());
    
    return this.menuItems().filter(menu => 
      menu.displayName.toLowerCase().includes(term) ||
      menu.name.toLowerCase().includes(term) ||
      (menu.route && menu.route.toLowerCase().includes(term))
    );
  });

  // Dialog states
  showCreateDialog = signal<boolean>(false);
  showEditDialog = signal<boolean>(false);
  editingMenu = signal<MenuDto | null>(null);
  
  // Expanded items for tree display
  expandedItems = signal<Set<string>>(new Set());

  // Form
  menuForm: FormGroup;

  // Dropdown options
  menuTypeOptions = [
    { label: 'Admin', value: MenuType.Admin },
    { label: 'Public', value: MenuType.Public }
  ];

  parentMenuOptions = computed(() => {
    const options = [{ label: 'None (Root Menu)', value: '' }];
    const parentMenus = this.menuItems().filter(menu => !menu.parentId);
    
    return [
      ...options,
      ...parentMenus.map(menu => ({
        label: menu.displayName,
        value: menu.id
      }))
    ];
  });

  constructor() {
    this.menuForm = this.fb.group({
      name: ['', Validators.required],
      displayName: ['', Validators.required],
      description: [''],
      icon: [''],
      route: [''],
      menuType: [MenuType.Admin],
      parentMenuId: [''],
      sortOrder: [0, Validators.required],
      isActive: [true],
      hasCrudPermissions: [false]
    });
  }

  async ngOnInit(): Promise<void> {
    await this.loadMenus();
  }

  async loadMenus(): Promise<void> {
    try {
      this.loading.set(true);
      const response = await this.menuApiService.getAllMenus().toPromise();
      
      if (response?.data) {
        this.menuItems.set(response.data);
      }
    } catch (error) {
      console.error('Error loading menus:', error);
      alert('Failed to load menus');
    } finally {
      this.loading.set(false);
    }
  }

  private buildMenuHierarchy(menus: MenuDto[]): MenuDto[] {
    return menus.filter(menu => !menu.parentId).sort((a, b) => a.sortOrder - b.sortOrder);
  }

  private flattenMenusWithLevel(menus: MenuDto[], expandedSet: Set<string>, level: number = 0): MenuWithLevel[] {
    const result: MenuWithLevel[] = [];
    
    const rootMenus = menus.filter(menu => !menu.parentId).sort((a, b) => a.sortOrder - b.sortOrder);
    
    for (const menu of rootMenus) {
      const children = menus.filter(m => m.parentId === menu.id);
      const hasChildren = children.length > 0;
      const isExpanded = expandedSet.has(menu.id);
      
      result.push({
        ...menu,
        level,
        hasChildren,
        isExpanded
      });
      
      if (hasChildren && isExpanded) {
        const childResults = this.flattenChildrenWithLevel(menus, menu.id, expandedSet, level + 1);
        result.push(...childResults);
      }
    }
    
    return result;
  }

  private flattenChildrenWithLevel(menus: MenuDto[], parentId: string, expandedSet: Set<string>, level: number): MenuWithLevel[] {
    const result: MenuWithLevel[] = [];
    const children = menus.filter(m => m.parentId === parentId).sort((a, b) => a.sortOrder - b.sortOrder);
    
    for (const child of children) {
      const grandChildren = menus.filter(m => m.parentId === child.id);
      const hasChildren = grandChildren.length > 0;
      const isExpanded = expandedSet.has(child.id);
      
      result.push({
        ...child,
        level,
        hasChildren,
        isExpanded
      });
      
      if (hasChildren && isExpanded) {
        const childResults = this.flattenChildrenWithLevel(menus, child.id, expandedSet, level + 1);
        result.push(...childResults);
      }
    }
    
    return result;
  }

  toggleExpanded(menuId: string): void {
    const expanded = this.expandedItems();
    const newExpanded = new Set(expanded);
    
    if (newExpanded.has(menuId)) {
      newExpanded.delete(menuId);
    } else {
      newExpanded.add(menuId);
    }
    
    this.expandedItems.set(newExpanded);
  }

  onSearch(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchTerm.set(target.value);
  }

  openCreateDialog(): void {
    this.menuForm.reset({
      name: '',
      displayName: '',
      description: '',
      icon: '',
      route: '',
      menuType: MenuType.Admin,
      parentMenuId: '',
      sortOrder: 0,
      isActive: true,
      hasCrudPermissions: false
    });
    this.editingMenu.set(null);
    this.showCreateDialog.set(true);
  }

  openEditDialog(menu: MenuDto): void {
    this.editingMenu.set(menu);
    this.menuForm.patchValue({
      name: menu.name,
      displayName: menu.displayName,
      description: menu.description || '',
      icon: menu.icon || '',
      route: menu.route || '',
      menuType: menu.menuType,
      parentMenuId: menu.parentId || '',
      sortOrder: menu.sortOrder,
      isActive: menu.isActive,
      hasCrudPermissions: false
    });
    this.showEditDialog.set(true);
  }

  async saveMenu(): Promise<void> {
    if (!this.menuForm.valid) {
      alert('Please fill all required fields');
      return;
    }

    try {
      const formValue = this.menuForm.value;
      const request = {
        ...formValue,
        parentMenuId: formValue.parentMenuId || undefined
      };
      
      if (this.editingMenu()) {
        // Update existing menu
        const response = await this.menuApiService.updateMenu(
          this.editingMenu()!.id,
          request as UpdateMenuRequest
        ).toPromise();
        
        if (response?.success) {
          alert('Menu updated successfully');
        }
      } else {
        // Create new menu
        const response = await this.menuApiService.createMenu(request).toPromise();
        
        if (response?.success) {
          alert('Menu created successfully');
        }
      }
      
      await this.loadMenus();
      this.closeDialogs();
    } catch (error) {
      console.error('Error saving menu:', error);
      alert('Failed to save menu');
    }
  }

  confirmDelete(menu: MenuDto): void {
    if (confirm(`Are you sure you want to delete "${menu.displayName}"?`)) {
      this.deleteMenu(menu);
    }
  }

  async deleteMenu(menu: MenuDto): Promise<void> {
    try {
      const response = await this.menuApiService.deleteMenu(menu.id).toPromise();
      
      if (response?.success) {
        alert('Menu deleted successfully');
        await this.loadMenus();
      }
    } catch (error: any) {
      console.error('Error deleting menu:', error);
      const message = error?.error?.message || 'Failed to delete menu';
      alert(message);
    }
  }

  closeDialogs(): void {
    this.showCreateDialog.set(false);
    this.showEditDialog.set(false);
    this.menuForm.reset();
    this.editingMenu.set(null);
  }

  getStatusClass(isActive: boolean): string {
    return isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800';
  }

  getStatusText(isActive: boolean): string {
    return isActive ? 'Active' : 'Inactive';
  }

  getMenuTypeText(menuType: number): string {
    return menuType === MenuType.Admin ? 'Admin' : 'Public';
  }

  getIndentClass(level: number): string {
    return `pl-${level * 4}`;
  }

  trackByMenuId(index: number, menu: MenuWithLevel): string {
    return menu.id;
  }
}