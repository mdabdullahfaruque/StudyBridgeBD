import { Injectable, signal, computed } from '@angular/core';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { MenuItem, MenuType, NavigationItem } from '../models/menu.models';
import { MenuDto, ApiResponse } from '../models/api.models';
import { AuthService } from './auth.service';
import { MenuApiService } from './menu-api.service';

@Injectable({
  providedIn: 'root'
})
export class MenuService {
  // Reactive state for menus
  private adminMenusSubject = new BehaviorSubject<MenuItem[]>([]);
  private publicMenusSubject = new BehaviorSubject<MenuItem[]>([]);
  private isLoadingSubject = new BehaviorSubject<boolean>(false);

  // Public observables
  public adminMenus$ = this.adminMenusSubject.asObservable();
  public publicMenus$ = this.publicMenusSubject.asObservable();
  public isLoading$ = this.isLoadingSubject.asObservable();

  // Signals for reactive state
  public adminMenusSignal = signal<MenuItem[]>([]);
  public publicMenusSignal = signal<MenuItem[]>([]);
  public isLoadingSignal = signal<boolean>(false);

  // Computed navigation items
  public adminNavigation = computed(() => this.convertToNavigationItems(this.adminMenusSignal()));
  public publicNavigation = computed(() => this.convertToNavigationItems(this.publicMenusSignal()));

  constructor(
    private menuApiService: MenuApiService,
    private authService: AuthService
  ) {}

  /**
   * Load admin menus based on user permissions
   */
  async loadAdminMenus(): Promise<MenuItem[]> {
    try {
      this.setLoading(true);
      
      const response = await firstValueFrom(this.menuApiService.getUserMenus());

      if (response?.data) {
        // API already returns hierarchical structure, so no need to build hierarchy
        const menus = this.convertMenuDtosToMenuItems(response.data);
        this.adminMenusSubject.next(menus);
        this.adminMenusSignal.set(menus);
        return menus;
      }
      
      return [];
    } catch (error) {
      console.error('Error loading admin menus:', error);
      return [];
    } finally {
      this.setLoading(false);
    }
  }

  /**
   * Load public menus based on user permissions
   */
  async loadPublicMenus(): Promise<MenuItem[]> {
    try {
      this.setLoading(true);
      
      const response = await firstValueFrom(this.menuApiService.getPublicMenus());

      if (response?.data) {
        const menus = this.buildMenuHierarchy(this.convertMenuDtosToMenuItems(response.data));
        this.publicMenusSubject.next(menus);
        this.publicMenusSignal.set(menus);
        return menus;
      }
      
      return [];
    } catch (error) {
      console.error('Error loading public menus:', error);
      
      // Fallback: Return default public menus if API fails
      const fallbackMenus = this.getDefaultPublicMenus();
      this.publicMenusSubject.next(fallbackMenus);
      this.publicMenusSignal.set(fallbackMenus);
      return fallbackMenus;
    } finally {
      this.setLoading(false);
    }
  }

  /**
   * Load menus based on user role
   */
  async loadMenusForCurrentUser(): Promise<void> {
    const user = this.authService.getCurrentUser();
    if (!user) return;

    // Load appropriate menus based on user role
    if (this.authService.isAdminUser()) {
      await this.loadAdminMenus();
    } else {
      await this.loadPublicMenus();
    }
  }

  /**
   * Get current admin menus
   */
  getAdminMenus(): MenuItem[] {
    return this.adminMenusSubject.value;
  }

  /**
   * Get current public menus  
   */
  getPublicMenus(): MenuItem[] {
    return this.publicMenusSubject.value;
  }

  /**
   * Clear all cached menus (useful on logout)
   */
  clearMenus(): void {
    this.adminMenusSubject.next([]);
    this.publicMenusSubject.next([]);
    this.adminMenusSignal.set([]);
    this.publicMenusSignal.set([]);
  }

  /**
   * Check if user has permission to access a menu
   */
  hasMenuPermission(menuItem: MenuItem): boolean {
    if (menuItem.requiredPermissions.length === 0) {
      return true; // No specific permissions required
    }
    
    return menuItem.requiredPermissions.some(permission => 
      this.authService.hasPermission(permission)
    );
  }

  /**
   * Build hierarchical menu structure from flat array
   */
  private buildMenuHierarchy(flatMenus: MenuItem[]): MenuItem[] {
    const menuMap = new Map<string, MenuItem>();
    const rootMenus: MenuItem[] = [];

    // Create menu map and initialize children arrays
    flatMenus.forEach(menu => {
      menuMap.set(menu.id, { ...menu, children: [] });
    });

    // Build hierarchy
    flatMenus.forEach(menu => {
      const menuItem = menuMap.get(menu.id)!;
      
      if (menu.parentId) {
        // Add to parent's children
        const parent = menuMap.get(menu.parentId);
        if (parent) {
          parent.children!.push(menuItem);
        }
      } else {
        // Root level menu
        rootMenus.push(menuItem);
      }
    });

    // Sort menus by sortOrder
    const sortMenus = (items: MenuItem[]) => {
      items.sort((a, b) => a.sortOrder - b.sortOrder);
      items.forEach(item => {
        if (item.children && item.children.length > 0) {
          sortMenus(item.children);
        }
      });
    };

    sortMenus(rootMenus);
    return rootMenus;
  }

  /**
   * Convert MenuItem[] to NavigationItem[] for UI components
   */
  private convertToNavigationItems(menus: MenuItem[]): NavigationItem[] {
    return menus.map(menu => this.menuToNavigationItem(menu));
  }

  private menuToNavigationItem(menu: MenuItem): NavigationItem {
    const navItem: NavigationItem = {
      id: menu.id,
      label: menu.displayName,
      icon: menu.icon,
      route: menu.route,
      isActive: menu.isActive
    };

    if (menu.children && menu.children.length > 0) {
      navItem.children = menu.children.map(child => this.menuToNavigationItem(child));
    }

    return navItem;
  }

  /**
   * Convert MenuDto[] from API to MenuItem[]
   */
  private convertMenuDtosToMenuItems(menuDtos: MenuDto[]): MenuItem[] {
    return menuDtos.map(dto => ({
      id: dto.id,
      name: dto.name,
      displayName: dto.displayName,
      icon: dto.icon,
      route: dto.route,
      parentId: dto.parentId,
      sortOrder: dto.sortOrder,
      isActive: dto.isActive,
      menuType: dto.menuType as MenuType,
      requiredPermissions: dto.requiredPermissions || [],
      children: dto.children ? this.convertMenuDtosToMenuItems(dto.children) : []
    }));
  }

  /**
   * Set loading state
   */
  private setLoading(loading: boolean): void {
    this.isLoadingSubject.next(loading);
    this.isLoadingSignal.set(loading);
  }

  /**
   * Find menu item by route
   */
  findMenuByRoute(route: string, menus: MenuItem[] = []): MenuItem | null {
    for (const menu of menus) {
      if (menu.route === route) {
        return menu;
      }
      
      if (menu.children && menu.children.length > 0) {
        const found = this.findMenuByRoute(route, menu.children);
        if (found) return found;
      }
    }
    
    return null;
  }

  /**
   * Get breadcrumb path for a route
   */
  getBreadcrumbPath(route: string, menus: MenuItem[] = []): MenuItem[] {
    const path: MenuItem[] = [];
    
    const findPath = (menuItems: MenuItem[], currentPath: MenuItem[]): boolean => {
      for (const menu of menuItems) {
        const newPath = [...currentPath, menu];
        
        if (menu.route === route) {
          path.push(...newPath);
          return true;
        }
        
        if (menu.children && menu.children.length > 0) {
          if (findPath(menu.children, newPath)) {
            return true;
          }
        }
      }
      
      return false;
    };
    
    findPath(menus, []);
    return path;
  }

  /**
   * Get default public menus as fallback when API fails
   */
  private getDefaultPublicMenus(): MenuItem[] {
    return [
      {
        id: 'public-dashboard',
        name: 'public-dashboard',
        displayName: 'Dashboard',
        icon: 'pi pi-home',
        route: '/public/dashboard',
        parentId: undefined,
        sortOrder: 10,
        isActive: true,
        menuType: MenuType.Public,
        requiredPermissions: ['public.dashboard'],
        children: []
      },
      {
        id: 'public-vocabulary',
        name: 'public-vocabulary',
        displayName: 'Vocabulary',
        icon: 'pi pi-book',
        route: '/public/vocabulary',
        parentId: undefined,
        sortOrder: 20,
        isActive: true,
        menuType: MenuType.Public,
        requiredPermissions: ['public.vocabulary'],
        children: []
      },
      {
        id: 'public-learning',
        name: 'public-learning',
        displayName: 'Learning',
        icon: 'pi pi-lightbulb',
        route: '/public/learning',
        parentId: undefined,
        sortOrder: 30,
        isActive: true,
        menuType: MenuType.Public,
        requiredPermissions: ['public.learning'],
        children: []
      }
    ];
  }
}