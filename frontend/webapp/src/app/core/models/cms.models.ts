export interface CmsPage {
  id: string;
  slug: string;
  title: string;
  content?: string;
  isPublished: boolean;
}
