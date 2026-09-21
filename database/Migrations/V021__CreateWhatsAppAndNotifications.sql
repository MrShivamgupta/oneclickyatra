-- WhatsApp Business integration + shared notification log (Communication domain per the SRS:
-- NotificationLogs, EmailTemplates, WhatsAppTemplates). Only WhatsApp is wired up to actually
-- send/receive in this pass; NotificationLogs is deliberately channel-agnostic (Channel column)
-- so a future Email channel can log through the same table without a new migration.

CREATE TABLE WhatsAppTemplates
(
    Id        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_WhatsAppTemplates PRIMARY KEY,
    Name      NVARCHAR(100)    NOT NULL,
    Category  NVARCHAR(20)     NOT NULL,
    BodyText  NVARCHAR(1000)   NOT NULL,
    IsActive  BIT              NOT NULL CONSTRAINT DF_WhatsAppTemplates_IsActive DEFAULT (1),
    CreatedAt DATETIME2        NOT NULL CONSTRAINT DF_WhatsAppTemplates_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2        NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT              NOT NULL CONSTRAINT DF_WhatsAppTemplates_IsDeleted DEFAULT (0),
    CONSTRAINT UQ_WhatsAppTemplates_Name UNIQUE (Name),
    CONSTRAINT CK_WhatsAppTemplates_Category CHECK (Category IN ('Welcome', 'Quotation', 'Reminder', 'TravelAlert'))
);
GO

CREATE INDEX IX_WhatsAppTemplates_Category ON WhatsAppTemplates (Category);
GO

-- Every outbound/inbound notification, across every channel, is logged here: an outbound send
-- writes Status = Sent/Failed immediately (this table is the log, not a work queue — there is no
-- background dispatcher reading Status = 'Queued' rows yet), and an inbound WhatsApp webhook
-- message is logged with Status = 'Received'. GatewayMessageId is unique (where present) so a
-- retried inbound webhook delivery — Meta re-sends if it does not see a timely 200 — is a no-op
-- rather than a duplicate log row, the same idempotency approach already used for
-- PaymentTransactions.GatewayEventId.
CREATE TABLE NotificationLogs
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_NotificationLogs PRIMARY KEY,
    Channel          NVARCHAR(20)     NOT NULL,
    RecipientPhone   NVARCHAR(20)     NULL,
    RecipientEmail   NVARCHAR(256)    NULL,
    TemplateId       UNIQUEIDENTIFIER NULL,
    Subject          NVARCHAR(200)    NULL,
    Body             NVARCHAR(MAX)    NULL,
    Status           NVARCHAR(20)     NOT NULL CONSTRAINT DF_NotificationLogs_Status DEFAULT ('Queued'),
    GatewayMessageId NVARCHAR(150)    NULL,
    ErrorMessage     NVARCHAR(500)    NULL,
    SentAt           DATETIME2        NULL,
    CreatedAt        DATETIME2        NOT NULL CONSTRAINT DF_NotificationLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy        UNIQUEIDENTIFIER NULL,
    UpdatedAt        DATETIME2        NULL,
    UpdatedBy        UNIQUEIDENTIFIER NULL,
    IsDeleted        BIT              NOT NULL CONSTRAINT DF_NotificationLogs_IsDeleted DEFAULT (0),
    CONSTRAINT FK_NotificationLogs_WhatsAppTemplates FOREIGN KEY (TemplateId) REFERENCES WhatsAppTemplates (Id),
    CONSTRAINT CK_NotificationLogs_Channel CHECK (Channel IN ('WhatsApp', 'Email')),
    CONSTRAINT CK_NotificationLogs_Status CHECK (Status IN ('Queued', 'Sent', 'Failed', 'Received'))
);
GO

CREATE INDEX IX_NotificationLogs_TemplateId ON NotificationLogs (TemplateId);
GO
CREATE INDEX IX_NotificationLogs_Channel_Status ON NotificationLogs (Channel, Status);
GO
CREATE UNIQUE INDEX UX_NotificationLogs_GatewayMessageId ON NotificationLogs (GatewayMessageId) WHERE GatewayMessageId IS NOT NULL;
GO
