import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ProductService } from '../../../services/product.service';
import { DiscountService } from '../../../services/disocunt.service';
import { ReviewService } from '../../../services/review.service';
import { VendorService } from '../../../services/vendor.service';
import { CategoryService } from '../../../services/category.service';
import { ProductResponse, ProductVariantResponse } from '../../../models/product.model';
import { DiscountResponse } from '../../../models/disocunt.model';
import { CartService } from '../../../services/cart.service';
import { ToastService } from '../../../services/toast.service';
import { ReviewResponse } from '../../../models/review.model';
import { VendorBasicResponse } from '../../../models/vendor.model';
import { Category } from '../../../models/category.model';
import { catchError, of } from 'rxjs';
import { VendorBasicProfile } from '../vendor-basic-profile/vendor-basic-profile';
import { ProductReview } from '../product-review/product-review';
import { WishlistService } from '../../../services/wishlist.service';

@Component({
  selector: 'app-product-detail',
  imports: [CommonModule, RouterModule, VendorBasicProfile, ProductReview],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css',
})
export class ProductDetail implements OnInit {
  product = signal<ProductResponse | undefined>(undefined);
  selectedVariant = signal<ProductVariantResponse | undefined>(undefined);
  discounts = signal<DiscountResponse[]>([]);
  selectedDiscount = signal<DiscountResponse | undefined>(undefined);
  reviews = signal<ReviewResponse[]>([]);
  vendor = signal<VendorBasicResponse | undefined>(undefined);
  loading = signal<boolean>(true);
  activeImageIndex = signal<number>(0);
  userRole = signal<string>('Customer');

  isWishlisted = signal<boolean>(false);
  wishlistItemId = signal<number | null>(null);
  quantity = signal<number>(1);
  cartMessage = signal<string>('');
  wishlistMessage = signal<string>('');

  categoryBreadcrumbs = computed(() => {
    const prod = this.product();
    if (!prod || !prod.categoryName) return [];
    return prod.categoryName.split('-').map(s => s.trim()).filter(Boolean);
  });

  originalPrice = computed(() => {
    return this.selectedVariant()?.price || 0;
  });

  selectedVariantProperties = computed(() => {
    const v = this.selectedVariant();
    if (!v || !v.availableValues) return [];
    return Object.entries(v.availableValues).map(([key, value]) => ({ key, value }));
  });

  discountedPrice = computed(() => {
    const orig = this.originalPrice();
    const disc = this.selectedDiscount();
    if (!disc) return orig;
    if (disc.type === 'Percentage') {
      return parseFloat((orig * (1 - disc.value / 100)).toFixed(2));
    }
    if (disc.type === 'Flat') {
      return Math.max(0, orig - disc.value);
    }
    return orig;
  });

  discountPercent = computed(() => {
    const disc = this.selectedDiscount();
    if (!disc) return 0;
    if (disc.type === 'Percentage') return disc.value;
    const orig = this.originalPrice();
    if (orig <= 0) return 0;
    return parseFloat(((disc.value / orig) * 100).toFixed(0));
  });

  mainImageUrl = computed(() => {
    const v = this.selectedVariant();
    if (!v || !v.variantImages || v.variantImages.length === 0) return '';
    const idx = this.activeImageIndex();
    return v.variantImages[idx]?.imageUrl || v.variantImages[0]?.imageUrl || '';
  });



  averageRatingValue = computed(() => {
    const list = this.reviews();
    if (list.length === 0) return this.product()?.averageRating || 0;
    const sum = list.reduce((acc, curr) => acc + curr.rating, 0);
    return parseFloat((sum / list.length).toFixed(1));
  });

  categories = signal<Category[]>([]);

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    private discountService: DiscountService,
    private reviewService: ReviewService,
    private vendorService: VendorService,
    private categoryService: CategoryService,
    private cartService: CartService,
    private toastService: ToastService,
    private wishlistService: WishlistService
  ) { }

  ngOnInit() {
    this.userRole.set(sessionStorage.getItem('role') || 'Customer');
    this.categoryService.getCategories().subscribe({
      next: (cats) => this.categories.set(cats || []),
      error: (err) => console.error('Error fetching categories:', err)
    });

    this.route.paramMap.subscribe(params => {
      const idStr = params.get('id');
      if (idStr) {
        const id = Number(idStr);
        this.loadProductDetails(id);
      }
    });
  }

  checkWishlistStatus() {
    if (this.userRole() !== 'Customer') {
      this.isWishlisted.set(false);
      this.wishlistItemId.set(null);
      return;
    }
    const variant = this.selectedVariant();
    if (!variant) return;

    this.wishlistService.getWishlist().subscribe({
      next: (wishlist) => {
        const match = wishlist.items.find(item => item.variantId === variant.id);
        if (match) {
          this.isWishlisted.set(true);
          this.wishlistItemId.set(match.id);
        } else {
          this.isWishlisted.set(false);
          this.wishlistItemId.set(null);
        }
      },
      error: (err) => {
        console.error('Error checking wishlist status:', err);
      }
    });
  }

  getCategoryIdForSegment(segmentName: string): number | undefined {
    const list = this.categories();
    const matched = list.find(
      c => c.name.toLowerCase() === segmentName.toLowerCase() || c.slug.toLowerCase() === segmentName.toLowerCase()
    );
    return matched?.id;
  }

  loadProductDetails(id: number) {
    this.loading.set(true);
    this.productService.getById(id).subscribe({
      next: (prod) => {
        this.product.set(prod);
        const def = prod.variants?.find(v => v.isDefault) || prod.variants?.find(v => v.isActive) || prod.variants?.[0];
        this.selectedVariant.set(def);
        this.activeImageIndex.set(0);

        this.discountService.getDiscountsOfProduct(prod.id, prod.categoryId, prod.vendorId)
          .pipe(catchError(() => of([])))
          .subscribe((discounts) => {
            const activeDiscs = discounts.filter(d => d.isActive);
            this.discounts.set(activeDiscs);
            this.selectedDiscount.set(undefined);
          });

        this.reviewService.getProductReviews(prod.id)
          .pipe(catchError(() => of([])))
          .subscribe((revs) => {
            this.reviews.set(revs || []);
          });

        this.vendorService.getVendorBasicProfileById(prod.vendorId)
          .pipe(catchError(() => of(undefined)))
          .subscribe((vProfile) => {
            this.vendor.set(vProfile);
          });

        this.checkWishlistStatus();
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching product:', err);
        this.loading.set(false);
      }
    });
  }

  selectVariant(variantId: number) {
    const v = this.product()?.variants.find(val => val.id === variantId);
    if (v) {
      this.selectedVariant.set(v);
      this.activeImageIndex.set(0); 

      const currentDisc = this.selectedDiscount();
      if (currentDisc && currentDisc.minOrderValue > v.price) {
        this.selectedDiscount.set(undefined);
      }
      this.checkWishlistStatus();
    }
  }

  selectDiscount(discount: DiscountResponse) {
    if (this.selectedDiscount()?.id === discount.id) {
      this.selectedDiscount.set(undefined);
    } else {
      if (this.originalPrice() >= discount.minOrderValue) {
        this.selectedDiscount.set(discount);
      } else {
        alert(`Minimum order value of Rs. ${discount.minOrderValue} required for this coupon.`);
      }
    }
  }

  setImageIndex(index: number) {
    this.activeImageIndex.set(index);
  }

  prevImage() {
    const total = this.selectedVariant()?.variantImages?.length || 0;
    if (total <= 1) return;
    this.activeImageIndex.update(idx => (idx === 0 ? total - 1 : idx - 1));
  }

  nextImage() {
    const total = this.selectedVariant()?.variantImages?.length || 0;
    if (total <= 1) return;
    this.activeImageIndex.update(idx => (idx === total - 1 ? 0 : idx + 1));
  }

  getVariantColor(variant: ProductVariantResponse): string {
    if (!variant || !variant.availableValues) return '';
    const colorKey = Object.keys(variant.availableValues).find(
      key => key.toLowerCase() === 'color' || key.toLowerCase() === 'colour'
    );
    if (!colorKey) return '';
    const colorVal = variant.availableValues[colorKey];

    const colorMap: { [key: string]: string } = {
      'arctic white': '#ffffff',
      'midnight blue': '#1e3a8a',
      'space gray': '#4b5563',
      'space grey': '#4b5563',
      'rose gold': '#e0a899',
      'pink': '#f472b6',
      'blue': '#3b82f6',
      'black': '#000000',
      'white': '#ffffff',
      'red': '#ef4444',
      'green': '#10b981',
      'yellow': '#f59e0b',
      'orange': '#f97316',
      'purple': '#8b5cf6',
      'gold': '#d4af37',
      'silver': '#c0c0c0',
      'grey': '#808080',
      'gray': '#808080',
      'charcoal': '#36454f'
    };

    return colorMap[colorVal.toLowerCase()] || colorVal;
  }

  getVariantLabel(variant: ProductVariantResponse): string {
    if (!variant || !variant.availableValues) return `Variant #${variant.id}`;
    return Object.entries(variant.availableValues)
      .map(([key, val]) => `${key}: ${val}`)
      .join(', ');
  }

  getVariantDisplayValue(variant: ProductVariantResponse): string {
    if (!variant || !variant.availableValues) return '';
    const colorKey = Object.keys(variant.availableValues).find(
      key => key.toLowerCase() === 'color' || key.toLowerCase() === 'colour'
    );
    if (colorKey) return variant.availableValues[colorKey];
    const firstVal = Object.values(variant.availableValues)[0];
    return firstVal || '';
  }

  incrementQty() {
    this.quantity.update(q => q + 1);
  }

  decrementQty() {
    this.quantity.update(q => (q > 1 ? q - 1 : 1));
  }

  addToCart() {
    const variant = this.selectedVariant();
    if (!variant) return;

    this.cartService.addToCart({
      variantId: variant.id,
      quantity: this.quantity()
    }).subscribe({
      next: () => {
        this.cartService.updateCartCount();
        const disc = this.selectedDiscount();
        let msg = '';
        if (disc) {
          msg = `Added ${this.quantity()} unit(s) of variant #${variant.id} to cart with coupon ${disc.code} applied!`;
        } else {
          msg = `Added ${this.quantity()} unit(s) of variant #${variant.id} to cart!`;
        }
        this.toastService.success(msg);
        this.cartMessage.set(msg);
        
        setTimeout(() => {
          this.cartMessage.set('');
        }, 3000);
      },
      error: (err) => {
        console.error('Error adding to cart:', err);
        alert('Failed to add item to cart. Make sure you are logged in as a Customer.');
      }
    });
  }

  toggleWishlist() {
    if (this.userRole() !== 'Customer') {
      this.toastService.warning('Please log in as a Customer to manage your wishlist.');
      return;
    }

    const variant = this.selectedVariant();
    if (!variant) return;

    if (this.isWishlisted()) {
      const itemId = this.wishlistItemId();
      if (itemId !== null) {
        this.wishlistService.removeFromWishlist(itemId).subscribe({
          next: () => {
            this.isWishlisted.set(false);
            this.wishlistItemId.set(null);
            this.wishlistService.updateWishlistCount();
            this.toastService.success('Removed from wishlist');
          },
          error: (err) => {
            console.error('Error removing from wishlist:', err);
            this.toastService.error('Failed to remove from wishlist');
          }
        });
      }
    } else {
      this.wishlistService.addToWishlist({ variantId: variant.id }).subscribe({
        next: (response) => {
          this.isWishlisted.set(true);
          this.wishlistItemId.set(response.data?.id || null);
          this.wishlistService.updateWishlistCount();
          this.toastService.success('Added to wishlist');
        },
        error: (err) => {
          console.error('Error adding to wishlist:', err);
          this.toastService.error('Failed to add to wishlist');
        }
      });
    }
  }



  addVariant() {
    alert('Add Variant action triggered');
  }

  updateVariant() {
    const v = this.selectedVariant();
    if (v) {
      alert(`Update Variant action triggered for Variant #${v.id}`);
    }
  }

  deactivateVariant() {
    const v = this.selectedVariant();
    if (v) {
      const isCurrentlyActive = v.isActive !== false;
      v.isActive = !isCurrentlyActive;
      this.selectedVariant.set({ ...v });
      alert(`Variant #${v.id} has been ${v.isActive ? 'activated' : 'deactivated'}.`);
    }
  }
}
