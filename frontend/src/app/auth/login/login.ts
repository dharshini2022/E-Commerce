import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { getCurrentUserName } from '../../rxjs/auth.operator';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  form: FormGroup;
  showPassword = false;

  private emailPattern = /^[^\s@]+@[^\s@]+\.(com|in|org)$/;

  benefits: string[] = [
    '10M+ happy shoppers',
    'Discover 500K+ products',
    'Fast & reliable delivery'
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService,
    private route: ActivatedRoute
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.pattern(this.emailPattern)]],
      password: ['', [Validators.required]]
    });
  }

  get f() {
    return this.form.controls;
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      alert('Please enter a valid email and password to continue.');
      return;
    }

    this.authService.login(this.form.value).subscribe({
      next: (response: any) => {
        const token = response.accessToken || response.AccessToken;
        sessionStorage.setItem('token', token);
        
        let role = '';
        const payload = this.authService.currentUserValue;
        if (payload) {
          role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload['role'] || '';
          if (role) {
            sessionStorage.setItem('role', role);
          }
        }

        const name = getCurrentUserName();
        if (name) {
          sessionStorage.setItem('name', name);
        }

        console.log(token);
        alert('Signed in successfully!');
        
        const becomeVendor = this.route.snapshot.queryParamMap.get('becomeVendor') === 'true';
        const upperRole = role ? role.toUpperCase() : '';
        if (becomeVendor) {
          this.router.navigate(['/vendor-register']);
        } else if (upperRole === 'ADMIN') {
          this.router.navigate(['/admin-home']);
        } else if (upperRole === 'VENDOR') {
          this.router.navigate(['/vendor-home']);
        } else {
          this.router.navigate(['/customer-home']);
        }
      },
      error: (error: any) => {
        console.error("Login failed:", error);
        alert(error.error?.message || error.error || "Login failed. Please check your credentials.");
      }
    });
  }
}
