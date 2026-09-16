CREATE TABLE Enquiries
(
    Id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Enquiries PRIMARY KEY,
    FullName      NVARCHAR(150)    NOT NULL,
    Email         NVARCHAR(256)    NOT NULL,
    Phone         NVARCHAR(30)     NOT NULL,
    DestinationId UNIQUEIDENTIFIER NULL,
    TravelDate    DATE             NULL,
    Message       NVARCHAR(1000)   NULL,
    Status        NVARCHAR(20)     NOT NULL CONSTRAINT DF_Enquiries_Status DEFAULT ('New'),
    CreatedAt     DATETIME2        NOT NULL CONSTRAINT DF_Enquiries_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy     UNIQUEIDENTIFIER NULL,
    UpdatedAt     DATETIME2        NULL,
    UpdatedBy     UNIQUEIDENTIFIER NULL,
    IsDeleted     BIT              NOT NULL CONSTRAINT DF_Enquiries_IsDeleted DEFAULT (0),
    CONSTRAINT FK_Enquiries_Destinations FOREIGN KEY (DestinationId) REFERENCES Destinations (Id),
    CONSTRAINT CK_Enquiries_Status CHECK (Status IN ('New', 'Contacted', 'Converted', 'Closed'))
);
GO

CREATE INDEX IX_Enquiries_Status ON Enquiries (Status);
GO
CREATE INDEX IX_Enquiries_DestinationId ON Enquiries (DestinationId);
GO
