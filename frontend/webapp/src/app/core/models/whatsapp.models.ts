export const WHATSAPP_TEMPLATE_CATEGORIES = ['Welcome', 'Quotation', 'Reminder', 'TravelAlert'] as const;
export type WhatsAppTemplateCategory = (typeof WHATSAPP_TEMPLATE_CATEGORIES)[number];

export const NOTIFICATION_CHANNELS = ['WhatsApp', 'Email'] as const;
export type NotificationChannel = (typeof NOTIFICATION_CHANNELS)[number];

export const NOTIFICATION_STATUSES = ['Queued', 'Sent', 'Failed', 'Received'] as const;
export type NotificationStatus = (typeof NOTIFICATION_STATUSES)[number];

export interface WhatsAppTemplate {
  id: string;
  name: string;
  category: string;
  bodyText: string;
  isActive: boolean;
  createdAt: string;
}

export interface WhatsAppTemplateRequest {
  name: string;
  category: string;
  bodyText: string;
  isActive: boolean;
}

export interface WhatsAppTemplateSearchParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  category?: string;
  isActive?: boolean;
}

/** Sends a pre-approved Meta template message — free-form text cannot be sent outside the 24-hour
 * customer-service window, so every outbound send goes through a named template. */
export interface WhatsAppSendMessageRequest {
  phoneNumber: string;
  templateName: string;
  parameters: string[];
}

export interface NotificationLog {
  id: string;
  channel: string;
  recipientPhone?: string | null;
  recipientEmail?: string | null;
  templateId?: string | null;
  templateName?: string | null;
  subject?: string | null;
  body?: string | null;
  status: string;
  gatewayMessageId?: string | null;
  errorMessage?: string | null;
  sentAt?: string | null;
  createdAt: string;
}

export interface NotificationLogSearchParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  channel?: string;
  status?: string;
}
