CREATE TABLE Quotations
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Quotations PRIMARY KEY,
    QuotationNumber  NVARCHAR(30)     NOT NULL,
    LeadId           UNIQUEIDENTIFIER NOT NULL,
    CustomerId       UNIQUEIDENTIFIER NULL,
    Title            NVARCHAR(200)    NOT NULL,
    Status           NVARCHAR(20)     NOT NULL CONSTRAINT DF_Quotations_Status DEFAULT ('Draft'),
    ValidUntil       DATE             NULL,
    Notes            NVARCHAR(1000)   NULL,
    PublicTokenHash  NVARCHAR(64)     NOT NULL,
    SelectedOptionId UNIQUEIDENTIFIER NULL,
    ApprovedAt       DATETIME2        NULL,
    ApprovedByName   NVARCHAR(150)    NULL,
    RejectionReason  NVARCHAR(500)    NULL,
    CreatedAt        DATETIME2        NOT NULL CONSTRAINT DF_Quotations_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy        UNIQUEIDENTIFIER NULL,
    UpdatedAt        DATETIME2        NULL,
    UpdatedBy        UNIQUEIDENTIFIER NULL,
    IsDeleted        BIT              NOT NULL CONSTRAINT DF_Quotations_IsDeleted DEFAULT (0),
    CONSTRAINT UQ_Quotations_QuotationNumber UNIQUE (QuotationNumber),
    CONSTRAINT UQ_Quotations_PublicTokenHash UNIQUE (PublicTokenHash),
    CONSTRAINT FK_Quotations_Leads FOREIGN KEY (LeadId) REFERENCES Leads (Id),
    CONSTRAINT FK_Quotations_Customers FOREIGN KEY (CustomerId) REFERENCES Customers (Id),
    CONSTRAINT CK_Quotations_Status CHECK (Status IN ('Draft', 'Sent', 'Approved', 'Rejected', 'Expired', 'Converted'))
);
GO

CREATE INDEX IX_Quotations_LeadId ON Quotations (LeadId);
GO
CREATE INDEX IX_Quotations_Status ON Quotations (Status);
GO

CREATE TABLE QuotationOptions
(
    Id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_QuotationOptions PRIMARY KEY,
    QuotationId    UNIQUEIDENTIFIER NOT NULL,
    PackageId      UNIQUEIDENTIFIER NULL,
    OptionName     NVARCHAR(150)    NOT NULL,
    DestinationId  UNIQUEIDENTIFIER NULL,
    DurationDays   INT              NULL,
    DurationNights INT              NULL,
    HotelCategory  NVARCHAR(50)     NULL,
    NumberOfPeople INT              NOT NULL CONSTRAINT DF_QuotationOptions_NumberOfPeople DEFAULT (1),
    PricePerPerson DECIMAL(12, 2)   NOT NULL,
    TotalPrice     DECIMAL(12, 2)   NOT NULL,
    IsRecommended  BIT              NOT NULL CONSTRAINT DF_QuotationOptions_IsRecommended DEFAULT (0),
    SortOrder      INT              NOT NULL CONSTRAINT DF_QuotationOptions_SortOrder DEFAULT (0),
    CreatedAt      DATETIME2        NOT NULL CONSTRAINT DF_QuotationOptions_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy      UNIQUEIDENTIFIER NULL,
    UpdatedAt      DATETIME2        NULL,
    UpdatedBy      UNIQUEIDENTIFIER NULL,
    IsDeleted      BIT              NOT NULL CONSTRAINT DF_QuotationOptions_IsDeleted DEFAULT (0),
    CONSTRAINT FK_QuotationOptions_Quotations FOREIGN KEY (QuotationId) REFERENCES Quotations (Id),
    CONSTRAINT FK_QuotationOptions_Packages FOREIGN KEY (PackageId) REFERENCES Packages (Id),
    CONSTRAINT FK_QuotationOptions_Destinations FOREIGN KEY (DestinationId) REFERENCES Destinations (Id)
);
GO

CREATE INDEX IX_QuotationOptions_QuotationId ON QuotationOptions (QuotationId);
GO

ALTER TABLE Quotations
    ADD CONSTRAINT FK_Quotations_SelectedOption FOREIGN KEY (SelectedOptionId) REFERENCES QuotationOptions (Id);
GO

CREATE TABLE QuotationItems
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_QuotationItems PRIMARY KEY,
    QuotationOptionId UNIQUEIDENTIFIER NOT NULL,
    Description      NVARCHAR(200)    NOT NULL,
    Category         NVARCHAR(50)     NULL,
    Amount           DECIMAL(12, 2)   NOT NULL,
    SortOrder        INT              NOT NULL CONSTRAINT DF_QuotationItems_SortOrder DEFAULT (0),
    CreatedAt        DATETIME2        NOT NULL CONSTRAINT DF_QuotationItems_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy        UNIQUEIDENTIFIER NULL,
    UpdatedAt        DATETIME2        NULL,
    UpdatedBy        UNIQUEIDENTIFIER NULL,
    IsDeleted        BIT              NOT NULL CONSTRAINT DF_QuotationItems_IsDeleted DEFAULT (0),
    CONSTRAINT FK_QuotationItems_Options FOREIGN KEY (QuotationOptionId) REFERENCES QuotationOptions (Id)
);
GO

CREATE INDEX IX_QuotationItems_QuotationOptionId ON QuotationItems (QuotationOptionId);
GO

CREATE TABLE QuotationApprovals
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_QuotationApprovals PRIMARY KEY,
    QuotationId      UNIQUEIDENTIFIER NOT NULL,
    SelectedOptionId UNIQUEIDENTIFIER NULL,
    Decision         NVARCHAR(20)     NOT NULL,
    ApprovedByName   NVARCHAR(150)    NULL,
    Comments         NVARCHAR(500)    NULL,
    IpAddress        NVARCHAR(64)     NULL,
    DecidedAt        DATETIME2        NOT NULL CONSTRAINT DF_QuotationApprovals_DecidedAt DEFAULT (SYSUTCDATETIME()),
    CreatedAt        DATETIME2        NOT NULL CONSTRAINT DF_QuotationApprovals_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy        UNIQUEIDENTIFIER NULL,
    UpdatedAt        DATETIME2        NULL,
    UpdatedBy        UNIQUEIDENTIFIER NULL,
    IsDeleted        BIT              NOT NULL CONSTRAINT DF_QuotationApprovals_IsDeleted DEFAULT (0),
    CONSTRAINT FK_QuotationApprovals_Quotations FOREIGN KEY (QuotationId) REFERENCES Quotations (Id),
    CONSTRAINT FK_QuotationApprovals_Options FOREIGN KEY (SelectedOptionId) REFERENCES QuotationOptions (Id),
    CONSTRAINT CK_QuotationApprovals_Decision CHECK (Decision IN ('Approved', 'Rejected'))
);
GO

CREATE INDEX IX_QuotationApprovals_QuotationId ON QuotationApprovals (QuotationId);
GO
