-- Development/demo accounts for testing role-restricted behavior (until now only SuperAdmin and
-- a self-registered Customer existed). Never run against Production.

DECLARE @AgentUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @OpsUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @FinanceUserId UNIQUEIDENTIFIER = NEWID();

-- PasswordHash below is BCrypt("Agent@12345", workFactor: 12).
INSERT INTO Users (Id, Email, FullName, PasswordHash, IsActive)
VALUES (
    @AgentUserId,
    'agent@oneclickyatra.dev',
    'Ananya Agent',
    '$2a$12$sTT7HpOF2JZnDxISYOYwKumC3jzxmiaGONGRSmJx1eTaES36LqCsC',
    1
);

-- PasswordHash below is BCrypt("Ops@12345", workFactor: 12).
INSERT INTO Users (Id, Email, FullName, PasswordHash, IsActive)
VALUES (
    @OpsUserId,
    'ops@oneclickyatra.dev',
    'Omkar Operations',
    '$2a$12$q6TeQpyoxEu50QhPYCVb1OGLxkxsPAeC90z2/TNiKg98jENnldYby',
    1
);

-- PasswordHash below is BCrypt("Finance@12345", workFactor: 12).
INSERT INTO Users (Id, Email, FullName, PasswordHash, IsActive)
VALUES (
    @FinanceUserId,
    'finance@oneclickyatra.dev',
    'Fiona Finance',
    '$2a$12$kKFl3QkiQ8hR4KyNliFwKeC/V1VHvPDmc4pSInvGQkbfAJqV2RVbC',
    1
);

INSERT INTO UserRoles (UserId, RoleId)
SELECT @AgentUserId, Id FROM Roles WHERE Name = 'TravelAgent';

INSERT INTO UserRoles (UserId, RoleId)
SELECT @OpsUserId, Id FROM Roles WHERE Name = 'OperationsStaff';

INSERT INTO UserRoles (UserId, RoleId)
SELECT @FinanceUserId, Id FROM Roles WHERE Name = 'Finance';
GO
