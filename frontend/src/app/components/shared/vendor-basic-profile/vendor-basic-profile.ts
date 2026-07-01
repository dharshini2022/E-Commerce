import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { VendorBasicResponse } from '../../../models/vendor.model';

@Component({
  selector: 'app-vendor-basic-profile',
  imports: [CommonModule, RouterModule],
  templateUrl: './vendor-basic-profile.html',
  styleUrl: './vendor-basic-profile.css',
})
export class VendorBasicProfile {
  @Input() vendor: VendorBasicResponse | undefined;
}
