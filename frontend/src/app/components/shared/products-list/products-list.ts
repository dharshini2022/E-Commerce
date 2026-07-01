import { Component, input, output, ViewChild, ElementRef, signal, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductCard } from '../product-card/product-card';
import { ProductWithDiscount } from '../../../models/product.model';

@Component({
  selector: 'app-products-list',
  standalone: true,
  imports: [CommonModule, ProductCard],
  templateUrl: './products-list.html',
  styleUrl: './products-list.css'
})
export class ProductsList implements AfterViewInit, OnDestroy {
  products = input<ProductWithDiscount[]>([]);
  loading = input<boolean>(true);
  loadingMore = input<boolean>(false);
  hasNext = input<boolean>(true);

  loadNextPage = output<void>();

  @ViewChild('sentinel') sentinel!: ElementRef;
  @ViewChild('scrollContainer') scrollContainer!: ElementRef<HTMLDivElement>;

  showScrollToTop = signal<boolean>(false);
  private observer: IntersectionObserver | undefined;

  ngAfterViewInit() {
    this.setupIntersectionObserver();
  }

  ngOnDestroy() {
    if (this.observer) {
      this.observer.disconnect();
    }
  }

  setupIntersectionObserver() {
    if (typeof IntersectionObserver === 'undefined') return;

    this.observer = new IntersectionObserver(
      (entries) => {
        const entry = entries[0];
        if (entry.isIntersecting && !this.loading() && !this.loadingMore() && this.hasNext()) {
          this.loadNextPage.emit();
        }
      },
      {
        root: this.scrollContainer?.nativeElement || null,
        threshold: 0.1
      }
    );
    if (this.sentinel) {
      this.observer.observe(this.sentinel.nativeElement);
    }
  }

  onScroll(event: Event) {
    const target = event.target as HTMLDivElement;
    this.showScrollToTop.set(target.scrollTop > 300);
  }

  scrollToTop() {
    if (this.scrollContainer) {
      this.scrollContainer.nativeElement.scrollTo({
        top: 0,
        behavior: 'smooth'
      });
    }
  }
}
