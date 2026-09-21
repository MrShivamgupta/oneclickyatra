/** Mirrors the backend's UserResponse and the 4-role validation shared by CreateStaffUserRequestValidator /
 * UpdateUserRoleRequestValidator, for the admin "Users" management screen (GET/POST/PUT /api/v1/users…).
 * This is staff accounts only — Customer/Guest/Vendor accounts are created through their own existing flows. */
export const STAFF_ROLES = ['TravelAgent', 'OperationsStaff', 'Finance', 'SuperAdmin'] as const;
export type StaffRole = (typeof STAFF_ROLES)[number];

export interface AppUser {
  id: string;
  fullName: string;
  email: string;
  isActive: boolean;
  roles: string[];
  createdAt: string;
}

/** Mirrors UserSearchRequest : PaginationRequest. */
export interface UserSearchParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  role?: string;
  isActive?: boolean;
}

export interface CreateStaffUserRequest {
  fullName: string;
  email: string;
  password: string;
  role: string;
}

export interface UpdateUserRoleRequest {
  role: string;
}

export interface UpdateUserStatusRequest {
  isActive: boolean;
}
