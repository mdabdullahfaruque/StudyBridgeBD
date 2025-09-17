import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { ChartModule } from 'primeng/chart';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { AdminService } from '../services/admin.service';

interface DashboardStats {
  totalUsers: number;
  activeUsers: number;
  newUsersThisMonth: number;
  totalSubscriptions: number;
  activeSubscriptions: number;
  revenue: number;
  revenueGrowth: number;
}

interface RecentUser {
  id: string;
  displayName: string;
  email: string;
  status: string;
  createdAt: string;
  roles: string[];
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    ChartModule,
    TableModule,
    TagModule
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  // Signals for reactive state
  isLoading = signal(true);
  stats = signal<DashboardStats | null>(null);
  recentUsers = signal<RecentUser[]>([]);
  userGrowthChart = signal<any>({});
  userRolesChart = signal<any>({});

  // Chart options
  chartOptions = computed(() => ({
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'top' as const
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          stepSize: 1
        }
      }
    }
  }));

  doughnutOptions = computed(() => ({
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'bottom' as const
      }
    }
  }));

  constructor(private adminService: AdminService) {}

  async ngOnInit() {
    await this.loadDashboardData();
  }

  private async loadDashboardData() {
    try {
      this.isLoading.set(true);
      
      // Simulate API calls - replace with actual service calls
      await Promise.all([
        this.loadStats(),
        this.loadRecentUsers(),
        this.loadChartData()
      ]);
    } catch (error) {
      console.error('Failed to load dashboard data:', error);
    } finally {
      this.isLoading.set(false);
    }
  }

  private async loadStats() {
    // TODO: Replace with actual API call
    // const response = await this.adminService.getDashboardStats().toPromise();
    
    // Simulated data
    const mockStats: DashboardStats = {
      totalUsers: 2456,
      activeUsers: 1832,
      newUsersThisMonth: 324,
      totalSubscriptions: 1240,
      activeSubscriptions: 1156,
      revenue: 45678,
      revenueGrowth: 15.3
    };
    
    this.stats.set(mockStats);
  }

  private async loadRecentUsers() {
    // TODO: Replace with actual API call
    // const response = await this.adminService.getUsers({ pageSize: 5 }).toPromise();
    
    // Simulated data
    const mockUsers: RecentUser[] = [
      {
        id: '1',
        displayName: 'John Smith',
        email: 'john.smith@example.com',
        status: 'Active',
        createdAt: '2025-01-15T10:30:00Z',
        roles: ['User']
      },
      {
        id: '2',
        displayName: 'Sarah Johnson',
        email: 'sarah.johnson@example.com',
        status: 'Active',
        createdAt: '2025-01-14T15:45:00Z',
        roles: ['User', 'ContentManager']
      },
      {
        id: '3',
        displayName: 'Mike Davis',
        email: 'mike.davis@example.com',
        status: 'Pending',
        createdAt: '2025-01-14T09:20:00Z',
        roles: ['User']
      },
      {
        id: '4',
        displayName: 'Emily Chen',
        email: 'emily.chen@example.com',
        status: 'Active',
        createdAt: '2025-01-13T14:15:00Z',
        roles: ['Admin']
      },
      {
        id: '5',
        displayName: 'Alex Rodriguez',
        email: 'alex.rodriguez@example.com',
        status: 'Inactive',
        createdAt: '2025-01-13T11:30:00Z',
        roles: ['User']
      }
    ];
    
    this.recentUsers.set(mockUsers);
  }

  private async loadChartData() {
    // User Growth Chart
    const userGrowthData = {
      labels: ['Jan 1', 'Jan 5', 'Jan 10', 'Jan 15', 'Jan 20', 'Jan 25', 'Jan 30'],
      datasets: [
        {
          label: 'New Users',
          data: [12, 19, 15, 25, 22, 30, 35],
          borderColor: '#3B82F6',
          backgroundColor: 'rgba(59, 130, 246, 0.1)',
          tension: 0.4
        }
      ]
    };
    this.userGrowthChart.set(userGrowthData);

    // User Roles Chart
    const userRolesData = {
      labels: ['Users', 'Admin', 'ContentManager', 'Finance'],
      datasets: [
        {
          data: [1850, 145, 78, 32],
          backgroundColor: [
            '#3B82F6',
            '#10B981',
            '#F59E0B',
            '#EF4444'
          ],
          borderWidth: 0
        }
      ]
    };
    this.userRolesChart.set(userRolesData);
  }

  async refreshData() {
    await this.loadDashboardData();
  }

  getInitials(name: string): string {
    return name ? name.split(' ').map(n => n[0]).join('').toUpperCase() : 'U';
  }

  getStatusSeverity(status: string): string {
    switch (status.toLowerCase()) {
      case 'active': return 'success';
      case 'pending': return 'warning';
      case 'inactive': return 'danger';
      default: return 'info';
    }
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return new Intl.RelativeTimeFormat('en', { numeric: 'auto' }).format(
      Math.ceil((date.getTime() - Date.now()) / (1000 * 60 * 60 * 24)),
      'day'
    );
  }

  viewUser(userId: string) {
    // Navigate to user details
    console.log('View user:', userId);
  }

  editUser(userId: string) {
    // Navigate to user edit
    console.log('Edit user:', userId);
  }
}