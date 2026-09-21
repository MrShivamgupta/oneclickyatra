/** Mirrors the backend's AuditLogResponse (GET /api/v1/audit-logs). ActorUserId/ActorName are
 * null for system actions with no acting user — ActorName already combines the actor's name and
 * email into one display string server-side (see AuditLogAppFunction.CombineActorName). */
export interface AuditLog {
  id: string;
  actorUserId?: string | null;
  actorName?: string | null;
  action: string;
  entityName: string;
  entityId?: string | null;
  oldValue?: string | null;
  newValue?: string | null;
  ipAddress?: string | null;
  createdAt: string;
}

/** Mirrors the backend's AuditLogSearchRequest. All filters are optional and combine with AND;
 * fromDate/toDate are plain 'yyyy-MM-dd' dates scoped against AuditLogs.CreatedAt. */
export interface AuditLogSearchParams {
  pageNumber: number;
  pageSize: number;
  userId?: string;
  action?: string;
  entityName?: string;
  fromDate?: string;
  toDate?: string;
}
