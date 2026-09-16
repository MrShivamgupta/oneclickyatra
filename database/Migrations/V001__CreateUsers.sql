CREATE TABLE Users
(
    Id                   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Email                NVARCHAR(256)    NOT NULL,
    FullName             NVARCHAR(200)    NOT NULL,
    PasswordHash         NVARCHAR(256)    NOT NULL,
    IsActive             BIT              NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    FailedLoginAttempts  INT              NOT NULL CONSTRAINT DF_Users_FailedLoginAttempts DEFAULT (0),
    LockedOutUntilUtc    DATETIME2        NULL,
    CreatedAt            DATETIME2        NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy            UNIQUEIDENTIFIER NULL,
    UpdatedAt            DATETIME2        NULL,
    UpdatedBy            UNIQUEIDENTIFIER NULL,
    IsDeleted             BIT             NOT NULL CONSTRAINT DF_Users_IsDeleted DEFAULT (0)
);
GO

CREATE UNIQUE INDEX UX_Users_Email ON Users (Email) WHERE IsDeleted = 0;
GO
