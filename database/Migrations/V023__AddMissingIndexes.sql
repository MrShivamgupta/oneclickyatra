-- Two indexes identified by the Phase 7 performance audit: both columns are filtered/joined on
-- directly in a paginated Repository query but had no supporting index.
CREATE INDEX IX_Quotations_CustomerId ON Quotations (CustomerId);
GO

CREATE INDEX IX_Bookings_DestinationId ON Bookings (DestinationId);
GO
