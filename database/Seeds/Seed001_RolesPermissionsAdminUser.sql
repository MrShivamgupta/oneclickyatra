-- Development/demo seed data. Never run against Production.
-- Dev SuperAdmin credentials: superadmin@oneclickyatra.dev / Admin@12345 (change immediately outside dev).

DECLARE @RoleSuperAdmin UNIQUEIDENTIFIER = NEWID();
DECLARE @RoleTravelAgent UNIQUEIDENTIFIER = NEWID();
DECLARE @RoleOperationsStaff UNIQUEIDENTIFIER = NEWID();
DECLARE @RoleFinance UNIQUEIDENTIFIER = NEWID();
DECLARE @RoleCustomer UNIQUEIDENTIFIER = NEWID();
DECLARE @RoleGuest UNIQUEIDENTIFIER = NEWID();

INSERT INTO Roles (Id, Name, Description)
VALUES
    (@RoleSuperAdmin, 'SuperAdmin', 'Full system access.'),
    (@RoleTravelAgent, 'TravelAgent', 'Sales CRM: leads, quotations, bookings.'),
    (@RoleOperationsStaff, 'OperationsStaff', 'Operations: bookings, vendors, departures.'),
    (@RoleFinance, 'Finance', 'Payments, invoices, refunds, financial reports.'),
    (@RoleCustomer, 'Customer', 'Customer portal access only.'),
    (@RoleGuest, 'Guest', 'Public/unauthenticated access.');
GO

INSERT INTO Permissions (Id, [Key], Description)
VALUES
    (NEWID(), 'dashboard.view', 'View admin dashboard'),
    (NEWID(), 'lead.view', 'View leads'),
    (NEWID(), 'lead.create', 'Create leads'),
    (NEWID(), 'lead.update', 'Update leads'),
    (NEWID(), 'lead.delete', 'Delete leads'),
    (NEWID(), 'package.view', 'View packages'),
    (NEWID(), 'package.create', 'Create packages'),
    (NEWID(), 'package.update', 'Update packages'),
    (NEWID(), 'package.delete', 'Delete packages'),
    (NEWID(), 'booking.view', 'View bookings'),
    (NEWID(), 'booking.create', 'Create bookings'),
    (NEWID(), 'booking.cancel', 'Cancel bookings'),
    (NEWID(), 'booking.refund', 'Refund bookings'),
    (NEWID(), 'payment.view', 'View payments'),
    (NEWID(), 'payment.create', 'Create payments'),
    (NEWID(), 'payment.refund', 'Refund payments'),
    (NEWID(), 'report.view', 'View reports'),
    (NEWID(), 'user.manage', 'Manage users'),
    (NEWID(), 'role.manage', 'Manage roles/permissions'),
    (NEWID(), 'settings.manage', 'Manage application settings'),
    (NEWID(), 'audit.view', 'View audit logs');
GO

-- SuperAdmin gets every permission.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Name = 'SuperAdmin';
GO

-- TravelAgent: CRM + sales workflow.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
INNER JOIN Permissions p ON p.[Key] IN (
    'dashboard.view', 'lead.view', 'lead.create', 'lead.update',
    'package.view', 'booking.view', 'booking.create', 'report.view'
)
WHERE r.Name = 'TravelAgent';
GO

-- OperationsStaff: bookings + packages operations.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
INNER JOIN Permissions p ON p.[Key] IN (
    'dashboard.view', 'booking.view', 'booking.create', 'booking.cancel',
    'package.view', 'package.update', 'report.view'
)
WHERE r.Name = 'OperationsStaff';
GO

-- Finance: payments/invoices/refunds/reports.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
INNER JOIN Permissions p ON p.[Key] IN (
    'dashboard.view', 'payment.view', 'payment.create', 'payment.refund',
    'booking.view', 'booking.refund', 'report.view'
)
WHERE r.Name = 'Finance';
GO

-- Dev SuperAdmin user. PasswordHash below is BCrypt("Admin@12345", workFactor: 12).
DECLARE @AdminUserId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Users (Id, Email, FullName, PasswordHash, IsActive)
VALUES (
    @AdminUserId,
    'superadmin@oneclickyatra.dev',
    'One Click Yatra Super Admin',
    '$2a$12$SeNoE1T6oxZ9AFnoZBMkHOUcntmLi/ldOO5xKOXeP6T6mhAn04Kka',
    1
);

INSERT INTO UserRoles (UserId, RoleId)
SELECT @AdminUserId, Id FROM Roles WHERE Name = 'SuperAdmin';
GO
