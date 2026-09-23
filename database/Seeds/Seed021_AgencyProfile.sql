-- Exactly one default row, matching the demo SuperAdmin's own brand. Idempotent so it's safe to
-- re-run and never clobbers whatever an admin has already edited via the Settings > Agency Profile tab.
IF NOT EXISTS (SELECT 1 FROM AgencyProfile WHERE IsDeleted = 0)
INSERT INTO AgencyProfile (Id, Name, LogoUrl, Address, GstNumber, Currency, SupportEmail, SupportPhone, CreatedAt, IsDeleted)
VALUES (NEWID(), 'One Click Yatra', NULL, NULL, NULL, 'INR', 'support@oneclickyatra.dev', NULL, SYSUTCDATETIME(), 0);
GO
