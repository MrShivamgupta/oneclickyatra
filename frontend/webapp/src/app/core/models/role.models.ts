export interface Permission {
  key: string;
  description?: string | null;
}

export interface RolePermissions {
  roleId: string;
  roleName: string;
  description?: string | null;
  permissionKeys: string[];
}

/** The whole role x permission grid the "Roles and Permissions" admin page renders as a matrix. */
export interface PermissionMatrix {
  permissions: Permission[];
  roles: RolePermissions[];
}

export interface UpdateRolePermissionsRequest {
  permissionKeys: string[];
}
