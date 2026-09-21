-- Seeds the four default WhatsApp templates required by the SRS (Welcome / Quotation / Reminder /
-- TravelAlert) and a dedicated whatsapp.manage permission key (chosen over reusing cms.manage —
-- template/message management is its own admin surface with its own audit trail, not CMS page
-- content — see the backend summary for the full rationale), granted to SuperAdmin and
-- OperationsStaff (the roles that already run bookings/CRM day-to-day).

INSERT INTO WhatsAppTemplates (Id, Name, Category, BodyText, IsActive, CreatedAt, IsDeleted)
SELECT NEWID(), v.Name, v.Category, v.BodyText, 1, SYSUTCDATETIME(), 0
FROM (VALUES
    ('welcome_greeting', 'Welcome',
        'Hi {{1}}, welcome to One Click Yatra! We are excited to help you plan an unforgettable trip. Reply anytime and one of our travel experts will assist you.'),
    ('quotation_ready', 'Quotation',
        'Hi {{1}}, your quotation for {{2}} is ready. Total amount: {{3}}. View full details and confirm your booking here: {{4}}'),
    ('payment_reminder', 'Reminder',
        'Hi {{1}}, a quick reminder that a payment of {{2}} for your booking {{3}} is due soon. Complete it here: {{4}}'),
    ('travel_alert', 'TravelAlert',
        'Hi {{1}}, important update regarding your upcoming trip to {{2}}: {{3}}. Contact us if you have any questions.')
) AS v(Name, Category, BodyText)
WHERE NOT EXISTS (SELECT 1 FROM WhatsAppTemplates t WHERE t.Name = v.Name AND t.IsDeleted = 0);
GO

INSERT INTO Permissions (Id, [Key], Description)
SELECT NEWID(), v.[Key], v.Description
FROM (VALUES
    ('whatsapp.manage', 'Manage WhatsApp templates, send messages and view notification logs')
) AS v([Key], Description)
WHERE NOT EXISTS (SELECT 1 FROM Permissions p WHERE p.[Key] = v.[Key] AND p.IsDeleted = 0);
GO

-- SuperAdmin: every permission, including the one just inserted above.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
CROSS JOIN Permissions p
WHERE r.Name = 'SuperAdmin'
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions existing WHERE existing.RoleId = r.Id AND existing.PermissionId = p.Id
  );
GO

-- OperationsStaff: manage WhatsApp templates, send messages and view notification logs day-to-day.
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT r.Id, p.Id
FROM Roles r
INNER JOIN Permissions p ON p.[Key] = 'whatsapp.manage'
WHERE r.Name = 'OperationsStaff'
  AND NOT EXISTS (
      SELECT 1 FROM RolePermissions existing WHERE existing.RoleId = r.Id AND existing.PermissionId = p.Id
  );
GO
