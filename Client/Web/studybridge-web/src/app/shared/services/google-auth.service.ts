import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment.development';

declare global {
  interface Window {
    google: any;
  }
}

@Injectable({
  providedIn: 'root'
})
export class GoogleAuthService {
  private clientId: string = '';
  private isInitialized = false;

  constructor() {}

  setClientId(clientId: string): void {
    this.clientId = clientId;
  }

  async initializeGoogleAuth(): Promise<void> {
    if (this.isInitialized) {
      return;
    }

    if (!this.clientId) {
      throw new Error('Google Client ID not set. Please set it using setClientId()');
    }

    return new Promise((resolve, reject) => {
      // Load Google Identity Services script if not already loaded
      if (!document.getElementById('google-identity-script')) {
        const script = document.createElement('script');
        script.id = 'google-identity-script';
        script.src = 'https://accounts.google.com/gsi/client';
        script.async = true;
        script.defer = true;
        
        script.onload = () => {
          this.initializeGoogleButton(resolve, reject);
        };
        
        script.onerror = () => {
          reject(new Error('Failed to load Google Identity Services'));
        };
        
        document.head.appendChild(script);
      } else {
        this.initializeGoogleButton(resolve, reject);
      }
    });
  }

  private initializeGoogleButton(resolve: Function, reject: Function): void {
    if (window.google && window.google.accounts) {
      try {
        window.google.accounts.id.initialize({
          client_id: this.clientId,
          callback: () => {} // We'll handle the callback separately
        });
        
        this.isInitialized = true;
        resolve();
      } catch (error) {
        reject(error);
      }
    } else {
      reject(new Error('Google Identity Services not available'));
    }
  }

  async signInWithPopup(): Promise<string> {
    if (!this.isInitialized) {
      throw new Error('Google Auth not initialized. Call initializeGoogleAuth() first.');
    }

    return new Promise((resolve, reject) => {
      try {
        window.google.accounts.id.prompt((notification: any) => {
          if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
            // If prompt is not displayed or skipped, try direct sign-in
            this.directSignIn(resolve, reject);
          }
        });

        // Set up callback for credential response
        window.google.accounts.id.initialize({
          client_id: this.clientId,
          callback: (response: any) => {
            if (response.credential) {
              resolve(response.credential);
            } else {
              reject(new Error('No credential received from Google'));
            }
          }
        });
      } catch (error) {
        reject(error);
      }
    });
  }

  private directSignIn(resolve: Function, reject: Function): void {
    // For cases where the prompt doesn't show, we can try the popup flow
    if (window.google && window.google.accounts && window.google.accounts.oauth2) {
      const client = window.google.accounts.oauth2.initTokenClient({
        client_id: this.clientId,
        scope: 'email profile',
        callback: (tokenResponse: any) => {
          if (tokenResponse && tokenResponse.access_token) {
            // Convert access token to ID token by making a request to userinfo
            this.getIdTokenFromAccessToken(tokenResponse.access_token, resolve, reject);
          } else {
            reject(new Error('No access token received'));
          }
        }
      });
      client.requestAccessToken();
    } else {
      reject(new Error('Google OAuth2 not available'));
    }
  }

  private async getIdTokenFromAccessToken(accessToken: string, resolve: Function, reject: Function): Promise<void> {
    try {
      const response = await fetch(`https://www.googleapis.com/oauth2/v2/userinfo?access_token=${accessToken}`);
      
      if (!response.ok) {
        throw new Error('Failed to get user info');
      }

      const userInfo = await response.json();
      
      // For this implementation, we'll create a mock JWT with the user info
      // In a real app, you should get the actual ID token from Google
      const mockIdToken = this.createMockIdToken(userInfo);
      resolve(mockIdToken);
    } catch (error) {
      reject(error);
    }
  }

  private createMockIdToken(userInfo: any): string {
    // This is a simplified approach for development
    // In production, you should use the actual ID token from Google
    const header = btoa(JSON.stringify({ alg: 'RS256', typ: 'JWT' }));
    const payload = btoa(JSON.stringify({
      iss: 'https://accounts.google.com',
      sub: userInfo.id,
      email: userInfo.email,
      name: userInfo.name,
      given_name: userInfo.given_name,
      family_name: userInfo.family_name,
      picture: userInfo.picture,
      iat: Math.floor(Date.now() / 1000),
      exp: Math.floor(Date.now() / 1000) + 3600 // 1 hour
    }));
    const signature = 'mock-signature-for-development';
    
    return `${header}.${payload}.${signature}`;
  }

  parseIdToken(idToken: string): any {
    try {
      const parts = idToken.split('.');
      if (parts.length !== 3) {
        throw new Error('Invalid token format');
      }
      
      const payload = JSON.parse(atob(parts[1]));
      return payload;
    } catch (error) {
      throw new Error('Failed to parse ID token');
    }
  }

  isGoogleAuthAvailable(): boolean {
    return !!(window.google && window.google.accounts);
  }
}