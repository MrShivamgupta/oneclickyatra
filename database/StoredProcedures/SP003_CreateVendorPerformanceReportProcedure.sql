-- The 12th SRS report (Vendor-Performance), added once both the Vendor and Reports domains had
-- landed (it was deliberately deferred by the Reports agent since the Vendor schema didn't exist
-- yet at the time). Aggregates VendorPerformance ratings and VendorPayments in the date range per
-- vendor. Each side is aggregated in its own subquery before joining so a vendor with multiple
-- rating rows AND multiple payment rows in range doesn't fan-out and double-count TotalPaid.
CREATE OR ALTER PROCEDURE sp_Report_VendorPerformance
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RangeEnd DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATETIME2));

    SELECT
        v.Name AS VendorName,
        v.VendorType,
        ISNULL(perf.RatingsCount, 0) AS RatingsCount,
        perf.AverageRating,
        ISNULL(pay.PaymentsCount, 0) AS PaymentsCount,
        ISNULL(pay.TotalPaid, 0) AS TotalPaid
    FROM Vendors v
    LEFT JOIN (
        SELECT VendorId, COUNT(*) AS RatingsCount, AVG(CAST(Rating AS DECIMAL(3,2))) AS AverageRating
        FROM VendorPerformance
        WHERE IsDeleted = 0 AND CreatedAt >= @FromDate AND CreatedAt < @RangeEnd
        GROUP BY VendorId
    ) perf ON perf.VendorId = v.Id
    LEFT JOIN (
        SELECT VendorId, COUNT(*) AS PaymentsCount, SUM(CASE WHEN Status = 'Paid' THEN Amount ELSE 0 END) AS TotalPaid
        FROM VendorPayments
        WHERE IsDeleted = 0 AND CreatedAt >= @FromDate AND CreatedAt < @RangeEnd
        GROUP BY VendorId
    ) pay ON pay.VendorId = v.Id
    WHERE v.IsDeleted = 0 AND (ISNULL(perf.RatingsCount, 0) > 0 OR ISNULL(pay.PaymentsCount, 0) > 0)
    ORDER BY perf.AverageRating DESC;
END
GO
