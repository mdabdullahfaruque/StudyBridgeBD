import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="dashboard-container">
      <div class="dashboard-header">
        <h1>Welcome to StudyBridge Dashboard</h1>
        <p>Your IELTS vocabulary learning journey starts here!</p>
      </div>
      
      <div class="dashboard-content">
        <div class="stats-grid">
          <div class="stat-card">
            <h3>Words Learned</h3>
            <p class="stat-number">0</p>
          </div>
          <div class="stat-card">
            <h3>Current Streak</h3>
            <p class="stat-number">0 days</p>
          </div>
          <div class="stat-card">
            <h3>Total Progress</h3>
            <p class="stat-number">0%</p>
          </div>
        </div>
        
        <div class="coming-soon">
          <h2>🚀 Coming Soon</h2>
          <ul>
            <li>2100+ IELTS vocabulary words</li>
            <li>Spaced repetition learning system</li>
            <li>Progress tracking & analytics</li>
            <li>Personalized learning paths</li>
          </ul>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      min-height: 100vh;
      padding: 2rem;
      background: linear-gradient(135deg, #eff6ff 0%, #ffffff 50%, #f9fafb 100%);
    }
    
    .dashboard-header {
      text-align: center;
      margin-bottom: 3rem;
    }
    
    .dashboard-header h1 {
      font-size: 2.5rem;
      font-weight: 700;
      color: #111827;
      margin-bottom: 0.5rem;
    }
    
    .dashboard-header p {
      color: #6b7280;
      font-size: 1.125rem;
    }
    
    .dashboard-content {
      max-width: 1200px;
      margin: 0 auto;
    }
    
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 1.5rem;
      margin-bottom: 3rem;
    }
    
    .stat-card {
      background: white;
      padding: 2rem;
      border-radius: 0.75rem;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
      text-align: center;
    }
    
    .stat-card h3 {
      color: #6b7280;
      font-size: 0.875rem;
      font-weight: 500;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      margin-bottom: 0.5rem;
    }
    
    .stat-number {
      font-size: 2rem;
      font-weight: 700;
      color: #3b82f6;
      margin: 0;
    }
    
    .coming-soon {
      background: white;
      padding: 2rem;
      border-radius: 0.75rem;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
      text-align: center;
    }
    
    .coming-soon h2 {
      color: #111827;
      font-size: 1.875rem;
      margin-bottom: 1rem;
    }
    
    .coming-soon ul {
      list-style: none;
      padding: 0;
      margin: 0;
    }
    
    .coming-soon li {
      padding: 0.5rem 0;
      color: #6b7280;
      font-size: 1.125rem;
    }
    
    @media (max-width: 768px) {
      .dashboard-container {
        padding: 1rem;
      }
      
      .dashboard-header h1 {
        font-size: 1.875rem;
      }
      
      .stats-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class DashboardComponent {}