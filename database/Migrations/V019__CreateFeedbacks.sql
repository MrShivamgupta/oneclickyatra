CREATE TABLE Feedbacks
(
    Id         UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Feedbacks PRIMARY KEY,
    BookingId  UNIQUEIDENTIFIER NOT NULL,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    Rating     INT              NOT NULL,
    Comment    NVARCHAR(1000)   NULL,
    CreatedAt  DATETIME2        NOT NULL CONSTRAINT DF_Feedbacks_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy  UNIQUEIDENTIFIER NULL,
    UpdatedAt  DATETIME2        NULL,
    UpdatedBy  UNIQUEIDENTIFIER NULL,
    IsDeleted  BIT              NOT NULL CONSTRAINT DF_Feedbacks_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Feedbacks_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings (Id),
    CONSTRAINT FK_Feedbacks_Customers FOREIGN KEY (CustomerId) REFERENCES Customers (Id),
    CONSTRAINT CK_Feedbacks_Rating CHECK (Rating BETWEEN 1 AND 5),
    CONSTRAINT UQ_Feedbacks_BookingId UNIQUE (BookingId)
);
GO

CREATE INDEX IX_Feedbacks_CustomerId ON Feedbacks (CustomerId);
GO
