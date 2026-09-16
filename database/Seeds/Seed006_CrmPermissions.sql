-- Adds permission keys introduced by the CRM domain (Leads/Customers/FollowUps) and grants them
-- to the appropriate roles. lead.* already existed from Phase 0's seed; only customer.* and
-- followup.* are new here.

INSERT INTO Permissions (Id, [Key], Description)
SELECT NEWID(), v.[Key], v.Description
FROM (VALUES
    ('customer.view', 'View customers'),
    ('customer.create', 'Create customers'),
    ('customer.update', 'Update customers'),
    ('customer.delete', 'Delete customers'),
    ('followup.view', 'View follow-ups'),
    ('followup.create', 'Create follow-ups'),
    ('followup.update', 'Update follow-ups'),
    ('followup.delete', 'Delete follow-ups')
) AS v([Key], Description)
WHERE NOT EXISTS (SELECT 1 FROM Permissions p WHERE p.[Key] = v.[Key] AND p.IsDeleted = 0);
GO

-- SuperAdmin: every permission, including the ones just inserted above.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Name = 'SuperAdmin'
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions existing WHERE existing.RoleId = r.Id AND existing.PermissionId = p.Id
  );
GO

-- TravelAgent: works leads day-to-day, so full CRM access except deleting customers/leads.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
INNER JOIN Permissions p ON p.[Key] IN (
    'customer.view', 'customer.create', 'customer.update',
    'followup.view', 'followup.create', 'followup.update', 'followup.delete'
)
WHERE r.Name = 'TravelAgent'
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions existing WHERE existing.RoleId = r.Id AND existing.PermissionId = p.Id
  );
GO

-- OperationsStaff: read-only visibility into customers/follow-ups for operational context.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
INNER JOIN Permissions p ON p.[Key] IN ('customer.view', 'followup.view')
WHERE r.Name = 'OperationsStaff'
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions existing WHERE existing.RoleId = r.Id AND existing.PermissionId = p.Id
  );
GO
