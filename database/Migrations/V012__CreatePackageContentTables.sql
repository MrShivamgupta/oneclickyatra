CREATE TABLE PackageItineraries
(
    Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PackageItineraries PRIMARY KEY,
    PackageId   UNIQUEIDENTIFIER NOT NULL,
    DayNumber   INT              NOT NULL,
    Title       NVARCHAR(200)    NOT NULL,
    Description NVARCHAR(MAX)    NULL,
    CONSTRAINT FK_PackageItineraries_Packages FOREIGN KEY (PackageId) REFERENCES Packages (Id) ON DELETE CASCADE,
    CONSTRAINT CK_PackageItineraries_DayNumber CHECK (DayNumber > 0)
);
GO

CREATE UNIQUE INDEX UX_PackageItineraries_PackageId_DayNumber ON PackageItineraries (PackageId, DayNumber);
GO

CREATE TABLE PackageInclusions
(
    Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PackageInclusions PRIMARY KEY,
    PackageId   UNIQUEIDENTIFIER NOT NULL,
    Description NVARCHAR(400)    NOT NULL,
    IsIncluded  BIT              NOT NULL CONSTRAINT DF_PackageInclusions_IsIncluded DEFAULT (1),
    SortOrder   INT              NOT NULL CONSTRAINT DF_PackageInclusions_SortOrder DEFAULT (0),
    CONSTRAINT FK_PackageInclusions_Packages FOREIGN KEY (PackageId) REFERENCES Packages (Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_PackageInclusions_PackageId ON PackageInclusions (PackageId);
GO

CREATE TABLE PackagePricing
(
    Id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PackagePricing PRIMARY KEY,
    PackageId      UNIQUEIDENTIFIER NOT NULL,
    TierName       NVARCHAR(100)    NOT NULL,
    HotelCategory  NVARCHAR(50)     NULL,
    PricePerPerson DECIMAL(10, 2)   NOT NULL,
    ChildPrice     DECIMAL(10, 2)   NULL,
    ValidFrom      DATE             NULL,
    ValidTo        DATE             NULL,
    Currency       NVARCHAR(3)      NOT NULL CONSTRAINT DF_PackagePricing_Currency DEFAULT ('INR'),
    CONSTRAINT FK_PackagePricing_Packages FOREIGN KEY (PackageId) REFERENCES Packages (Id) ON DELETE CASCADE,
    CONSTRAINT CK_PackagePricing_Price CHECK (PricePerPerson >= 0)
);
GO

CREATE INDEX IX_PackagePricing_PackageId ON PackagePricing (PackageId);
GO

CREATE TABLE PackageInventory
(
    Id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PackageInventory PRIMARY KEY,
    PackageId     UNIQUEIDENTIFIER NOT NULL,
    DepartureDate DATE             NOT NULL,
    TotalSeats    INT              NOT NULL,
    BookedSeats   INT              NOT NULL CONSTRAINT DF_PackageInventory_BookedSeats DEFAULT (0),
    Status        NVARCHAR(20)     NOT NULL CONSTRAINT DF_PackageInventory_Status DEFAULT ('Open'),
    CONSTRAINT FK_PackageInventory_Packages FOREIGN KEY (PackageId) REFERENCES Packages (Id) ON DELETE CASCADE,
    CONSTRAINT CK_PackageInventory_Seats CHECK (TotalSeats >= 0 AND BookedSeats >= 0 AND BookedSeats <= TotalSeats),
    CONSTRAINT CK_PackageInventory_Status CHECK (Status IN ('Open', 'Closed', 'SoldOut'))
);
GO

CREATE UNIQUE INDEX UX_PackageInventory_PackageId_DepartureDate ON PackageInventory (PackageId, DepartureDate);
GO

CREATE TABLE PackageMedia
(
    Id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PackageMedia PRIMARY KEY,
    PackageId     UNIQUEIDENTIFIER NOT NULL,
    MediaUrl      NVARCHAR(500)    NOT NULL,
    MediaType     NVARCHAR(20)     NOT NULL CONSTRAINT DF_PackageMedia_MediaType DEFAULT ('Image'),
    SortOrder     INT              NOT NULL CONSTRAINT DF_PackageMedia_SortOrder DEFAULT (0),
    IsCoverImage  BIT              NOT NULL CONSTRAINT DF_PackageMedia_IsCoverImage DEFAULT (0),
    CONSTRAINT FK_PackageMedia_Packages FOREIGN KEY (PackageId) REFERENCES Packages (Id) ON DELETE CASCADE,
    CONSTRAINT CK_PackageMedia_MediaType CHECK (MediaType IN ('Image', 'Video'))
);
GO

CREATE INDEX IX_PackageMedia_PackageId ON PackageMedia (PackageId);
GO
