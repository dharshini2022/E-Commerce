import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { VendorService } from '../../services/vendor.service';
import { VendorModel } from '../../models/vendor.model';

@Component({
  selector: 'app-vendor-register',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './vendor-register.html',
  styleUrl: './vendor-register.css',
})
export class VendorRegister {
  form: FormGroup;
  private gstPattern = /^[A-Z0-9]{15}$/;
  private panPattern = /^[A-Z0-9]{10}$/;
  private emailPattern = /^[^\s@]+@[^\s@]+\.(com|in|org|co)$/;

  benefits: string[] = [
    'Reach millions of active shoppers',
    'Launch your digital storefront in minutes',
    'Low platform fees, high seller margins',
    'Verified vendor status trust badge'
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private vendorService: VendorService
  ) {
    this.form = this.fb.group({
      storeName: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
      storeEmail: ['', [Validators.required, Validators.pattern(this.emailPattern), Validators.minLength(10), Validators.maxLength(100)]],
      gstNumber: ['', [Validators.required, Validators.pattern(this.gstPattern)]],
      panNumber: ['', [Validators.required, Validators.pattern(this.panPattern)]],
      description: [''],
      logoUrl: ['']
    });
  }

  get f() {
    return this.form.controls;
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      alert('Please fill all required vendor details correctly.');
      return;
    }

    const data: VendorModel = this.form.value;

    this.vendorService.registerVendor(data).subscribe({
      next: (response: any) => {
        alert('Vendor registered successfully! Waiting for admin approval.');
        this.router.navigate(['/dashboard']);
      },
      error: (error: any) => {
        console.error("Vendor registration failed:", error);
        alert(error.error?.message || error.error || "Vendor registration failed. Make sure you are signed in and have a valid customer account.");
      }
    });
  }
}
