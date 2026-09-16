import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CmsPageService } from '../../core/services/cms-page.service';
import { CmsPage as CmsPageModel } from '../../core/models/cms.models';

@Component({
  selector: 'app-cms-page',
  standalone: true,
  imports: [],
  templateUrl: './cms-page.html',
  styleUrl: './cms-page.scss'
})
export class CmsPage {
  private readonly route = inject(ActivatedRoute);
  private readonly cmsPageService = inject(CmsPageService);

  readonly page = signal<CmsPageModel | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);
  readonly error = signal<string | null>(null);

  constructor() {
    // Supports a static route (slug passed via route "data") as well as a future
    // dynamic route (slug passed via the "slug" route param) with no component changes.
    const slug = (this.route.snapshot.data['slug'] as string | undefined) ?? this.route.snapshot.paramMap.get('slug');

    if (slug) {
      this.load(slug);
    } else {
      this.loading.set(false);
      this.notFound.set(true);
    }
  }

  load(slug: string): void {
    this.loading.set(true);
    this.notFound.set(false);
    this.error.set(null);
    this.cmsPageService.getBySlug(slug).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (response.success && response.data) {
          this.page.set(response.data);
        } else {
          this.notFound.set(true);
        }
      },
      error: (error) => {
        this.loading.set(false);
        if (error?.status === 404) {
          this.notFound.set(true);
        } else {
          this.error.set(error?.error?.message ?? 'Could not load this page. Please try again.');
        }
      }
    });
  }
}
