export interface PackageSummary {
  id: string;
  destinationId: string;
  destinationName: string;
  categoryId?: string | null;
  categoryName?: string | null;
  seasonId?: string | null;
  seasonName?: string | null;
  title: string;
  slug: string;
  durationDays: number;
  durationNights: number;
  shortDescription?: string | null;
  description?: string | null;
  heroImageUrl?: string | null;
  status: 'Draft' | 'Published';
  startingPricePerPerson?: number | null;
  priceCurrency?: string | null;
}

export interface PackageRequest {
  destinationId: string;
  categoryId?: string | null;
  seasonId?: string | null;
  title: string;
  slug: string;
  durationDays: number;
  durationNights: number;
  shortDescription?: string | null;
  description?: string | null;
  heroImageUrl?: string | null;
}

export interface PackageSearchParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  destinationId?: string;
  categoryId?: string;
  status?: string;
}

export interface PackageItineraryDay {
  id?: string;
  dayNumber: number;
  title: string;
  description?: string | null;
}

export interface PackageInclusion {
  id?: string;
  description: string;
  isIncluded: boolean;
  sortOrder: number;
}

export interface PackagePricingTier {
  id?: string;
  tierName: string;
  hotelCategory?: string | null;
  pricePerPerson: number;
  childPrice?: number | null;
  validFrom?: string | null;
  validTo?: string | null;
  currency: string;
}

export interface PackageInventoryDeparture {
  id?: string;
  departureDate: string;
  totalSeats: number;
  bookedSeats: number;
  availableSeats?: number;
  status: 'Open' | 'Closed' | 'SoldOut';
}

export interface PackageMediaItem {
  id?: string;
  mediaUrl: string;
  mediaType: 'Image' | 'Video';
  sortOrder: number;
  isCoverImage: boolean;
}

export interface PackageDetail {
  package: PackageSummary;
  itinerary: PackageItineraryDay[];
  inclusions: PackageInclusion[];
  pricing: PackagePricingTier[];
  inventory: PackageInventoryDeparture[];
  media: PackageMediaItem[];
}
