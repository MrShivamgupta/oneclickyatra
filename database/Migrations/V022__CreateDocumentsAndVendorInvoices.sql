CREATE TABLE CustomerDocuments
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerDocuments PRIMARY KEY,
    BookingId        UNIQUEIDENTIFIER NOT NULL,
    UploadedByUserId UNIQUEIDENTIFIER NOT NULL,
    FileName         NVARCHAR(260)    NOT NULL,
    ContentType      NVARCHAR(150)    NOT NULL,
    FileSizeBytes    BIGINT           NOT NULL,
    StoragePath      NVARCHAR(500)    NOT NULL,
    CreatedAt        DATETIME2        NOT NULL CONSTRAINT DF_CustomerDocuments_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy        UNIQUEIDENTIFIER NULL,
    UpdatedAt        DATETIME2        NULL,
    UpdatedBy        UNIQUEIDENTIFIER NULL,
    IsDeleted        BIT              NOT NULL CONSTRAINT DF_CustomerDocuments_IsDeleted DEFAULT (0),
    CONSTRAINT FK_CustomerDocuments_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings (Id),
    CONSTRAINT FK_CustomerDocuments_Users FOREIGN KEY (UploadedByUserId) REFERENCES Users (Id)
);
GO

CREATE INDEX IX_CustomerDocuments_BookingId ON CustomerDocuments (BookingId);
GO

CREATE TABLE VendorInvoices
(
    Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VendorInvoices PRIMARY KEY,
    VendorId         UNIQUEIDENTIFIER NOT NULL,
    BookingId        UNIQUEIDENTIFIER NULL,
    UploadedByUserId UNIQUEIDENTIFIER NOT NULL,
    FileName         NVARCHAR(260)    NOT NULL,
    ContentType      NVARCHAR(150)    NOT NULL,
    FileSizeBytes    BIGINT           NOT NULL,
    StoragePath      NVARCHAR(500)    NOT NULL,
    Amount           DECIMAL(12, 2)   NOT NULL,
    Notes            NVARCHAR(500)    NULL,
    Status           NVARCHAR(20)     NOT NULL CONSTRAINT DF_VendorInvoices_Status DEFAULT ('Pending'),
    CreatedAt        DATETIME2        NOT NULL CONSTRAINT DF_VendorInvoices_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy        UNIQUEIDENTIFIER NULL,
    UpdatedAt        DATETIME2        NULL,
    UpdatedBy        UNIQUEIDENTIFIER NULL,
    IsDeleted        BIT              NOT NULL CONSTRAINT DF_VendorInvoices_IsDeleted DEFAULT (0),
    CONSTRAINT FK_VendorInvoices_Vendors FOREIGN KEY (VendorId) REFERENCES Vendors (Id),
    CONSTRAINT FK_VendorInvoices_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings (Id),
    CONSTRAINT FK_VendorInvoices_Users FOREIGN KEY (UploadedByUserId) REFERENCES Users (Id),
    CONSTRAINT CK_VendorInvoices_Status CHECK (Status IN ('Pending', 'Reviewed'))
);
GO

CREATE INDEX IX_VendorInvoices_VendorId ON VendorInvoices (VendorId);
GO
