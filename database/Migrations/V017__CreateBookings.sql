CREATE TABLE Bookings
(
    Id                 UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Bookings PRIMARY KEY,
    BookingNumber      NVARCHAR(30)     NOT NULL,
    LeadId             UNIQUEIDENTIFIER NULL,
    CustomerId         UNIQUEIDENTIFIER NOT NULL,
    QuotationId        UNIQUEIDENTIFIER NULL,
    QuotationOptionId  UNIQUEIDENTIFIER NULL,
    PackageId          UNIQUEIDENTIFIER NULL,
    DestinationId      UNIQUEIDENTIFIER NULL,
    TravelDate         DATE             NULL,
    ReturnDate         DATE             NULL,
    NumberOfAdults     INT              NOT NULL CONSTRAINT DF_Bookings_NumberOfAdults DEFAULT (1),
    NumberOfChildren   INT              NOT NULL CONSTRAINT DF_Bookings_NumberOfChildren DEFAULT (0),
    TotalAmount        DECIMAL(12, 2)   NOT NULL CONSTRAINT DF_Bookings_TotalAmount DEFAULT (0),
    AmountPaid         DECIMAL(12, 2)   NOT NULL CONSTRAINT DF_Bookings_AmountPaid DEFAULT (0),
    Notes              NVARCHAR(1000)   NULL,
    Status             NVARCHAR(20)     NOT NULL CONSTRAINT DF_Bookings_Status DEFAULT ('Draft'),
    CancellationReason NVARCHAR(500)    NULL,
    CreatedAt          DATETIME2        NOT NULL CONSTRAINT DF_Bookings_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy          UNIQUEIDENTIFIER NULL,
    UpdatedAt          DATETIME2        NULL,
    UpdatedBy          UNIQUEIDENTIFIER NULL,
    IsDeleted          BIT              NOT NULL CONSTRAINT DF_Bookings_IsDeleted DEFAULT (0),
    CONSTRAINT UQ_Bookings_BookingNumber UNIQUE (BookingNumber),
    CONSTRAINT FK_Bookings_Leads FOREIGN KEY (LeadId) REFERENCES Leads (Id),
    CONSTRAINT FK_Bookings_Customers FOREIGN KEY (CustomerId) REFERENCES Customers (Id),
    CONSTRAINT FK_Bookings_Quotations FOREIGN KEY (QuotationId) REFERENCES Quotations (Id),
    CONSTRAINT FK_Bookings_QuotationOptions FOREIGN KEY (QuotationOptionId) REFERENCES QuotationOptions (Id),
    CONSTRAINT FK_Bookings_Packages FOREIGN KEY (PackageId) REFERENCES Packages (Id),
    CONSTRAINT FK_Bookings_Destinations FOREIGN KEY (DestinationId) REFERENCES Destinations (Id),
    CONSTRAINT CK_Bookings_Status CHECK (Status IN (
        'Draft', 'Quoted', 'PendingPayment', 'Confirmed', 'InProgress', 'Completed',
        'Cancelled', 'RefundPending', 'Refunded'
    ))
);
GO

CREATE INDEX IX_Bookings_CustomerId ON Bookings (CustomerId);
GO
CREATE INDEX IX_Bookings_Status ON Bookings (Status);
GO
CREATE INDEX IX_Bookings_QuotationId ON Bookings (QuotationId);
GO

CREATE TABLE BookingPassengers
(
    Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_BookingPassengers PRIMARY KEY,
    BookingId       UNIQUEIDENTIFIER NOT NULL,
    FullName        NVARCHAR(150)    NOT NULL,
    Age             INT              NULL,
    Gender          NVARCHAR(20)     NULL,
    IdProofType     NVARCHAR(50)     NULL,
    IdProofNumber   NVARCHAR(50)     NULL,
    IsLeadPassenger BIT              NOT NULL CONSTRAINT DF_BookingPassengers_IsLeadPassenger DEFAULT (0),
    CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_BookingPassengers_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy       UNIQUEIDENTIFIER NULL,
    UpdatedAt       DATETIME2        NULL,
    UpdatedBy       UNIQUEIDENTIFIER NULL,
    IsDeleted       BIT              NOT NULL CONSTRAINT DF_BookingPassengers_IsDeleted DEFAULT (0),
    CONSTRAINT FK_BookingPassengers_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings (Id)
);
GO

CREATE INDEX IX_BookingPassengers_BookingId ON BookingPassengers (BookingId);
GO

CREATE TABLE BookingAddOns
(
    Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_BookingAddOns PRIMARY KEY,
    BookingId   UNIQUEIDENTIFIER NOT NULL,
    Name        NVARCHAR(150)    NOT NULL,
    Description NVARCHAR(500)    NULL,
    Price       DECIMAL(12, 2)   NOT NULL,
    Quantity    INT              NOT NULL CONSTRAINT DF_BookingAddOns_Quantity DEFAULT (1),
    CreatedAt   DATETIME2        NOT NULL CONSTRAINT DF_BookingAddOns_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedAt   DATETIME2        NULL,
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    IsDeleted   BIT              NOT NULL CONSTRAINT DF_BookingAddOns_IsDeleted DEFAULT (0),
    CONSTRAINT FK_BookingAddOns_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings (Id)
);
GO

CREATE INDEX IX_BookingAddOns_BookingId ON BookingAddOns (BookingId);
GO

-- Append-only audit trail — every status transition writes exactly one row here and it is
-- never updated or deleted, so it intentionally has no soft-delete/UpdatedAt/UpdatedBy columns.
CREATE TABLE BookingStatusHistory
(
    Id         UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_BookingStatusHistory PRIMARY KEY,
    BookingId  UNIQUEIDENTIFIER NOT NULL,
    OldStatus  NVARCHAR(20)     NULL,
    NewStatus  NVARCHAR(20)     NOT NULL,
    ChangedBy  UNIQUEIDENTIFIER NULL,
    ChangedAt  DATETIME2        NOT NULL CONSTRAINT DF_BookingStatusHistory_ChangedAt DEFAULT (SYSUTCDATETIME()),
    Reason     NVARCHAR(500)    NULL,
    TrackingId NVARCHAR(64)     NULL,
    CONSTRAINT FK_BookingStatusHistory_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings (Id)
);
GO

CREATE INDEX IX_BookingStatusHistory_BookingId ON BookingStatusHistory (BookingId);
GO
