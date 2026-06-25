import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { VendorRegister } from './vendor-register';

describe('VendorRegister', () => {
  let component: VendorRegister;
  let fixture: ComponentFixture<VendorRegister>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VendorRegister],
      providers: [
        provideHttpClient(),
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(VendorRegister);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
