import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-access-denied',
  imports: [CommonModule],
  templateUrl: './access-denied.html',
  styleUrl: './access-denied.css'
})
export class AccessDenied implements OnInit {
  isLoggedIn = false;

  constructor(private router: Router) {}

  ngOnInit(): void {
    const token = sessionStorage.getItem('token');
    this.isLoggedIn = !!token && !this.isTokenExpired(token);
  }

  private isTokenExpired(token: string): boolean {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return true;
      const decoded = JSON.parse(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')));
      if (decoded && decoded.exp) {
        return Math.floor(Date.now() / 1000) >= decoded.exp;
      }
      return false;
    } catch {
      return true;
    }
  }

  handleAction(): void {
    if (this.isLoggedIn) {
      this.router.navigate(['/dashboard']);
    } else {
      this.router.navigate(['/login']);
    }
  }
}
