-- Email notification templates (Communication domain), mirroring WhatsAppTemplates from
-- V021__CreateWhatsAppAndNotifications.sql. NotificationLogs was already made channel-agnostic in
-- that migration (Channel CHECK already allows 'Email') specifically so a future Email channel
-- could log through it without a new migration -- this script only adds the template store itself.
-- BodyHtml may contain {{PlaceholderName}}-style tokens; substitution is simple literal string
-- replacement (see Services/Email/EmailTemplateService.cs), not Handlebars/Razor.

CREATE TABLE EmailTemplates
(
    Id        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EmailTemplates PRIMARY KEY,
    Name      NVARCHAR(100)    NOT NULL,
    Subject   NVARCHAR(200)    NOT NULL,
    BodyHtml  NVARCHAR(MAX)    NOT NULL,
    IsActive  BIT              NOT NULL CONSTRAINT DF_EmailTemplates_IsActive DEFAULT (1),
    CreatedAt DATETIME2        NOT NULL CONSTRAINT DF_EmailTemplates_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2        NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT              NOT NULL CONSTRAINT DF_EmailTemplates_IsDeleted DEFAULT (0),
    CONSTRAINT UQ_EmailTemplates_Name UNIQUE (Name)
);
GO
