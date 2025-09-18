export interface MenuItem {
  id: string;
  name: string;
  displayName: string;
  icon?: string;
  route?: string;
  parentId?: string;
  sortOrder: number;
  isActive: boolean;
  menuType: MenuType;
  requiredPermissions: string[];
  children?: MenuItem[];
}

export enum MenuType {
  Admin = 0,
  Public = 1
}

export interface MenuApiResponse {
  success: boolean;
  data: MenuItem[];
  message?: string;
}

export interface MenuHierarchy {
  parentMenus: MenuItem[];
  childMenus: { [parentId: string]: MenuItem[] };
}

// Navigation item interface for UI components
export interface NavigationItem {
  id: string;
  label: string;
  icon?: string;
  route?: string;
  children?: NavigationItem[];
  isActive?: boolean;
  badge?: {
    text: string;
    severity: 'primary' | 'secondary' | 'success' | 'info' | 'warning' | 'danger';
  };
}