CREATE TABLE Categories
(
    Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
    Name        NVARCHAR(150)    NOT NULL,
    Slug        NVARCHAR(160)    NOT NULL,
    Description NVARCHAR(500)    NULL,
    CreatedAt   DATETIME2        NOT NULL CONSTRAINT DF_Categories_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedAt   DATETIME2        NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_Categories_IsDeleted DEFAULT (0)
);
GO

CREATE UNIQUE INDEX UX_Categories_Slug ON Categories (Slug) WHERE IsDeleted = 0;
GO

CREATE TABLE Seasons
(
    Id         UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Seasons PRIMARY KEY,
    Name       NVARCHAR(100)    NOT NULL,
    StartMonth INT              NOT NULL,
    EndMonth   INT              NOT NULL,
    CreatedAt  DATETIME2        NOT NULL CONSTRAINT DF_Seasons_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy  UNIQUEIDENTIFIER NULL,
    UpdatedAt  DATETIME2        NULL,
    UpdatedBy  UNIQUEIDENTIFIER NULL,
    IsDeleted  BIT              NOT NULL CONSTRAINT DF_Seasons_IsDeleted DEFAULT (0),
    CONSTRAINT CK_Seasons_Months CHECK (StartMonth BETWEEN 1 AND 12 AND EndMonth BETWEEN 1 AND 12)
);
GO

CREATE UNIQUE INDEX UX_Seasons_Name ON Seasons (Name) WHERE IsDeleted = 0;
GO
