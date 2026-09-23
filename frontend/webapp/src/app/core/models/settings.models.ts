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
