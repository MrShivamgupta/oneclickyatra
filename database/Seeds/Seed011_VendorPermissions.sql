-- Adds the Vendor role and the vendor.* permission keys for the Vendor Management domain,
-- and grants them the same way Seed001/Seed009 grant Booking permissions.

-- Vendor role: portal access only, exactly like Customer (Seed001) — it is granted NO
-- permissions below. Vendor Portal endpoints authorize by ownership ([Authorize] +
-- Vendors.UserId match), never by [HasPermission].
INSERT INTO Roles (Id, Name, Description)
SELECT NEWID(), 'Vendor', 'Vendor portal access only.'
WHERE NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Vendor');
GO

INSERT INTO Permissions (Id, [Key], Description)
SELECT NEWID(), v.[Key], v.Description
FROM (VALUES
    ('vendor.view', 'View vendors'),
    ('vendor.create', 'Create vendors'),
    ('vendor.update', 'Update vendors'),
    ('vendor.delete', 'Delete vendors')
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

-- OperationsStaff: manages day-to-day vendor coordination, but cannot delete vendors.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
INNER JOIN Permissions p ON p.[Key] IN ('vendor.view', 'vendor.create', 'vendor.update')
WHERE r.Name = 'OperationsStaff'
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions existing WHERE existing.RoleId = r.Id AND existing.PermissionId = p.Id
  );
GO
