CREATE TABLE Customers
(
    Id        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
    FullName  NVARCHAR(150)    NOT NULL,
    Email     NVARCHAR(256)    NULL,
    Phone     NVARCHAR(30)     NOT NULL,
    UserId    UNIQUEIDENTIFIER NULL,
    CreatedAt DATETIME2        NOT NULL CONSTRAINT DF_Customers_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2        NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT              NOT NULL CONSTRAINT DF_Customers_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Customers_Users FOREIGN KEY (UserId) REFERENCES Users (Id)
);
GO

CREATE INDEX IX_Customers_Phone ON Customers (Phone);
GO
CREATE INDEX IX_Customers_UserId ON Customers (UserId);
GO

CREATE TABLE Leads
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Leads PRIMARY KEY,
    CustomerName     NVARCHAR(150)    NOT NULL,
    Mobile           NVARCHAR(30)     NOT NULL,
    Email            NVARCHAR(256)    NULL,
    DestinationId    UNIQUEIDENTIFIER NULL,
    TravelDate       DATE             NULL,
    Budget           DECIMAL(12, 2)   NULL,
    Source           NVARCHAR(50)     NULL,
    AssignedToUserId UNIQUEIDENTIFIER NULL,
    LeadScore        INT              NOT NULL CONSTRAINT DF_Leads_LeadScore DEFAULT (0),
    Status           NVARCHAR(20)     NOT NULL CONSTRAINT DF_Leads_Status DEFAULT ('New'),
    CustomerId       UNIQUEIDENTIFIER NULL,
    CreatedAt        DATETIME2        NOT NULL CONSTRAINT DF_Leads_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy        UNIQUEIDENTIFIER NULL,
    UpdatedAt        DATETIME2        NULL,
    UpdatedBy        UNIQUEIDENTIFIER NULL,
    IsDeleted        BIT              NOT NULL CONSTRAINT DF_Leads_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Leads_Destinations FOREIGN KEY (DestinationId) REFERENCES Destinations (Id),
    CONSTRAINT FK_Leads_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users (Id),
    CONSTRAINT FK_Leads_Customers FOREIGN KEY (CustomerId) REFERENCES Customers (Id),
    CONSTRAINT CK_Leads_LeadScore CHECK (LeadScore BETWEEN 0 AND 100),
    CONSTRAINT CK_Leads_Status CHECK (Status IN ('New', 'Contacted', 'QuotationSent', 'Negotiation', 'Confirmed', 'Lost'))
);
GO

CREATE INDEX IX_Leads_Status ON Leads (Status);
GO
CREATE INDEX IX_Leads_AssignedToUserId ON Leads (AssignedToUserId);
GO
CREATE INDEX IX_Leads_DestinationId ON Leads (DestinationId);
GO

CREATE TABLE FollowUps
(
    Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FollowUps PRIMARY KEY,
    LeadId      UNIQUEIDENTIFIER NOT NULL,
    ScheduledAt DATETIME2        NOT NULL,
    Type        NVARCHAR(30)     NOT NULL CONSTRAINT DF_FollowUps_Type DEFAULT ('Call'),
    Notes       NVARCHAR(500)    NULL,
    Status      NVARCHAR(20)     NOT NULL CONSTRAINT DF_FollowUps_Status DEFAULT ('Pending'),
    CompletedAt DATETIME2        NULL,
    CreatedAt   DATETIME2        NOT NULL CONSTRAINT DF_FollowUps_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedAt   DATETIME2        NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_FollowUps_IsDeleted DEFAULT (0),
    CONSTRAINT FK_FollowUps_Leads FOREIGN KEY (LeadId) REFERENCES Leads (Id),
    CONSTRAINT CK_FollowUps_Type CHECK (Type IN ('Call', 'WhatsApp', 'Email', 'Visit')),
    CONSTRAINT CK_FollowUps_Status CHECK (Status IN ('Pending', 'Completed', 'Cancelled'))
);
GO

CREATE INDEX IX_FollowUps_LeadId ON FollowUps (LeadId);
GO
CREATE INDEX IX_FollowUps_ScheduledAt ON FollowUps (ScheduledAt);
GO
