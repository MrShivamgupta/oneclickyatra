-- Dashboard aggregation procedures (§8: dashboard aggregations/analytics must be stored
-- procedures, not inline Dapper SQL). Applied via `dotnet run -- storedprocs`, tracked in
-- __StoredProcedureVersions like every other versioned script — never edit a released one,
-- add a new SPNNN file with CREATE OR ALTER instead.
--
-- @FromDate/@ToDate scope the "activity in a period" numbers (leads created, bookings created,
-- revenue, enquiries, conversion rate). A few figures are always a current snapshot regardless of
-- the selected range (ActiveQuotations, PendingPayments, UpcomingDepartures) because re-scoping
-- "how many departures are upcoming" to an arbitrary past date range would not be meaningful.

CREATE OR ALTER PROCEDURE sp_Dashboard_GetKpis
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT
        (SELECT COUNT(*) FROM Leads WHERE IsDeleted = 0 AND CreatedAt >= @FromDate AND CreatedAt < @RangeEnd) AS TotalLeads,
        (SELECT COUNT(*) FROM Bookings WHERE IsDeleted = 0 AND CreatedAt >= @FromDate AND CreatedAt < @RangeEnd) AS TotalBookings,
        (SELECT COUNT(*) FROM Bookings WHERE IsDeleted = 0 AND Status = 'Confirmed' AND CreatedAt >= @FromDate AND CreatedAt < @RangeEnd) AS ConfirmedBookings,
        (SELECT COUNT(*) FROM Bookings WHERE IsDeleted = 0 AND Status = 'Cancelled' AND CreatedAt >= @FromDate AND CreatedAt < @RangeEnd) AS Cancellations,
        (SELECT COUNT(*) FROM Quotations WHERE IsDeleted = 0 AND Status IN ('Draft', 'Sent')) AS ActiveQuotations,
        (SELECT ISNULL(SUM(TotalAmount), 0) FROM Bookings
            WHERE IsDeleted = 0 AND Status IN ('Confirmed', 'InProgress', 'Completed')
              AND CreatedAt >= @FromDate AND CreatedAt < @RangeEnd) AS PeriodRevenue,
        (SELECT ISNULL(SUM(TotalAmount - AmountPaid), 0) FROM Bookings
            WHERE IsDeleted = 0 AND Status NOT IN ('Cancelled', 'Refunded')) AS PendingPayments,
        (SELECT COUNT(*) FROM Bookings
            WHERE IsDeleted = 0 AND Status IN ('Confirmed', 'InProgress')
              AND TravelDate BETWEEN CAST(SYSUTCDATETIME() AS DATE) AND DATEADD(DAY, 30, CAST(SYSUTCDATETIME() AS DATE))) AS UpcomingDepartures,
        (SELECT COUNT(*) FROM Enquiries WHERE IsDeleted = 0 AND Status = 'New' AND CreatedAt >= @FromDate AND CreatedAt < @RangeEnd) AS OpenEnquiries,
        (SELECT CASE WHEN COUNT(*) = 0 THEN 0
                     ELSE CAST(SUM(CASE WHEN Status = 'Confirmed' THEN 1 ELSE 0 END) AS DECIMAL(10, 2)) * 100.0 / COUNT(*)
                END
         FROM Leads WHERE IsDeleted = 0 AND CreatedAt >= @FromDate AND CreatedAt < @RangeEnd) AS ConversionRate;
END
GO

CREATE OR ALTER PROCEDURE sp_Dashboard_GetRevenueTrend
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT CAST(CreatedAt AS DATE) AS TrendDate, SUM(TotalAmount) AS Revenue
    FROM Bookings
    WHERE IsDeleted = 0 AND Status IN ('Confirmed', 'InProgress', 'Completed')
      AND CreatedAt >= @FromDate AND CreatedAt < @RangeEnd
    GROUP BY CAST(CreatedAt AS DATE)
    ORDER BY TrendDate;
END
GO

CREATE OR ALTER PROCEDURE sp_Dashboard_GetLeadFunnel
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT Status, COUNT(*) AS LeadCount
    FROM Leads
    WHERE IsDeleted = 0 AND CreatedAt >= @FromDate AND CreatedAt < @RangeEnd
    GROUP BY Status;
END
GO

CREATE OR ALTER PROCEDURE sp_Dashboard_GetDestinationPerformance
    @FromDate DATE,
    @ToDate DATE,
    @Top INT = 8
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT TOP (@Top) d.Name AS DestinationName, COUNT(b.Id) AS BookingCount, SUM(b.TotalAmount) AS Revenue
    FROM Bookings b
    INNER JOIN Destinations d ON d.Id = b.DestinationId
    WHERE b.IsDeleted = 0 AND b.CreatedAt >= @FromDate AND b.CreatedAt < @RangeEnd
    GROUP BY d.Name
    ORDER BY Revenue DESC;
END
GO

-- "Team Performance" from the SRS is implemented here rather than as a separate scalar KPI:
-- leads handled + bookings converted + revenue attributed, per staff member.
CREATE OR ALTER PROCEDURE sp_Dashboard_GetSalesPerformance
    @FromDate DATE,
    @ToDate DATE,
    @Top INT = 8
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT TOP (@Top)
        u.FullName AS StaffName,
        COUNT(DISTINCT l.Id) AS LeadsHandled,
        COUNT(DISTINCT CASE WHEN b.Status IN ('Confirmed', 'InProgress', 'Completed') THEN b.Id END) AS BookingsCount,
        ISNULL(SUM(CASE WHEN b.Status IN ('Confirmed', 'InProgress', 'Completed') THEN b.TotalAmount END), 0) AS Revenue
    FROM Users u
    INNER JOIN Leads l ON l.AssignedToUserId = u.Id AND l.IsDeleted = 0 AND l.CreatedAt >= @FromDate AND l.CreatedAt < @RangeEnd
    LEFT JOIN Bookings b ON b.LeadId = l.Id AND b.IsDeleted = 0
    GROUP BY u.FullName
    ORDER BY Revenue DESC;
END
GO
