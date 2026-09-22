-- Closes the remaining 3 empty dashboard widgets Seed018 didn't touch, after being asked directly
-- to fill "the rest" too. Each was empty for its own real, checked reason (read
-- DashboardAppFunction.cs / FollowUpRepository.ListTodayAsync before writing this, not guessed):
--   - Today's Follow-Ups needs Status='Pending' AND ScheduledAt = exactly today's date. No FollowUp
--     row satisfied that.
--   - Refund Requests needs a Booking with Status='RefundPending'. None existed.
--   - Upcoming Departures needs Status IN ('Confirmed','InProgress') AND TravelDate within the next
--     30 days. Seed018's new bookings all used TravelDate 1-3 months out, so none actually landed
--     inside the 30-day window it added revenue/destination data for -- a real gap in that seed,
--     fixed here by moving 2 of those same bookings' TravelDate closer rather than adding new rows.

-- 2 pending follow-ups scheduled for today, against 2 of Seed018's new leads.
IF NOT EXISTS (SELECT 1 FROM FollowUps f INNER JOIN Leads l ON l.Id = f.LeadId WHERE l.CustomerName = 'Vikram Nair' AND f.Type = 'Call')
BEGIN
    DECLARE @VikramLeadId2 UNIQUEIDENTIFIER = (SELECT Id FROM Leads WHERE CustomerName = 'Vikram Nair');
    INSERT INTO FollowUps (Id, LeadId, ScheduledAt, Type, Notes, Status, CreatedAt, IsDeleted)
    VALUES (NEWID(), @VikramLeadId2, CAST(SYSUTCDATETIME() AS DATE), 'Call', 'Confirm Bali itinerary details.', 'Pending', SYSUTCDATETIME(), 0);
END

IF NOT EXISTS (SELECT 1 FROM FollowUps f INNER JOIN Leads l ON l.Id = f.LeadId WHERE l.CustomerName = 'Sneha Kapoor' AND f.Type = 'WhatsApp')
BEGIN
    DECLARE @SnehaLeadId2 UNIQUEIDENTIFIER = (SELECT Id FROM Leads WHERE CustomerName = 'Sneha Kapoor');
    INSERT INTO FollowUps (Id, LeadId, ScheduledAt, Type, Notes, Status, CreatedAt, IsDeleted)
    VALUES (NEWID(), @SnehaLeadId2, CAST(SYSUTCDATETIME() AS DATE), 'WhatsApp', 'Share Jaipur hotel options.', 'Pending', SYSUTCDATETIME(), 0);
END

-- A dedicated new booking for Refund Requests, deliberately NOT reusing one of Seed018's bookings:
-- RefundPending is excluded from the Confirmed/InProgress/Completed status list every revenue-
-- counting proc filters on (Revenue Trend, Team Performance), so repurposing one of those bookings
-- would have quietly undone the exact numbers already verified live in the previous pass.
IF NOT EXISTS (SELECT 1 FROM Bookings WHERE BookingNumber = 'BK-20260920-DEMO08')
BEGIN
    DECLARE @SingaporeId UNIQUEIDENTIFIER = (SELECT Id FROM Destinations WHERE Slug = 'singapore');
    DECLARE @PortalTestCustomerId UNIQUEIDENTIFIER = (SELECT Id FROM Customers WHERE Email = 'portaltest@example.com');
    INSERT INTO Bookings (Id, BookingNumber, CustomerId, DestinationId, TravelDate, NumberOfAdults, NumberOfChildren, TotalAmount, AmountPaid, Status, CancellationReason, CreatedAt, IsDeleted)
    VALUES (NEWID(), 'BK-20260920-DEMO08', @PortalTestCustomerId, @SingaporeId, '2026-11-15', 2, 0, 40000, 40000, 'RefundPending', 'Customer requested cancellation due to a schedule conflict.', '2026-09-20T11:00:00', 0);
END

-- Move 2 Confirmed bookings' travel dates from 1-2 months out to inside the 30-day "upcoming
-- departures" window (today = 2026-09-22, window = through 2026-10-22), without touching their
-- amounts/status/destination -- purely a date fix so the widget that already reads this exact
-- Status+TravelDate combination has matching rows.
UPDATE Bookings SET TravelDate = '2026-10-05'
WHERE BookingNumber = 'BK-20260915-DEMO01' AND Status = 'Confirmed';

UPDATE Bookings SET TravelDate = '2026-10-12'
WHERE BookingNumber = 'BK-20260920-DEMO05' AND Status = 'Confirmed';
GO
