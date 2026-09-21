CREATE TABLE Vendors
(
    Id         UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Vendors PRIMARY KEY,
    Name       NVARCHAR(200)    NOT NULL,
    VendorType NVARCHAR(20)     NOT NULL,
    Email      NVARCHAR(256)    NOT NULL,
    Phone      NVARCHAR(30)     NOT NULL,
    Address    NVARCHAR(500)    NULL,
    City       NVARCHAR(100)    NULL,
    Country    NVARCHAR(100)    NULL,
    UserId     UNIQUEIDENTIFIER NULL,
    Rating     DECIMAL(3, 2)    NULL,
    IsActive   BIT              NOT NULL CONSTRAINT DF_Vendors_IsActive DEFAULT (1),
    CreatedAt  DATETIME2        NOT NULL CONSTRAINT DF_Vendors_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy  UNIQUEIDENTIFIER NULL,
    UpdatedAt  DATETIME2        NULL,
    UpdatedBy  UNIQUEIDENTIFIER NULL,
    IsDeleted  BIT              NOT NULL CONSTRAINT DF_Vendors_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Vendors_Users FOREIGN KEY (UserId) REFERENCES Users (Id),
    CONSTRAINT CK_Vendors_VendorType CHECK (VendorType IN ('Hotel', 'Airline', 'Transport', 'DMC', 'ActivityProvider'))
);
GO

CREATE INDEX IX_Vendors_VendorType ON Vendors (VendorType);
GO
CREATE INDEX IX_Vendors_UserId ON Vendors (UserId);
GO
CREATE INDEX IX_Vendors_IsActive ON Vendors (IsActive);
GO

CREATE TABLE VendorContacts
(
    Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VendorContacts PRIMARY KEY,
    VendorId    UNIQUEIDENTIFIER NOT NULL,
    ContactName NVARCHAR(150)    NOT NULL,
    Designation NVARCHAR(100)    NULL,
    Phone       NVARCHAR(30)     NOT NULL,
    Email       NVARCHAR(256)    NULL,
    IsPrimary   BIT              NOT NULL CONSTRAINT DF_VendorContacts_IsPrimary DEFAULT (0),
    CreatedAt   DATETIME2        NOT NULL CONSTRAINT DF_VendorContacts_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedAt   DATETIME2        NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_VendorContacts_IsDeleted DEFAULT (0),
    CONSTRAINT FK_VendorContacts_Vendors FOREIGN KEY (VendorId) REFERENCES Vendors (Id)
);
GO

CREATE INDEX IX_VendorContacts_VendorId ON VendorContacts (VendorId);
GO

CREATE TABLE VendorRates
(
    Id                 UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VendorRates PRIMARY KEY,
    VendorId           UNIQUEIDENTIFIER NOT NULL,
    DestinationId      UNIQUEIDENTIFIER NULL,
    ServiceDescription NVARCHAR(300)    NOT NULL,
    RateAmount         DECIMAL(12, 2)   NOT NULL,
    Currency           NVARCHAR(3)      NOT NULL CONSTRAINT DF_VendorRates_Currency DEFAULT ('INR'),
    ValidFrom          DATE             NULL,
    ValidTo            DATE             NULL,
    CreatedAt          DATETIME2        NOT NULL CONSTRAINT DF_VendorRates_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy          UNIQUEIDENTIFIER NULL,
    UpdatedAt          DATETIME2        NULL,
    UpdatedBy          UNIQUEIDENTIFIER NULL,
    IsDeleted          BIT              NOT NULL CONSTRAINT DF_VendorRates_IsDeleted DEFAULT (0),
    CONSTRAINT FK_VendorRates_Vendors FOREIGN KEY (VendorId) REFERENCES Vendors (Id),
    CONSTRAINT FK_VendorRates_Destinations FOREIGN KEY (DestinationId) REFERENCES Destinations (Id)
);
GO

CREATE INDEX IX_VendorRates_VendorId ON VendorRates (VendorId);
GO
CREATE INDEX IX_VendorRates_DestinationId ON VendorRates (DestinationId);
GO

CREATE TABLE VendorPayments
(
    Id        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VendorPayments PRIMARY KEY,
    VendorId  UNIQUEIDENTIFIER NOT NULL,
    BookingId UNIQUEIDENTIFIER NULL,
    Amount    DECIMAL(12, 2)   NOT NULL,
    Status    NVARCHAR(20)     NOT NULL CONSTRAINT DF_VendorPayments_Status DEFAULT ('Pending'),
    Notes     NVARCHAR(500)    NULL,
    PaidAt    DATETIME2        NULL,
    CreatedAt DATETIME2        NOT NULL CONSTRAINT DF_VendorPayments_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedAt DATETIME2        NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    IsDeleted BIT              NOT NULL CONSTRAINT DF_VendorPayments_IsDeleted DEFAULT (0),
    CONSTRAINT FK_VendorPayments_Vendors FOREIGN KEY (VendorId) REFERENCES Vendors (Id),
    CONSTRAINT FK_VendorPayments_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings (Id),
    CONSTRAINT CK_VendorPayments_Status CHECK (Status IN ('Pending', 'Paid', 'Overdue'))
);
GO

CREATE INDEX IX_VendorPayments_VendorId ON VendorPayments (VendorId);
GO
CREATE INDEX IX_VendorPayments_BookingId ON VendorPayments (BookingId);
GO
CREATE INDEX IX_VendorPayments_Status ON VendorPayments (Status);
GO

-- Internal, staff-recorded performance notes about a vendor (distinct from anything
-- vendor-submitted). Unlike BookingStatusHistory/PaymentTransactions this is NOT an
-- append-only ledger — a mis-entered note can be corrected/soft-deleted — so it keeps the
-- full standard audit column set.
CREATE TABLE VendorPerformance
(
    Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VendorPerformance PRIMARY KEY,
    VendorId    UNIQUEIDENTIFIER NOT NULL,
    BookingId   UNIQUEIDENTIFIER NULL,
    Rating      INT              NOT NULL,
    Notes       NVARCHAR(500)    NULL,
    RecordedBy  UNIQUEIDENTIFIER NULL,
    CreatedAt   DATETIME2        NOT NULL CONSTRAINT DF_VendorPerformance_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedAt   DATETIME2        NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_VendorPerformance_IsDeleted DEFAULT (0),
    CONSTRAINT FK_VendorPerformance_Vendors FOREIGN KEY (VendorId) REFERENCES Vendors (Id),
    CONSTRAINT FK_VendorPerformance_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings (Id),
    CONSTRAINT CK_VendorPerformance_Rating CHECK (Rating BETWEEN 1 AND 5)
);
GO

CREATE INDEX IX_VendorPerformance_VendorId ON VendorPerformance (VendorId);
GO
