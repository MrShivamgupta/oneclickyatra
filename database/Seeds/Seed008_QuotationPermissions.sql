-- Adds permission keys for the Quotations domain and grants them to the appropriate roles.

INSERT INTO Permissions (Id, [Key], Description)
SELECT NEWID(), v.[Key], v.Description
FROM (VALUES
    ('quotation.view', 'View quotations'),
    ('quotation.create', 'Create quotations'),
    ('quotation.update', 'Update quotations'),
    ('quotation.delete', 'Delete quotations')
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

-- TravelAgent: builds and sends quotations day-to-day, but cannot delete them.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
INNER JOIN Permissions p ON p.[Key] IN ('quotation.view', 'quotation.create', 'quotation.update')
WHERE r.Name = 'TravelAgent'
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions existing WHERE existing.RoleId = r.Id AND existing.PermissionId = p.Id
  );
GO

-- OperationsStaff and Finance: read-only visibility for booking prep and pricing oversight.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
INNER JOIN Permissions p ON p.[Key] = 'quotation.view'
WHERE r.Name IN ('OperationsStaff', 'Finance')
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions existing WHERE existing.RoleId = r.Id AND existing.PermissionId = p.Id
  );
GO
