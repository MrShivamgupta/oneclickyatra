import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../../core/models/api.models';
import { PackageService } from '../../../core/services/package.service';
import { DestinationService } from '../../../core/services/destination.service';
import { CategoryService } from '../../../core/services/category.service';
import { SeasonService } from '../../../core/services/season.service';
import { Destination, Category, Season } from '../../../core/models/master-data.models';
import { Spinner } from '../../../shared/components/spinner/spinner';
import {
  PackageInclusion,
  PackageInventoryDeparture,
  PackageItineraryDay,
  PackageMediaItem,
  PackagePricingTier
} from '../../../core/models/package.models';

type TabKey = 'basic' | 'itinerary' | 'inclusions' | 'pricing' | 'inventory' | 'media';

@Component({
  selector: 'app-package-editor',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, RouterLink, Spinner],
  templateUrl: './package-editor.html',
  styleUrl: './package-editor.scss'
})
export class PackageEditor {
  private readonly packageService = inject(PackageService);
  private readonly destinationService = inject(DestinationService);
  private readonly categoryService = inject(CategoryService);
  private readonly seasonService = inject(SeasonService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly packageId = signal<string | null>(null);
  readonly activeTab = signal<TabKey>('basic');
  readonly loading = signal(false);

  readonly destinations = signal<Destination[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly seasons = signal<Season[]>([]);

  readonly basicError = signal<string | null>(null);
  readonly basicSaving = signal(false);
  readonly sectionError = signal<string | null>(null);
  readonly sectionSaving = signal(false);
  readonly sectionSaved = signal<TabKey | null>(null);

  readonly itineraryDays = signal<PackageItineraryDay[]>([]);
  readonly inclusions = signal<PackageInclusion[]>([]);
  readonly pricingTiers = signal<PackagePricingTier[]>([]);
  readonly inventoryDepartures = signal<PackageInventoryDeparture[]>([]);
  readonly mediaItems = signal<PackageMediaItem[]>([]);

  readonly tabs: { key: TabKey; label: string }[] = [
    { key: 'basic', label: 'Basic Info' },
    { key: 'itinerary', label: 'Itinerary' },
    { key: 'inclusions', label: 'Inclusions & Exclusions' },
    { key: 'pricing', label: 'Pricing' },
    { key: 'inventory', label: 'Inventory' },
    { key: 'media', label: 'Media' }
  ];

  readonly basicForm = this.formBuilder.nonNullable.group({
    destinationId: ['', Validators.required],
    categoryId: [''],
    seasonId: [''],
    title: ['', [Validators.required, Validators.maxLength(200)]],
    slug: ['', [Validators.required, Validators.pattern(/^[a-z0-9]+(-[a-z0-9]+)*$/)]],
    durationDays: [1, [Validators.required, Validators.min(1)]],
    durationNights: [0, [Validators.required, Validators.min(0)]],
    shortDescription: [''],
    description: [''],
    heroImageUrl: ['']
  });

  constructor() {
    this.destinationService.search({ pageNumber: 1, pageSize: 100 }).subscribe((response) => {
      if (response.success && response.data) this.destinations.set(response.data.items);
    });
    this.categoryService.list({ pageNumber: 1, pageSize: 100 }).subscribe((response) => {
      if (response.success && response.data) this.categories.set(response.data.items);
    });
    this.seasonService.list({ pageNumber: 1, pageSize: 100 }).subscribe((response) => {
      if (response.success && response.data) this.seasons.set(response.data.items);
    });

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam && idParam !== 'new') {
      this.packageId.set(idParam);
      this.loadPackage(idParam);
    }
  }

  get isNew(): boolean {
    return this.packageId() === null;
  }

  selectTab(tab: TabKey): void {
    this.sectionError.set(null);
    this.sectionSaved.set(null);
    this.activeTab.set(tab);
  }

  private loadPackage(id: string): void {
    this.loading.set(true);
    this.packageService.getById(id).subscribe({
      next: (response) => {
        this.loading.set(false);
        if (!response.success || !response.data) return;
        const detail = response.data;
        this.basicForm.setValue({
          destinationId: detail.package.destinationId,
          categoryId: detail.package.categoryId ?? '',
          seasonId: detail.package.seasonId ?? '',
          title: detail.package.title,
          slug: detail.package.slug,
          durationDays: detail.package.durationDays,
          durationNights: detail.package.durationNights,
          shortDescription: detail.package.shortDescription ?? '',
          description: detail.package.description ?? '',
          heroImageUrl: detail.package.heroImageUrl ?? ''
        });
        this.itineraryDays.set(detail.itinerary);
        this.inclusions.set(detail.inclusions);
        this.pricingTiers.set(detail.pricing);
        this.inventoryDepartures.set(detail.inventory);
        this.mediaItems.set(detail.media);
      },
      error: () => this.loading.set(false)
    });
  }

  private buildBasicRequest() {
    const raw = this.basicForm.getRawValue();
    return {
      destinationId: raw.destinationId,
      categoryId: raw.categoryId || null,
      seasonId: raw.seasonId || null,
      title: raw.title,
      slug: raw.slug,
      durationDays: Number(raw.durationDays),
      durationNights: Number(raw.durationNights),
      shortDescription: raw.shortDescription || null,
      description: raw.description || null,
      heroImageUrl: raw.heroImageUrl || null
    };
  }

  saveBasicInfo(): void {
    if (this.basicForm.invalid) {
      this.basicForm.markAllAsTouched();
      return;
    }

    this.basicSaving.set(true);
    this.basicError.set(null);
    const request = this.buildBasicRequest();
    const currentId = this.packageId();
    const save$ = currentId ? this.packageService.update(currentId, request) : this.packageService.create(request);

    save$.subscribe({
      next: (response) => {
        this.basicSaving.set(false);
        if (response.success && response.data) {
          if (!currentId) {
            this.packageId.set(response.data.id);
            this.router.navigate(['/admin/packages', response.data.id], { replaceUrl: true });
          }
        }
      },
      error: (error) => {
        this.basicSaving.set(false);
        this.basicError.set(error?.error?.message ?? 'Something went wrong.');
      }
    });
  }

  // Itinerary
  addItineraryDay(): void {
    const nextDay = this.itineraryDays().length + 1;
    this.itineraryDays.update((days) => [...days, { dayNumber: nextDay, title: '', description: '' }]);
  }

  removeItineraryDay(index: number): void {
    this.itineraryDays.update((days) => days.filter((_, i) => i !== index));
  }

  saveItinerary(): void {
    this.runSectionSave('itinerary', this.packageService.replaceItinerary(this.packageId()!, this.itineraryDays()));
  }

  // Inclusions
  addInclusion(isIncluded: boolean): void {
    const sortOrder = this.inclusions().length + 1;
    this.inclusions.update((items) => [...items, { description: '', isIncluded, sortOrder }]);
  }

  removeInclusion(index: number): void {
    this.inclusions.update((items) => items.filter((_, i) => i !== index));
  }

  saveInclusions(): void {
    this.runSectionSave('inclusions', this.packageService.replaceInclusions(this.packageId()!, this.inclusions()));
  }

  // Pricing
  addPricingTier(): void {
    this.pricingTiers.update((tiers) => [...tiers, { tierName: '', pricePerPerson: 0, currency: 'INR' }]);
  }

  removePricingTier(index: number): void {
    this.pricingTiers.update((tiers) => tiers.filter((_, i) => i !== index));
  }

  savePricing(): void {
    this.runSectionSave('pricing', this.packageService.replacePricing(this.packageId()!, this.pricingTiers()));
  }

  // Inventory
  addInventoryDeparture(): void {
    this.inventoryDepartures.update((rows) => [
      ...rows,
      { departureDate: '', totalSeats: 0, bookedSeats: 0, status: 'Open' }
    ]);
  }

  removeInventoryDeparture(index: number): void {
    this.inventoryDepartures.update((rows) => rows.filter((_, i) => i !== index));
  }

  saveInventory(): void {
    this.runSectionSave('inventory', this.packageService.replaceInventory(this.packageId()!, this.inventoryDepartures()));
  }

  // Media
  addMediaItem(): void {
    const sortOrder = this.mediaItems().length + 1;
    this.mediaItems.update((items) => [...items, { mediaUrl: '', mediaType: 'Image', sortOrder, isCoverImage: false }]);
  }

  removeMediaItem(index: number): void {
    this.mediaItems.update((items) => items.filter((_, i) => i !== index));
  }

  saveMedia(): void {
    this.runSectionSave('media', this.packageService.replaceMedia(this.packageId()!, this.mediaItems()));
  }

  private runSectionSave<T>(tab: TabKey, observable: Observable<ApiResponse<T>>): void {
    this.sectionSaving.set(true);
    this.sectionError.set(null);
    this.sectionSaved.set(null);
    observable.subscribe({
      next: () => {
        this.sectionSaving.set(false);
        this.sectionSaved.set(tab);
      },
      error: (error: { error?: { message?: string } }) => {
        this.sectionSaving.set(false);
        this.sectionError.set(error?.error?.message ?? 'Something went wrong.');
      }
    });
  }
}
