CREATE TABLE Payments
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Payments PRIMARY KEY,
    BookingId        UNIQUEIDENTIFIER NOT NULL,
    Amount           DECIMAL(12, 2)   NOT NULL,
    Currency         NVARCHAR(3)      NOT NULL CONSTRAINT DF_Payments_Currency DEFAULT ('INR'),
    Status           NVARCHAR(20)     NOT NULL CONSTRAINT DF_Payments_Status DEFAULT ('Created'),
    GatewayProvider  NVARCHAR(30)     NOT NULL CONSTRAINT DF_Payments_GatewayProvider DEFAULT ('Razorpay'),
    GatewayOrderId   NVARCHAR(100)    NULL,
    GatewayPaymentId NVARCHAR(100)    NULL,
    Notes            NVARCHAR(500)    NULL,
    CreatedAt        DATETIME2        NOT NULL CONSTRAINT DF_Payments_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy        UNIQUEIDENTIFIER NULL,
    UpdatedAt        DATETIME2        NULL,
    UpdatedBy        UNIQUEIDENTIFIER NULL,
    IsDeleted        BIT              NOT NULL CONSTRAINT DF_Payments_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Payments_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings (Id),
    CONSTRAINT CK_Payments_Status CHECK (Status IN ('Created', 'Pending', 'Paid', 'Failed', 'Refunded', 'PartiallyRefunded'))
);
GO

CREATE INDEX IX_Payments_BookingId ON Payments (BookingId);
GO
CREATE UNIQUE INDEX UX_Payments_GatewayOrderId ON Payments (GatewayOrderId) WHERE GatewayOrderId IS NOT NULL;
GO

-- Append-only ledger of every gateway event for a payment (order created, webhook received,
-- verified, captured, failed) — kept for audit/idempotency, never updated or soft-deleted.
CREATE TABLE PaymentTransactions
(
    Id             UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PaymentTransactions PRIMARY KEY,
    PaymentId      UNIQUEIDENTIFIER NOT NULL,
    EventType      NVARCHAR(30)     NOT NULL,
    GatewayEventId NVARCHAR(150)    NULL,
    RawPayload     NVARCHAR(MAX)    NULL,
    CreatedAt      DATETIME2        NOT NULL CONSTRAINT DF_PaymentTransactions_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_PaymentTransactions_Payments FOREIGN KEY (PaymentId) REFERENCES Payments (Id)
);
GO

CREATE INDEX IX_PaymentTransactions_PaymentId ON PaymentTransactions (PaymentId);
GO
-- Enforces webhook idempotency at the database level, not just in application code.
CREATE UNIQUE INDEX UX_PaymentTransactions_GatewayEventId ON PaymentTransactions (GatewayEventId) WHERE GatewayEventId IS NOT NULL;
GO

CREATE TABLE Refunds
(
    Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Refunds PRIMARY KEY,
    PaymentId       UNIQUEIDENTIFIER NOT NULL,
    BookingId       UNIQUEIDENTIFIER NOT NULL,
    Amount          DECIMAL(12, 2)   NOT NULL,
    Reason          NVARCHAR(500)    NULL,
    Status          NVARCHAR(20)     NOT NULL CONSTRAINT DF_Refunds_Status DEFAULT ('Requested'),
    GatewayRefundId NVARCHAR(100)    NULL,
    RequestedBy     UNIQUEIDENTIFIER NULL,
    CreatedAt       DATETIME2        NOT NULL CONSTRAINT DF_Refunds_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy       UNIQUEIDENTIFIER NULL,
    UpdatedAt       DATETIME2        NULL,
    UpdatedBy       UNIQUEIDENTIFIER NULL,
    IsDeleted       BIT              NOT NULL CONSTRAINT DF_Refunds_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Refunds_Payments FOREIGN KEY (PaymentId) REFERENCES Payments (Id),
    CONSTRAINT FK_Refunds_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings (Id),
    CONSTRAINT CK_Refunds_Status CHECK (Status IN ('Requested', 'Processing', 'Refunded', 'Rejected'))
);
GO

CREATE INDEX IX_Refunds_PaymentId ON Refunds (PaymentId);
GO
CREATE INDEX IX_Refunds_BookingId ON Refunds (BookingId);
GO

CREATE TABLE Invoices
(
    Id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Invoices PRIMARY KEY,
    BookingId     UNIQUEIDENTIFIER NOT NULL,
    InvoiceNumber NVARCHAR(30)     NOT NULL,
    Amount        DECIMAL(12, 2)   NOT NULL,
    IssuedAt      DATETIME2        NOT NULL CONSTRAINT DF_Invoices_IssuedAt DEFAULT (SYSUTCDATETIME()),
    CreatedAt     DATETIME2        NOT NULL CONSTRAINT DF_Invoices_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy     UNIQUEIDENTIFIER NULL,
    UpdatedAt     DATETIME2        NULL,
    UpdatedBy     UNIQUEIDENTIFIER NULL,
    IsDeleted     BIT              NOT NULL CONSTRAINT DF_Invoices_IsDeleted DEFAULT (0),
    CONSTRAINT UQ_Invoices_InvoiceNumber UNIQUE (InvoiceNumber),
    CONSTRAINT FK_Invoices_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings (Id)
);
GO

CREATE INDEX IX_Invoices_BookingId ON Invoices (BookingId);
GO
