CREATE TABLE Packages
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Packages PRIMARY KEY,
    DestinationId    UNIQUEIDENTIFIER NOT NULL,
    CategoryId       UNIQUEIDENTIFIER NULL,
    SeasonId         UNIQUEIDENTIFIER NULL,
    Title            NVARCHAR(200)    NOT NULL,
    Slug             NVARCHAR(220)    NOT NULL,
    DurationDays     INT              NOT NULL,
    DurationNights   INT              NOT NULL,
    ShortDescription NVARCHAR(300)    NULL,
    Description      NVARCHAR(MAX)    NULL,
    HeroImageUrl     NVARCHAR(500)    NULL,
    Status           NVARCHAR(20)     NOT NULL CONSTRAINT DF_Packages_Status DEFAULT ('Draft'),
    CreatedAt        DATETIME2        NOT NULL CONSTRAINT DF_Packages_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy        UNIQUEIDENTIFIER NULL,
    UpdatedAt        DATETIME2        NULL,
    UpdatedBy        UNIQUEIDENTIFIER NULL,
    IsDeleted        BIT              NOT NULL CONSTRAINT DF_Packages_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Packages_Destinations FOREIGN KEY (DestinationId) REFERENCES Destinations (Id),
    CONSTRAINT FK_Packages_Categories FOREIGN KEY (CategoryId) REFERENCES Categories (Id),
    CONSTRAINT FK_Packages_Seasons FOREIGN KEY (SeasonId) REFERENCES Seasons (Id),
    CONSTRAINT CK_Packages_Status CHECK (Status IN ('Draft', 'Published')),
    CONSTRAINT CK_Packages_Duration CHECK (DurationDays > 0 AND DurationNights >= 0)
);
GO

CREATE UNIQUE INDEX UX_Packages_Slug ON Packages (Slug) WHERE IsDeleted = 0;
GO
CREATE INDEX IX_Packages_DestinationId ON Packages (DestinationId);
GO
CREATE INDEX IX_Packages_Status ON Packages (Status);
GO
