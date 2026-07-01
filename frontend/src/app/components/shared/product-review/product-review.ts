import { Component, Input, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReviewResponse } from '../../../models/review.model';

@Component({
  selector: 'app-product-review',
  imports: [CommonModule],
  templateUrl: './product-review.html',
  styleUrl: './product-review.css',
})
export class ProductReview {
  private _reviews = signal<ReviewResponse[]>([]);
  private _productAverageRating = signal<number>(0);

  @Input() set reviews(value: ReviewResponse[]) {
    this._reviews.set(value || []);
  }
  get reviews(): ReviewResponse[] {
    return this._reviews();
  }

  @Input() set productAverageRating(value: number) {
    this._productAverageRating.set(value || 0);
  }
  get productAverageRating(): number {
    return this._productAverageRating();
  }

  ratingDistribution = computed<{ [key: number]: number }>(() => {
    const list = this._reviews();
    const counts: { [key: number]: number } = { 5: 0, 4: 0, 3: 0, 2: 0, 1: 0 };
    if (list.length === 0) return counts;
    list.forEach(r => {
      const star = Math.round(r.rating);
      if (star >= 1 && star <= 5) {
        counts[star]++;
      }
    });
    return counts;
  });

  ratingPercentages = computed<{ [key: number]: number }>(() => {
    const dist = this.ratingDistribution();
    const total = this._reviews().length;
    const percentages: { [key: number]: number } = { 5: 0, 4: 0, 3: 0, 2: 0, 1: 0 };
    if (total <= 0) return percentages;
    for (let key in dist) {
      const k = Number(key);
      percentages[k] = parseFloat(((dist[k] / total) * 100).toFixed(1));
    }
    return percentages;
  });

  averageRatingValue = computed(() => {
    const list = this._reviews();
    if (list.length === 0) return this._productAverageRating();
    const sum = list.reduce((acc, curr) => acc + curr.rating, 0);
    return parseFloat((sum / list.length).toFixed(1));
  });

  getStarArray(rating: number): number[] {
    const fullStars = Math.floor(rating);
    const halfStar = rating % 1 >= 0.5 ? 1 : 0;
    const emptyStars = 5 - fullStars - halfStar;

    const stars: number[] = [];
    for (let i = 0; i < fullStars; i++) stars.push(1);
    if (halfStar) stars.push(0.5);
    for (let i = 0; i < emptyStars; i++) stars.push(0);
    return stars;
  }
}
