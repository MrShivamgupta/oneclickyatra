export interface Country {
  id: string;
  name: string;
  isoCode: string;
}

export interface CountryRequest {
  name: string;
  isoCode: string;
}

export interface City {
  id: string;
  countryId: string;
  countryName: string;
  name: string;
}

export interface CityRequest {
  countryId: string;
  name: string;
}

export interface Category {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
}

export interface CategoryRequest {
  name: string;
  slug: string;
  description?: string | null;
}

export interface Season {
  id: string;
  name: string;
  startMonth: number;
  endMonth: number;
}

export interface SeasonRequest {
  name: string;
  startMonth: number;
  endMonth: number;
}

export interface Destination {
  id: string;
  countryId: string;
  countryName: string;
  cityId?: string | null;
  cityName?: string | null;
  name: string;
  slug: string;
  shortDescription?: string | null;
  description?: string | null;
  heroImageUrl?: string | null;
  isFeatured: boolean;
  isPublished: boolean;
}

export interface DestinationRequest {
  countryId: string;
  cityId?: string | null;
  name: string;
  slug: string;
  shortDescription?: string | null;
  description?: string | null;
  heroImageUrl?: string | null;
  isFeatured: boolean;
  isPublished: boolean;
}

export interface DestinationSearchParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  countryId?: string;
  isFeatured?: boolean;
  isPublished?: boolean;
}
