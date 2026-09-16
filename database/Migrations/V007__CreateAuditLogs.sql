CREATE TABLE AuditLogs
(
    Id         UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
    UserId     UNIQUEIDENTIFIER NULL,
    Action     NVARCHAR(100)    NOT NULL,
    EntityName NVARCHAR(100)    NOT NULL,
    EntityId   NVARCHAR(100)    NULL,
    OldValue   NVARCHAR(MAX)    NULL,
    NewValue   NVARCHAR(MAX)    NULL,
    IpAddress  NVARCHAR(64)     NULL,
    UserAgent  NVARCHAR(400)    NULL,
    TrackingId NVARCHAR(64)     NOT NULL,
    CreatedAt  DATETIME2        NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT (SYSUTCDATETIME())
);
GO

CREATE INDEX IX_AuditLogs_UserId_CreatedAt ON AuditLogs (UserId, CreatedAt DESC);
GO

CREATE INDEX IX_AuditLogs_EntityName_EntityId ON AuditLogs (EntityName, EntityId);
GO
