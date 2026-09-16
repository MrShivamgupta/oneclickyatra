CREATE TABLE Countries
(
    Id        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Countries PRIMARY KEY,
    Name      NVARCHAR(150)    NOT NULL,
    IsoCode   NVARCHAR(3)      NOT NULL,
    CreatedAt DATETIME2        NOT NULL CONSTRAINT DF_Countries_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2        NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT              NOT NULL CONSTRAINT DF_Countries_IsDeleted DEFAULT (0)
);
GO

CREATE UNIQUE INDEX UX_Countries_IsoCode ON Countries (IsoCode) WHERE IsDeleted = 0;
GO
CREATE UNIQUE INDEX UX_Countries_Name ON Countries (Name) WHERE IsDeleted = 0;
GO

CREATE TABLE Cities
(
    Id        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Cities PRIMARY KEY,
    CountryId UNIQUEIDENTIFIER NOT NULL,
    Name      NVARCHAR(150)    NOT NULL,
    CreatedAt DATETIME2        NOT NULL CONSTRAINT DF_Cities_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2        NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT              NOT NULL CONSTRAINT DF_Cities_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Cities_Countries FOREIGN KEY (CountryId) REFERENCES Countries (Id)
);
GO

CREATE INDEX IX_Cities_CountryId ON Cities (CountryId);
GO
CREATE UNIQUE INDEX UX_Cities_CountryId_Name ON Cities (CountryId, Name) WHERE IsDeleted = 0;
GO
