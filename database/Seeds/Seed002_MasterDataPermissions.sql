-- Adds permission keys introduced by Phase 1 (master data / destinations) that did not exist
-- when Seed001 ran, and grants them to the appropriate roles.

INSERT INTO Permissions (Id, [Key], Description)
SELECT NEWID(), v.[Key], v.Description
FROM (VALUES
    ('destination.view', 'View destinations'),
    ('destination.create', 'Create destinations'),
    ('destination.update', 'Update destinations'),
    ('destination.delete', 'Delete destinations'),
    ('masterdata.view', 'View master data (countries/cities/categories/seasons)'),
    ('masterdata.manage', 'Manage master data (countries/cities/categories/seasons)')
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

-- TravelAgent and OperationsStaff need to browse destinations (package building, quotations)
-- but not manage master data or delete destinations.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
INNER JOIN Permissions p ON p.[Key] IN ('destination.view', 'masterdata.view')
WHERE r.Name IN ('TravelAgent', 'OperationsStaff')
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions existing WHERE existing.RoleId = r.Id AND existing.PermissionId = p.Id
  );
GO
