/**
 * Menu interfaces matching the backend MenuDto structure
 */

export interface MenuDto {
  id: string;
  name: string;
  displayName: string;
  icon?: string;
  route?: string;
  parentId?: string;
  sortOrder: number;
  isActive: boolean;
  menuType: number;
  requiredPermissions: string[];
  children?: MenuDto[];
}

export interface MenuHierarchy {
  parentMenu: MenuDto;
  childMenus: MenuDto[];
  isExpanded?: boolean;
}

export enum MenuType {
  Admin = 0,
  Public = 1
}

/**
 * Internal menu item interface for component use
 */
export interface MenuItemConfig {
  id: string;
  label: string;
  icon?: string;
  routerLink?: string;
  items?: MenuItemConfig[];
  expanded?: boolean;
  visible?: boolean;
  permissions?: string[];
}

/**
 * Menu loading states
 */
export interface MenuState {
  loading: boolean;
  error: string | null;
  menus: MenuDto[];
  lastUpdated: Date | null;
}