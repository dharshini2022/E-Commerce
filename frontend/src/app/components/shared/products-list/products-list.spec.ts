import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProductsList } from './products-list';
import { ProductService } from '../../../services/product.service';
import { CategoryService } from '../../../services/category.service';
import { DiscountService } from '../../../services/disocunt.service';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

describe('ProductsList', () => {
  let component: ProductsList;
  let fixture: ComponentFixture<ProductsList>;
  let mockProductService: any;
  let mockCategoryService: any;
  let mockDiscountService: any;

  beforeEach(async () => {
    mockProductService = {
      getCatalog: () => of({ items: [], totalCount: 0 }),
      getVendorProducts: () => of([])
    };

    mockCategoryService = {
      getCategories: () => of([])
    };

    mockDiscountService = {
      getDiscountsOfProduct: () => of([])
    };

    await TestBed.configureTestingModule({
      imports: [ProductsList],
      providers: [
        { provide: ProductService, useValue: mockProductService },
        { provide: CategoryService, useValue: mockCategoryService },
        { provide: DiscountService, useValue: mockDiscountService },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductsList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

