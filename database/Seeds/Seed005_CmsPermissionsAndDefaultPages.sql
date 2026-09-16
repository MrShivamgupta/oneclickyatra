-- Adds CMS page-management permissions (Phase: CMS Pages) and seeds default published pages
-- so the public site has real content (About/Contact/Terms/Privacy) immediately. Idempotent:
-- safe to re-run.

INSERT INTO Permissions (Id, [Key], Description)
SELECT NEWID(), v.[Key], v.Description
FROM (VALUES
    ('cms.view', 'View CMS content pages'),
    ('cms.manage', 'Create/update/delete CMS content pages')
) AS v([Key], Description)
WHERE NOT EXISTS (SELECT 1 FROM Permissions p WHERE p.[Key] = v.[Key] AND p.IsDeleted = 0);
GO

-- SuperAdmin: grant both new permissions.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Name = 'SuperAdmin'
  AND p.[Key] IN ('cms.view', 'cms.manage')
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions existing WHERE existing.RoleId = r.Id AND existing.PermissionId = p.Id
  );
GO

-- Default published pages. Content below is short placeholder copy for a travel agency
-- called "One Click Yatra" -- replace with real copy before going live.
INSERT INTO Pages (Id, Slug, Title, Content, IsPublished, CreatedAt, IsDeleted)
SELECT NEWID(), 'about', 'About Us',
    N'One Click Yatra is a travel CRM and booking platform that helps travel agencies plan, manage, and sell tour packages with ease. We connect travelers with curated destinations, transparent pricing, and dependable support at every step. This is placeholder content -- replace it with your agency''s real story.',
    1, SYSUTCDATETIME(), 0
WHERE NOT EXISTS (SELECT 1 FROM Pages WHERE Slug = 'about' AND IsDeleted = 0);
GO

INSERT INTO Pages (Id, Slug, Title, Content, IsPublished, CreatedAt, IsDeleted)
SELECT NEWID(), 'contact', 'Contact Us',
    N'Have a question about your trip or booking? Reach the One Click Yatra team at support@oneclickyatra.example or call +91-98765-43210, Monday to Saturday, 9 AM to 7 PM IST. This is placeholder contact information -- replace it with your real details.',
    1, SYSUTCDATETIME(), 0
WHERE NOT EXISTS (SELECT 1 FROM Pages WHERE Slug = 'contact' AND IsDeleted = 0);
GO

INSERT INTO Pages (Id, Slug, Title, Content, IsPublished, CreatedAt, IsDeleted)
SELECT NEWID(), 'terms', 'Terms and Conditions',
    N'By booking through One Click Yatra you agree to our booking, payment, and cancellation terms as communicated at the time of reservation. All packages are subject to availability and third-party supplier policies. THIS IS PLACEHOLDER LEGAL TEXT -- replace it with terms reviewed by qualified legal counsel before going live.',
    1, SYSUTCDATETIME(), 0
WHERE NOT EXISTS (SELECT 1 FROM Pages WHERE Slug = 'terms' AND IsDeleted = 0);
GO

INSERT INTO Pages (Id, Slug, Title, Content, IsPublished, CreatedAt, IsDeleted)
SELECT NEWID(), 'privacy', 'Privacy Policy',
    N'One Click Yatra collects only the information needed to process bookings and improve our services, and we never sell your personal data to third parties. THIS IS PLACEHOLDER LEGAL TEXT -- replace it with a privacy policy reviewed by qualified legal counsel before going live.',
    1, SYSUTCDATETIME(), 0
WHERE NOT EXISTS (SELECT 1 FROM Pages WHERE Slug = 'privacy' AND IsDeleted = 0);
GO
