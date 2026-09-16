CREATE TABLE Destinations
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Destinations PRIMARY KEY,
    CountryId        UNIQUEIDENTIFIER NOT NULL,
    CityId           UNIQUEIDENTIFIER NULL,
    Name             NVARCHAR(150)    NOT NULL,
    Slug             NVARCHAR(160)    NOT NULL,
    ShortDescription NVARCHAR(300)    NULL,
    Description      NVARCHAR(MAX)    NULL,
    HeroImageUrl     NVARCHAR(500)    NULL,
    IsFeatured       BIT              NOT NULL CONSTRAINT DF_Destinations_IsFeatured DEFAULT (0),
    IsPublished      BIT              NOT NULL CONSTRAINT DF_Destinations_IsPublished DEFAULT (0),
    CreatedAt        DATETIME2        NOT NULL CONSTRAINT DF_Destinations_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy        UNIQUEIDENTIFIER NULL,
    UpdatedAt        DATETIME2        NULL,
    UpdatedBy        UNIQUEIDENTIFIER NULL,
    IsDeleted        BIT              NOT NULL CONSTRAINT DF_Destinations_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Destinations_Countries FOREIGN KEY (CountryId) REFERENCES Countries (Id),
    CONSTRAINT FK_Destinations_Cities FOREIGN KEY (CityId) REFERENCES Cities (Id)
);
GO

CREATE UNIQUE INDEX UX_Destinations_Slug ON Destinations (Slug) WHERE IsDeleted = 0;
GO
CREATE INDEX IX_Destinations_CountryId ON Destinations (CountryId);
GO
CREATE INDEX IX_Destinations_IsPublished_IsFeatured ON Destinations (IsPublished, IsFeatured);
GO
