import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VendorBasicProfile } from './vendor-basic-profile';

describe('VendorBasicProfile', () => {
  let component: VendorBasicProfile;
  let fixture: ComponentFixture<VendorBasicProfile>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VendorBasicProfile],
    }).compileComponents();

    fixture = TestBed.createComponent(VendorBasicProfile);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
