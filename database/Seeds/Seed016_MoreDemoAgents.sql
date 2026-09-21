-- Adds two more demo TravelAgent accounts alongside the one Seed010 created, so multiple
-- concurrent staff/agent users can actually be exercised (lead assignment, per-agent Sales
-- Performance, audit-log actor attribution) instead of only ever testing with a single agent.
-- Never run against Production. Reuses Seed010's exact BCrypt("Agent@12345", workFactor: 12) hash
-- -- a fresh hash isn't needed since these are throwaway dev/demo credentials, same convention as
-- every other seeded account.

DECLARE @Agent2UserId UNIQUEIDENTIFIER = NEWID();
DECLARE @Agent3UserId UNIQUEIDENTIFIER = NEWID();

-- PasswordHash below is BCrypt("Agent@12345", workFactor: 12) -- same as agent@oneclickyatra.dev.
INSERT INTO Users (Id, Email, FullName, PasswordHash, IsActive)
VALUES (
    @Agent2UserId,
    'rohan.agent@oneclickyatra.dev',
    'Rohan Sharma',
    '$2a$12$sTT7HpOF2JZnDxISYOYwKumC3jzxmiaGONGRSmJx1eTaES36LqCsC',
    1
);

INSERT INTO Users (Id, Email, FullName, PasswordHash, IsActive)
VALUES (
    @Agent3UserId,
    'kavya.agent@oneclickyatra.dev',
    'Kavya Reddy',
    '$2a$12$sTT7HpOF2JZnDxISYOYwKumC3jzxmiaGONGRSmJx1eTaES36LqCsC',
    1
);

INSERT INTO UserRoles (UserId, RoleId)
SELECT @Agent2UserId, Id FROM Roles WHERE Name = 'TravelAgent';

INSERT INTO UserRoles (UserId, RoleId)
SELECT @Agent3UserId, Id FROM Roles WHERE Name = 'TravelAgent';
GO
