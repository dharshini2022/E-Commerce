import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-customer-register',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './customer-register.html',
  styleUrl: './customer-register.css',
})
export class CustomerRegister {
  form : FormGroup;
  showPassword = false;
  private emailPattern = /^[^\s@]+@[^\s@]+\.(com|in|org)$/;
  constructor(private fb: FormBuilder, private router : Router, private authService: AuthService){
    this.form = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(4)]],
      email: ['', [Validators.required, Validators.pattern(this.emailPattern)]],
      password: ['', [Validators.required, Validators.minLength(5)]],
      agreeTerms: [false, Validators.requiredTrue],
      becomeVendor: [false]
    });
  }

  get f(){
    return this.form?.controls;
  }

  togglePassword(): void{
    this.showPassword = !this.showPassword;
  }

  onSubmit() : void{
    if(this.form!.invalid){
      this.form.markAllAsTouched();
      alert('Please fill all required fields correctly before continuing.');
      return;
    }

    const payload = {
      fullName: this.form.value.fullName,
      email: this.form.value.email,
      password: this.form.value.password
    };

    this.authService.register(payload).subscribe({
      next: (response: any) => {
        alert('Registration Successful!');
        const becomeVendor = this.form?.value.becomeVendor;

        if (becomeVendor) {
          this.router.navigate(['/login'], { queryParams: { becomeVendor: true } });
        } else {
          this.router.navigate(['/login']);
        }
      },
      error: (error: any) => {
        console.error("Registration failed:", error);
        alert(error.error?.message || error.error || "Registration failed. Please try again.");
      }
    });
  }
}
