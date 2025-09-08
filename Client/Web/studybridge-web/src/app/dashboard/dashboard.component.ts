import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { User } from '../models/user.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  currentUser: User | null = null;

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