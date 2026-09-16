-- Adds the booking.update / booking.delete permission keys introduced by the Booking Engine
-- (booking.view/create/cancel/refund already existed from Seed001) and grants them.

INSERT INTO Permissions (Id, [Key], Description)
SELECT NEWID(), v.[Key], v.Description
FROM (VALUES
    ('booking.update', 'Update bookings (details, passengers, add-ons, status)'),
    ('booking.delete', 'Delete bookings')
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

-- TravelAgent and OperationsStaff: run bookings through their lifecycle day-to-day, but cannot delete.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
INNER JOIN Permissions p ON p.[Key] = 'booking.update'
WHERE r.Name IN ('TravelAgent', 'OperationsStaff')
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions existing WHERE existing.RoleId = r.Id AND existing.PermissionId = p.Id
  );
GO
