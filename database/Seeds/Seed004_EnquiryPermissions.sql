-- Adds permission keys introduced by the Enquiries module (public enquiry capture form) that did
-- not exist when earlier seeds ran, and grants them to the appropriate roles.

INSERT INTO Permissions (Id, [Key], Description)
SELECT NEWID(), v.[Key], v.Description
FROM (VALUES
    ('enquiry.view', 'View enquiries'),
    ('enquiry.update', 'Update enquiry status'),
    ('enquiry.delete', 'Delete enquiries')
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

-- TravelAgent and OperationsStaff need to view incoming enquiries (to follow up / build quotes)
-- but not update status or delete them.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
INNER JOIN Permissions p ON p.[Key] = 'enquiry.view'
WHERE r.Name IN ('TravelAgent', 'OperationsStaff')
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions existing WHERE existing.RoleId = r.Id AND existing.PermissionId = p.Id
  );
GO
