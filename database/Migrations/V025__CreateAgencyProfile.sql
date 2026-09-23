-- A deliberate singleton: there is exactly one agency running this CRM, so this table only ever
-- holds one non-deleted row (created by Seed021_AgencyProfile.sql). Read/replaced as a whole record
-- by AgencyProfileRepository -- no per-field partial updates, matching how small this record is.
-- Feeds the PDF header (name + logo) on invoices/quotations/vouchers, which previously hardcoded
-- "One Click Yatra" as a string literal in each PDF service.

CREATE TABLE AgencyProfile
(
    Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AgencyProfile PRIMARY KEY,
    Name         NVARCHAR(200)    NOT NULL,
    LogoUrl      NVARCHAR(500)    NULL,
    Address      NVARCHAR(500)    NULL,
    GstNumber    NVARCHAR(50)     NULL,
    Currency     NVARCHAR(10)     NOT NULL CONSTRAINT DF_AgencyProfile_Currency DEFAULT ('INR'),
    SupportEmail NVARCHAR(256)    NULL,
    SupportPhone NVARCHAR(30)     NULL,
    CreatedAt    DATETIME2        NOT NULL CONSTRAINT DF_AgencyProfile_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy    UNIQUEIDENTIFIER NULL,
    UpdatedAt    DATETIME2        NULL,
    UpdatedBy    UNIQUEIDENTIFIER NULL,
    IsDeleted    BIT              NOT NULL CONSTRAINT DF_AgencyProfile_IsDeleted DEFAULT (0)
);
GO
