export interface EmailTemplate {
  id: string;
  name: string;
  subject: string;
  bodyHtml: string;
  isActive: boolean;
  updatedAt?: string | null;
}

export interface UpsertEmailTemplateRequest {
  name: string;
  subject: string;
  bodyHtml: string;
  isActive: boolean;
}

export interface EmailTemplateSearchParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  isActive?: boolean;
}

export interface IntegrationStatus {
  razorpayConfigured: boolean;
  whatsAppConfigured: boolean;
  emailConfigured: boolean;
}

export interface AgencyProfile {
  name: string;
  logoUrl?: string | null;
  address?: string | null;
  gstNumber?: string | null;
  currency: string;
  supportEmail?: string | null;
  supportPhone?: string | null;
  updatedAt?: string | null;
}

export interface UpdateAgencyProfileRequest {
  name: string;
  logoUrl?: string | null;
  address?: string | null;
  gstNumber?: string | null;
  currency: string;
  supportEmail?: string | null;
  supportPhone?: string | null;
}
