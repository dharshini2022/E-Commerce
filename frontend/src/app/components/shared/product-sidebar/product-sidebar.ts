import { Component, input, output, signal, effect, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Category } from '../../../models/category.model';

@Component({
  selector: 'app-product-sidebar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-sidebar.html',
  styleUrl: './product-sidebar.css'
})
export class ProductSidebar {
  categories = input<Category[]>([]);
  selectedCategoryId = input<number | undefined>(undefined);
  minPrice = input<number>(0);
  maxPrice = input<number>(1000000);

  applyFilters = output<{ categoryId: number | undefined, minPrice: number, maxPrice: number }>();
  clearFilters = output<void>();
  closeSidebar = output<void>();

  localSelectedCategoryId = signal<number | undefined>(undefined);
  localMinPrice = signal<number>(0);
  localMaxPrice = signal<number>(1000000);

  minPriceOptions = [0, 1000, 2000, 5000, 10000, 20000, 50000, 100000, 250000, 500000];
  maxPriceOptions = [1000, 2000, 5000, 10000, 20000, 50000, 100000, 250000, 500000, 1000000];

  getMinPriceOptions = computed(() => {
    const val = this.localMinPrice();
    const list = [...this.minPriceOptions];
    if (!list.includes(val)) {
      list.push(val);
      list.sort((a, b) => a - b);
    }
    return list;
  });

  getMaxPriceOptions = computed(() => {
    const val = this.localMaxPrice();
    const list = [...this.maxPriceOptions];
    if (!list.includes(val)) {
      list.push(val);
      list.sort((a, b) => a - b);
    }
    return list;
  });

  minPercent = computed(() => {
    const min = 0;
    const max = 1000000;
    const current = this.localMinPrice();
    return ((current - min) / (max - min)) * 100;
  });

  maxPercent = computed(() => {
    const min = 0;
    const max = 1000000;
    const current = this.localMaxPrice();
    return 100 - (((current - min) / (max - min)) * 100);
  });

  constructor() {
    effect(() => {
      this.localSelectedCategoryId.set(this.selectedCategoryId());
    });
    effect(() => {
      this.localMinPrice.set(this.minPrice());
    });
    effect(() => {
      this.localMaxPrice.set(this.maxPrice());
    });
  }

  onCategoryClick(catId: number) {
    this.localSelectedCategoryId.set(catId);
  }

  onMinPriceInput(event: Event) {
    const target = event.target as HTMLInputElement;
    const val = Number(target.value);
    if (val > this.localMaxPrice()) {
      this.localMinPrice.set(this.localMaxPrice());
    } else {
      this.localMinPrice.set(val);
    }
  }

  onMaxPriceInput(event: Event) {
    const target = event.target as HTMLInputElement;
    const val = Number(target.value);
    if (val < this.localMinPrice()) {
      this.localMaxPrice.set(this.localMinPrice());
    } else {
      this.localMaxPrice.set(val);
    }
  }

  onMinSelectChange(event: Event) {
    const target = event.target as HTMLSelectElement;
    const val = Number(target.value);
    if (val > this.localMaxPrice()) {
      this.localMinPrice.set(this.localMaxPrice());
    } else {
      this.localMinPrice.set(val);
    }
  }

  onMaxSelectChange(event: Event) {
    const target = event.target as HTMLSelectElement;
    const val = Number(target.value);
    if (val < this.localMinPrice()) {
      this.localMaxPrice.set(this.localMinPrice());
    } else {
      this.localMaxPrice.set(val);
    }
  }

  onClearPrice() {
    this.localMinPrice.set(0);
    this.localMaxPrice.set(1000000);
  }

  onApplyClick() {
    this.applyFilters.emit({
      categoryId: this.localSelectedCategoryId(),
      minPrice: this.localMinPrice(),
      maxPrice: this.localMaxPrice()
    });
  }
}
