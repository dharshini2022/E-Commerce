import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProductDetail } from './product-detail';
import { ProductService } from '../../../services/product.service';
import { CategoryService } from '../../../services/category.service';
import { DiscountService } from '../../../services/disocunt.service';
import { ReviewService } from '../../../services/review.service';
import { VendorService } from '../../../services/vendor.service';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { of } from 'rxjs';

describe('ProductDetail', () => {
  let component: ProductDetail;
  let fixture: ComponentFixture<ProductDetail>;
  let mockProductService: any;
  let mockCategoryService: any;
  let mockDiscountService: any;
  let mockReviewService: any;
  let mockVendorService: any;

  beforeEach(async () => {
    mockProductService = {
      getById: () => of({
        id: 1,
        vendorId: 1,
        categoryId: 1,
        name: 'Test Product',
        categoryName: 'Electronics - Audio',
        variants: [
          {
            id: 101,
            productId: 1,
            price: 1000,
            isDefault: true,
            isActive: true,
            stockQty: 5,
            variantImages: [{ id: 1, imageUrl: 'test.jpg', imageOrder: 1 }]
          }
        ]
      })
    };

    mockCategoryService = {
      getCategories: () => of([])
    };

    mockDiscountService = {
      getDiscountsOfProduct: () => of([])
    };

    mockReviewService = {
      getProductReviews: () => of([])
    };

    mockVendorService = {
      getVendorBasicProfileById: () => of({
        storeName: 'Test Vendor',
        storeEmail: 'vendor@test.com',
        description: 'Test Description',
        logoUrl: ''
      })
    };

    await TestBed.configureTestingModule({
      imports: [ProductDetail],
      providers: [
        { provide: ProductService, useValue: mockProductService },
        { provide: CategoryService, useValue: mockCategoryService },
        { provide: DiscountService, useValue: mockDiscountService },
        { provide: ReviewService, useValue: mockReviewService },
        { provide: VendorService, useValue: mockVendorService },
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            paramMap: of({
              get: (key: string) => (key === 'id' ? '1' : null)
            })
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductDetail);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
