import { Component, Input, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { switchMap, map, catchError, of } from 'rxjs';
import { ProductResponse, ProductVariantResponse } from '../../../models/product.model';
import { DiscountService } from '../../../services/disocunt.service';
import { DiscountType, DiscountResponse } from '../../../models/disocunt.model';

@Component({
  selector: 'app-product-card',
  imports: [CommonModule, RouterModule],
  templateUrl: './product-card.html',
  styleUrl: './product-card.css',
})
export class ProductCard implements OnInit {
  private _product = signal<ProductResponse | undefined>(undefined);

  @Input() set product(value: ProductResponse) {
    this._product.set(value);
  }
  get product(): ProductResponse {
    return this._product()!;
  }

  constructor(private discountService: DiscountService) { }

  role = signal<string>('Customer');
  isWishlisted = signal<boolean>(false);

  detailRoute = computed(() => {
    const r = this.role();
    if (r === 'Admin') return '/admin-home/product-detail';
    if (r === 'Vendor') return '/vendor-home/product-detail';
    return '/customer-home/product-detail';
  });

  defaultVariant = computed<ProductVariantResponse | undefined>(() => {
    const prod = this._product();
    if (!prod || !prod.variants || prod.variants.length === 0) return undefined;
    return prod.variants.find(v => v.isDefault) || prod.variants.find(v => v.isActive) || undefined;
  });

  originalPrice = computed<number>(() => {
    return this.defaultVariant()?.price || 0;
  });

  averageRating = computed<number>(() => {
    return this._product()?.averageRating || 0;
  });

  reviewCount = computed<number>(() => {
    return this._product()?.reviewCount || 0;
  });

  private product$ = toObservable(this._product);

  discountInfo = toSignal(
    this.product$.pipe(
      switchMap(prod => {
        const variant = prod?.variants?.find(v => v.isDefault) || prod?.variants?.find(v => v.isActive) || undefined;
        if (!prod || !variant) {
          return of({ label: '', discountedPrice: 0 });
        }

        if (prod && (prod as any).discountLabel !== undefined) {
          return of({
            label: (prod as any).discountLabel || '',
            discountedPrice: (prod as any).discountedPrice ?? variant.price
          });
        }

        return this.discountService.getDiscountsOfProduct(
          prod.id,
          prod.categoryId,
          prod.vendorId
        ).pipe(
          map(discounts => {
            const price = variant.price;
            if (discounts && discounts.length > 0) {
              let bestDiscount = discounts[0];
              let maxDiscountAmount = 0;

              discounts.forEach((d: DiscountResponse) => {
                let amount = 0;
                if (d.type === DiscountType.Flat) {
                  amount = d.value;
                } else if (d.type === DiscountType.Percentage) {
                  amount = (d.value / 100) * price;
                }

                if (amount > maxDiscountAmount) {
                  maxDiscountAmount = amount;
                  bestDiscount = d;
                }
              });

              if (maxDiscountAmount > 0) {
                if (bestDiscount.type === DiscountType.Flat) {
                  return {
                    label: `Rs. ${bestDiscount.value}`,
                    discountedPrice: Math.max(0, price - bestDiscount.value)
                  };
                } else if (bestDiscount.type === DiscountType.Percentage) {
                  return {
                    label: `${bestDiscount.value}%`,
                    discountedPrice: parseFloat((price * (1 - bestDiscount.value / 100)).toFixed(2))
                  };
                }
              }
            }
            return { label: '', discountedPrice: price };
          }),
          catchError(err => {
            console.error('Error fetching discount for product', prod.id, err);
            return of({ label: '', discountedPrice: variant.price });
          })
        );
      })
    ),
    { initialValue: { label: '', discountedPrice: 0 } }
  );

  ngOnInit() {
    this.role.set(sessionStorage.getItem('role') ?? 'Customer');
  }

  getRating(): string {
    return this.averageRating().toFixed(1);
  }

  getReviewCount(): number {
    return this.reviewCount();
  }

  toggleWishlist(event: Event) {
    event.stopPropagation();
    this.isWishlisted.update(val => !val);
    if (this.isWishlisted()) {
      console.log("Added to Whishlist");
    } else {
      console.log("Removed from Whislist");
    }
  }
}
