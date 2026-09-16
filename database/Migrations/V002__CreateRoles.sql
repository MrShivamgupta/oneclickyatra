CREATE TABLE Roles
(
    Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    Name        NVARCHAR(100)    NOT NULL,
    Description NVARCHAR(400)    NULL,
    CreatedAt   DATETIME2        NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedAt   DATETIME2        NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_Roles_IsDeleted DEFAULT (0)
);
GO

CREATE UNIQUE INDEX UX_Roles_Name ON Roles (Name) WHERE IsDeleted = 0;
GO
