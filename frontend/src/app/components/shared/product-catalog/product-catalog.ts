import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ProductService } from '../../../services/product.service';
import { CategoryService } from '../../../services/category.service';
import { DiscountService } from '../../../services/disocunt.service';
import { ProductResponse, ProductWithDiscount } from '../../../models/product.model';
import { Category } from '../../../models/category.model';
import { DiscountResponse, DiscountType } from '../../../models/disocunt.model';
import { ProductSidebar } from '../product-sidebar/product-sidebar';
import { ProductSort } from '../product-sort/product-sort';
import { ProductsList } from '../products-list/products-list';
import { forkJoin, of, Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

@Component({
  selector: 'app-product-catalog',
  standalone: true,
  imports: [CommonModule, ProductSidebar, ProductSort, ProductsList],
  templateUrl: './product-catalog.html',
  styleUrl: './product-catalog.css'
})
export class ProductCatalog implements OnInit {
  products = signal<ProductWithDiscount[]>([]);
  categories = signal<Category[]>([]);
  loading = signal<boolean>(true);
  loadingMore = signal<boolean>(false);

  selectedCategoryId = signal<number | undefined>(undefined);
  minPrice = signal<number>(0);
  maxPrice = signal<number>(1000000);
  currentSort = signal<string>('newest');
  totalProducts = signal<number>(0);

  draftSort = signal<string>('newest');
  isPriceFilterActive = signal<boolean>(false);
  isSidebarOpen = signal<boolean>(false);

  currentPage = 1;
  pageSize = 12;
  hasNext = true;

  backendSortBy = computed(() => {
    const s = this.currentSort();
    if (s === 'price_asc' || s === 'price_desc') return 'price';
    if (s === 'rating') return 'rating';
    if (s === 'discount') return 'discount';
    return 'newest';
  });

  backendSortOrder = computed(() => {
    const s = this.currentSort();
    if (s === 'price_asc') return 'asc';
    return 'desc';
  });

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    private categoryService: CategoryService,
    private discountService: DiscountService
  ) { }

  ngOnInit() {
    this.categoryService.getCategories().subscribe({
      next: (cats) => this.categories.set(cats || []),
      error: (err) => console.error('Error loading categories:', err)
    });

    let isInitialized = false;
    this.route.queryParams.subscribe(params => {
      const catIdStr = params['categoryId'];
      if (catIdStr) {
        const catId = Number(catIdStr);
        this.selectedCategoryId.set(catId);
        this.isPriceFilterActive.set(true);
      } else {
        this.selectedCategoryId.set(undefined);
      }
      if (isInitialized) {
        this.loadProducts(true);
      }
    });

    setTimeout(() => {
      this.loadProducts(true);
      isInitialized = true;
    }, 500);
  }

  loadProducts(isFirstPage: boolean = false) {
    if (isFirstPage) {
      this.currentPage = 1;
      this.loading.set(true);
      this.hasNext = true;
    } else {
      this.loadingMore.set(true);
    }

    const filter = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      categoryId: this.selectedCategoryId(),
      sortBy: this.backendSortBy(),
      sortOrder: this.backendSortOrder(),
      minPrice: this.isPriceFilterActive() ? this.minPrice() : undefined,
      maxPrice: this.isPriceFilterActive() ? this.maxPrice() : undefined
    };

    this.productService.getCatalog(filter).subscribe({
      next: (res) => {
        this.enrichProductsWithDiscounts(res.items || []).subscribe(enriched => {
          if (isFirstPage) {
            this.products.set(enriched);
          } else {
            this.products.update(items => [...items, ...enriched]);
          }
          this.totalProducts.set(res.totalCount || 0);
          this.hasNext = res.hasNext;
          this.loading.set(false);
          this.loadingMore.set(false);
        });
      },
      error: (err) => {
        console.error(err);
        this.loading.set(false);
        this.loadingMore.set(false);
      }
    });
  }

  onLoadNextPage() {
    this.currentPage++;
    this.loadProducts(false);
  }

  onApplyFilters(event: { categoryId: number | undefined, minPrice: number, maxPrice: number }) {
    this.selectedCategoryId.set(event.categoryId);
    this.minPrice.set(event.minPrice);
    this.maxPrice.set(event.maxPrice);
    this.isPriceFilterActive.set(true);
    this.loadProducts(true);
    this.isSidebarOpen.set(false);
  }

  onSortChanged(event: { sortBy: string, sortOrder: string }) {
    let sortStr = 'newest';
    if (event.sortBy === 'discount') {
      sortStr = 'discount';
    } else if (event.sortBy === 'price') {
      sortStr = `price_${event.sortOrder}`;
    } else {
      sortStr = event.sortBy;
    }
    this.currentSort.set(sortStr);
    this.draftSort.set(sortStr);
    this.loadProducts(true);
  }

  onClearFilters() {
    this.selectedCategoryId.set(undefined);
    this.minPrice.set(0);
    this.maxPrice.set(1000000);
    this.currentSort.set('newest');
    this.draftSort.set('newest');
    this.isPriceFilterActive.set(false);
    this.loadProducts(true);
    this.isSidebarOpen.set(false);
  }

  toggleSidebar() {
    this.isSidebarOpen.update(v => !v);
  }

  enrichProductsWithDiscounts(prods: ProductResponse[]): Observable<ProductWithDiscount[]> {
    if (!prods || prods.length === 0) return of([]);

    const obs = prods.map(prod => {
      const variant = prod.variants?.find(v => v.isDefault) || prod.variants?.find(v => v.isActive);
      if (!variant) {
        return of({ ...prod, discountLabel: '', discountedPrice: 0, discountPercent: 0 } as ProductWithDiscount);
      }

      return this.discountService.getDiscountsOfProduct(prod.id, prod.categoryId, prod.vendorId).pipe(
        map(discounts => {
          let label = '';
          let discountedPrice = variant.price;
          let discountPercent = 0;

          if (discounts && discounts.length > 0) {
            let bestDiscount = discounts[0];
            let maxDiscountAmount = 0;

            discounts.forEach((d: DiscountResponse) => {
              let amount = 0;
              if (d.type === DiscountType.Flat) {
                amount = d.value;
              } else if (d.type === DiscountType.Percentage) {
                amount = (d.value / 100) * variant.price;
              }

              if (amount > maxDiscountAmount) {
                maxDiscountAmount = amount;
                bestDiscount = d;
              }
            });

            if (maxDiscountAmount > 0) {
              if (bestDiscount.type === DiscountType.Flat) {
                label = `Rs. ${bestDiscount.value}`;
                discountedPrice = Math.max(0, variant.price - bestDiscount.value);
                discountPercent = (bestDiscount.value / variant.price) * 100;
              } else if (bestDiscount.type === DiscountType.Percentage) {
                label = `${bestDiscount.value}%`;
                discountedPrice = parseFloat((variant.price * (1 - bestDiscount.value / 100)).toFixed(2));
                discountPercent = bestDiscount.value;
              }
            }
          }

          return {
            ...prod,
            discountLabel: label,
            discountedPrice,
            discountPercent
          } as ProductWithDiscount;
        }),
        catchError(() => {
          return of({
            ...prod,
            discountLabel: '',
            discountedPrice: variant.price,
            discountPercent: 0
          } as ProductWithDiscount);
        })
      );
    });

    return forkJoin(obs);
  }
}
