CREATE TABLE Pages
(
    Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Pages PRIMARY KEY,
    Slug        NVARCHAR(160)    NOT NULL,
    Title       NVARCHAR(200)    NOT NULL,
    Content     NVARCHAR(MAX)    NULL,
    IsPublished BIT              NOT NULL CONSTRAINT DF_Pages_IsPublished DEFAULT (1),
    CreatedAt   DATETIME2        NOT NULL CONSTRAINT DF_Pages_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedAt   DATETIME2        NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_Pages_IsDeleted DEFAULT (0)
);
GO

CREATE UNIQUE INDEX UX_Pages_Slug ON Pages (Slug) WHERE IsDeleted = 0;
GO
