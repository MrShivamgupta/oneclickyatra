CREATE TABLE Permissions
(
    Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Permissions PRIMARY KEY,
    [Key]       NVARCHAR(100)    NOT NULL,
    Description NVARCHAR(400)    NULL,
    CreatedAt   DATETIME2        NOT NULL CONSTRAINT DF_Permissions_CreatedAt DEFAULT (SYSUTCDATETIME()),
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_Permissions_IsDeleted DEFAULT (0)
);
GO

CREATE UNIQUE INDEX UX_Permissions_Key ON Permissions ([Key]) WHERE IsDeleted = 0;
GO
