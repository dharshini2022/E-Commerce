import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProductCard } from './product-card';
import { DiscountService } from '../../../services/disocunt.service';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

describe('ProductCard', () => {
  let component: ProductCard;
  let fixture: ComponentFixture<ProductCard>;
  let mockDiscountService: any;

  beforeEach(async () => {
    mockDiscountService = {
      getDiscountsOfProduct: () => of([])
    };

    await TestBed.configureTestingModule({
      imports: [ProductCard],
      providers: [
        { provide: DiscountService, useValue: mockDiscountService },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductCard);
    component = fixture.componentInstance;
    
    // Set mock product input
    component.product = {
      id: 1,
      vendorId: 10,
      categoryId: 5,
      name: 'Test Product',
      storeName: 'Test Store',
      categoryName: 'Test Category',
      averageRating: 4.5,
      reviewCount: 10,
      status: 'Active',
      createdAt: new Date().toISOString(),
      variants: [
        {
          id: 1,
          productId: 1,
          stockQty: 20,
          price: 99.99,
          isDefault: true,
          isActive: true,
          orderCount: 5,
          availableValues: {},
          variantImages: []
        }
      ]
    };

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
