-- Reports domain procedures (§8: cross-table aggregations/analytics must be stored procedures,
-- not inline Dapper SQL). Applied via `dotnet run -- storedprocs`, tracked in
-- __StoredProcedureVersions like every other versioned script — never edit a released one, add a
-- new SPNNN file with CREATE OR ALTER instead.
--
-- 11 of the 12 SRS-named reports are implemented here (Vendor-Performance is intentionally
-- omitted — the Vendor domain/schema is being built by another agent concurrently and will be
-- wired up by hand once it lands). @FromDate/@ToDate use the same
-- "DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));" inclusive
-- date-range pattern as SP001, applied consistently everywhere a report scopes by CreatedAt.
--
-- Two reports are point-in-time snapshots with no date parameters at all (Outstanding,
-- ActiveBookings) — re-scoping "what's outstanding right now" or "what's currently active" to an
-- arbitrary past range would not be meaningful, exactly like Dashboard's PendingPayments/
-- UpcomingDepartures KPIs.
--
-- AgentCommission and Profitability are named for what the SRS asked for, but this schema has no
-- commission-rate configuration table and no cost/expense tracking table, so they return
-- Revenue/BookingsCount only — never a fabricated commission amount or profit margin. See
-- ReportModels.cs and the summary from the agent that added this file for the caveat.

-- 1) Sales: one row per booking created in range.
CREATE OR ALTER PROCEDURE sp_Report_Sales
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT
        b.BookingNumber,
        c.FullName AS CustomerName,
        d.Name AS DestinationName,
        b.TotalAmount,
        b.Status,
        b.CreatedAt,
        u.FullName AS AssignedAgentName
    FROM Bookings b
    INNER JOIN Customers c ON c.Id = b.CustomerId
    LEFT JOIN Destinations d ON d.Id = b.DestinationId
    LEFT JOIN Leads l ON l.Id = b.LeadId
    LEFT JOIN Users u ON u.Id = l.AssignedToUserId
    WHERE b.IsDeleted = 0 AND b.CreatedAt >= @FromDate AND b.CreatedAt < @RangeEnd
    ORDER BY b.CreatedAt DESC;
END
GO

-- 2) Revenue trend, bucketed by day/week/month.
CREATE OR ALTER PROCEDURE sp_Report_Revenue
    @FromDate DATE,
    @ToDate DATE,
    @GroupBy NVARCHAR(10) = 'day'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT
        CASE @GroupBy
            WHEN 'week' THEN DATEADD(DAY, -(DATEPART(WEEKDAY, b.CreatedAt) - 1), CAST(b.CreatedAt AS DATE))
            WHEN 'month' THEN DATEFROMPARTS(YEAR(b.CreatedAt), MONTH(b.CreatedAt), 1)
            ELSE CAST(b.CreatedAt AS DATE)
        END AS PeriodStart,
        SUM(b.TotalAmount) AS Revenue
    FROM Bookings b
    WHERE b.IsDeleted = 0 AND b.Status IN ('Confirmed', 'InProgress', 'Completed')
      AND b.CreatedAt >= @FromDate AND b.CreatedAt < @RangeEnd
    GROUP BY CASE @GroupBy
            WHEN 'week' THEN DATEADD(DAY, -(DATEPART(WEEKDAY, b.CreatedAt) - 1), CAST(b.CreatedAt AS DATE))
            WHEN 'month' THEN DATEFROMPARTS(YEAR(b.CreatedAt), MONTH(b.CreatedAt), 1)
            ELSE CAST(b.CreatedAt AS DATE)
        END
    ORDER BY PeriodStart;
END
GO

-- 3) Cancellation: cancelled bookings in range, with the lost value and reason.
CREATE OR ALTER PROCEDURE sp_Report_Cancellation
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT
        b.BookingNumber,
        c.FullName AS CustomerName,
        b.TotalAmount,
        b.CancellationReason,
        b.CreatedAt
    FROM Bookings b
    INNER JOIN Customers c ON c.Id = b.CustomerId
    WHERE b.IsDeleted = 0 AND b.Status = 'Cancelled'
      AND b.CreatedAt >= @FromDate AND b.CreatedAt < @RangeEnd
    ORDER BY b.CreatedAt DESC;
END
GO

-- 4) Agent commission: no AgentCommissions/rate-config table exists yet, so this returns
-- StaffName/BookingsCount/Revenue only — never a fabricated commission percentage or amount.
CREATE OR ALTER PROCEDURE sp_Report_AgentCommission
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT
        u.FullName AS StaffName,
        COUNT(DISTINCT b.Id) AS BookingsCount,
        ISNULL(SUM(b.TotalAmount), 0) AS Revenue
    FROM Users u
    INNER JOIN Leads l ON l.AssignedToUserId = u.Id AND l.IsDeleted = 0
    INNER JOIN Bookings b ON b.LeadId = l.Id AND b.IsDeleted = 0
        AND b.Status IN ('Confirmed', 'InProgress', 'Completed')
        AND b.CreatedAt >= @FromDate AND b.CreatedAt < @RangeEnd
    GROUP BY u.FullName
    ORDER BY Revenue DESC;
END
GO

-- 5) Lead conversion: leads and confirmed-leads per source, for a client-computed conversion rate.
CREATE OR ALTER PROCEDURE sp_Report_LeadConversion
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT
        ISNULL(l.Source, 'Unknown') AS Source,
        COUNT(*) AS TotalLeads,
        SUM(CASE WHEN l.Status = 'Confirmed' THEN 1 ELSE 0 END) AS ConvertedLeads
    FROM Leads l
    WHERE l.IsDeleted = 0 AND l.CreatedAt >= @FromDate AND l.CreatedAt < @RangeEnd
    GROUP BY l.Source
    ORDER BY TotalLeads DESC;
END
GO

-- 6) Destination sales: same logic as sp_Dashboard_GetDestinationPerformance, reused across a
-- real date range (no TOP limit) for exporting rather than a dashboard widget.
CREATE OR ALTER PROCEDURE sp_Report_DestinationSales
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT
        d.Name AS DestinationName,
        COUNT(b.Id) AS BookingCount,
        SUM(b.TotalAmount) AS Revenue
    FROM Bookings b
    INNER JOIN Destinations d ON d.Id = b.DestinationId
    WHERE b.IsDeleted = 0 AND b.CreatedAt >= @FromDate AND b.CreatedAt < @RangeEnd
    GROUP BY d.Name
    ORDER BY Revenue DESC;
END
GO

-- 7) Collection: paid payments in range. PaymentId is aliased to PaymentReference rather than a
-- bare "Id" column, since this is a report row rather than an entity payload.
CREATE OR ALTER PROCEDURE sp_Report_Collection
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT
        p.Id AS PaymentReference,
        b.BookingNumber,
        c.FullName AS CustomerName,
        p.Amount,
        p.CreatedAt
    FROM Payments p
    INNER JOIN Bookings b ON b.Id = p.BookingId
    INNER JOIN Customers c ON c.Id = b.CustomerId
    WHERE p.IsDeleted = 0 AND p.Status = 'Paid'
      AND p.CreatedAt >= @FromDate AND p.CreatedAt < @RangeEnd
    ORDER BY p.CreatedAt DESC;
END
GO

-- 8) Outstanding: point-in-time snapshot (no date range), mirrors the dashboard's PendingPayments
-- KPI but as a row-per-booking export.
CREATE OR ALTER PROCEDURE sp_Report_Outstanding
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        b.BookingNumber,
        c.FullName AS CustomerName,
        b.TotalAmount,
        b.AmountPaid,
        (b.TotalAmount - b.AmountPaid) AS OutstandingAmount
    FROM Bookings b
    INNER JOIN Customers c ON c.Id = b.CustomerId
    WHERE b.IsDeleted = 0 AND b.TotalAmount > b.AmountPaid
      AND b.Status NOT IN ('Cancelled', 'Refunded')
    ORDER BY OutstandingAmount DESC;
END
GO

-- 9) Profitability: revenue-only per destination — this schema has no cost/expense tracking
-- table, so a true profit margin cannot be computed. The column is named Revenue, never Profit.
CREATE OR ALTER PROCEDURE sp_Report_Profitability
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT
        d.Name AS DestinationName,
        SUM(b.TotalAmount) AS Revenue
    FROM Bookings b
    INNER JOIN Destinations d ON d.Id = b.DestinationId
    WHERE b.IsDeleted = 0 AND b.CreatedAt >= @FromDate AND b.CreatedAt < @RangeEnd
    GROUP BY d.Name
    ORDER BY Revenue DESC;
END
GO

-- 10) Active bookings: point-in-time snapshot (no date range) of bookings still in flight.
CREATE OR ALTER PROCEDURE sp_Report_ActiveBookings
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        b.BookingNumber,
        c.FullName AS CustomerName,
        b.TravelDate,
        b.Status
    FROM Bookings b
    INNER JOIN Customers c ON c.Id = b.CustomerId
    WHERE b.IsDeleted = 0 AND b.Status IN ('Confirmed', 'InProgress', 'PendingPayment')
    ORDER BY b.TravelDate ASC;
END
GO

-- 11) Employee productivity: leads assigned, follow-ups completed and quotations sent per staff
-- member in range. Any role can appear here, not just TravelAgent. Staff with zero activity
-- across all three metrics in the range are omitted rather than listing every user account.
CREATE OR ALTER PROCEDURE sp_Report_EmployeeProductivity
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT
        u.FullName AS StaffName,
        (SELECT COUNT(*) FROM Leads l
            WHERE l.IsDeleted = 0 AND l.AssignedToUserId = u.Id
              AND l.CreatedAt >= @FromDate AND l.CreatedAt < @RangeEnd) AS LeadsAssigned,
        (SELECT COUNT(*) FROM FollowUps f
            INNER JOIN Leads l2 ON l2.Id = f.LeadId
            WHERE f.IsDeleted = 0 AND l2.AssignedToUserId = u.Id AND f.Status = 'Completed'
              AND f.CompletedAt >= @FromDate AND f.CompletedAt < @RangeEnd) AS FollowUpsCompleted,
        (SELECT COUNT(*) FROM Quotations q
            WHERE q.IsDeleted = 0 AND q.CreatedBy = u.Id AND q.Status <> 'Draft'
              AND q.CreatedAt >= @FromDate AND q.CreatedAt < @RangeEnd) AS QuotationsSent
    FROM Users u
    WHERE u.IsDeleted = 0
      AND (
        EXISTS (SELECT 1 FROM Leads l WHERE l.AssignedToUserId = u.Id AND l.IsDeleted = 0
                  AND l.CreatedAt >= @FromDate AND l.CreatedAt < @RangeEnd)
        OR EXISTS (SELECT 1 FROM FollowUps f INNER JOIN Leads l2 ON l2.Id = f.LeadId
                  WHERE l2.AssignedToUserId = u.Id AND f.IsDeleted = 0 AND f.Status = 'Completed'
                    AND f.CompletedAt >= @FromDate AND f.CompletedAt < @RangeEnd)
        OR EXISTS (SELECT 1 FROM Quotations q WHERE q.CreatedBy = u.Id AND q.IsDeleted = 0
                  AND q.Status <> 'Draft' AND q.CreatedAt >= @FromDate AND q.CreatedAt < @RangeEnd)
      )
    ORDER BY u.FullName;
END
GO

-- 12) Monthly growth: new leads, new bookings and revenue per calendar month in range.
CREATE OR ALTER PROCEDURE sp_Report_MonthlyGrowth
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT
        COALESCE(lm.MonthStart, bm.MonthStart) AS MonthStart,
        ISNULL(lm.NewLeads, 0) AS NewLeads,
        ISNULL(bm.NewBookings, 0) AS NewBookings,
        ISNULL(bm.Revenue, 0) AS Revenue
    FROM (
        SELECT DATEFROMPARTS(YEAR(CreatedAt), MONTH(CreatedAt), 1) AS MonthStart, COUNT(*) AS NewLeads
        FROM Leads
        WHERE IsDeleted = 0 AND CreatedAt >= @FromDate AND CreatedAt < @RangeEnd
        GROUP BY DATEFROMPARTS(YEAR(CreatedAt), MONTH(CreatedAt), 1)
    ) lm
    FULL OUTER JOIN (
        SELECT DATEFROMPARTS(YEAR(CreatedAt), MONTH(CreatedAt), 1) AS MonthStart,
               COUNT(*) AS NewBookings,
               ISNULL(SUM(CASE WHEN Status IN ('Confirmed', 'InProgress', 'Completed') THEN TotalAmount ELSE 0 END), 0) AS Revenue
        FROM Bookings
        WHERE IsDeleted = 0 AND CreatedAt >= @FromDate AND CreatedAt < @RangeEnd
        GROUP BY DATEFROMPARTS(YEAR(CreatedAt), MONTH(CreatedAt), 1)
    ) bm ON bm.MonthStart = lm.MonthStart
    ORDER BY MonthStart;
END
GO
