-- Enriches demo data so all 4 dashboard chart widgets actually render something meaningful instead
-- of near-empty/single-point charts, without touching any application code -- purely a data gap:
--   - Revenue Trend: every existing booking was created on the exact same single day (2026-09-16),
--     so a "trend" line had nothing to trend across. Adds 7 new bookings spread across Sep 15-22.
--   - Destination Performance: every existing booking had DestinationId = NULL (a strict INNER JOIN
--     in sp_Dashboard_GetDestinationPerformance, so NULL rows never appear at all). Sets a real
--     destination on 3 existing bookings and every new one below.
--   - Team Performance (sp_Dashboard_GetSalesPerformance): needs a Lead assigned to a staff user
--     (AssignedToUserId) that also has a Booking linked via Bookings.LeadId -- none of the 3 leads
--     already assigned to staff had a linked booking at all. Adds 3 new leads, one per existing demo
--     TravelAgent (Seed016), each linked to 2-3 new bookings.
-- Guarded with NOT EXISTS checks so this is safe to re-run and never creates duplicates.

DECLARE @AnanyaId UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Email = 'agent@oneclickyatra.dev');
DECLARE @RohanId UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Email = 'rohan.agent@oneclickyatra.dev');
DECLARE @KavyaId UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Email = 'kavya.agent@oneclickyatra.dev');

DECLARE @BaliId UNIQUEIDENTIFIER = (SELECT Id FROM Destinations WHERE Slug = 'bali-indonesia');
DECLARE @GoaId UNIQUEIDENTIFIER = (SELECT Id FROM Destinations WHERE Slug = 'goa-india');
DECLARE @PhuketId UNIQUEIDENTIFIER = (SELECT Id FROM Destinations WHERE Slug = 'phuket-thailand');
DECLARE @DubaiId UNIQUEIDENTIFIER = (SELECT Id FROM Destinations WHERE Slug = 'dubai-uae');
DECLARE @JaipurId UNIQUEIDENTIFIER = (SELECT Id FROM Destinations WHERE Slug = 'jaipur-india');

-- Backfill DestinationId on 3 existing, already-seeded bookings (never touches Status/CreatedAt/
-- LeadId -- purely adds the missing destination tag so Destination Performance has more than one
-- day's worth of bookings feeding it).
UPDATE Bookings SET DestinationId = @DubaiId WHERE BookingNumber = 'BK-20260916-4BAB17' AND DestinationId IS NULL;
UPDATE Bookings SET DestinationId = @BaliId WHERE BookingNumber = 'BK-20260916-90E005' AND DestinationId IS NULL;
UPDATE Bookings SET DestinationId = @PhuketId WHERE BookingNumber = 'BK-20260916-B2F146' AND DestinationId IS NULL;

-- 3 new customers, one per new lead below.
IF NOT EXISTS (SELECT 1 FROM Customers WHERE Email = 'vikram.nair@example.com')
BEGIN
    DECLARE @VikramCustomerId UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Customers (Id, FullName, Email, Phone, CreatedAt, IsDeleted)
    VALUES (@VikramCustomerId, 'Vikram Nair', 'vikram.nair@example.com', '+919845000001', SYSUTCDATETIME(), 0);
END

IF NOT EXISTS (SELECT 1 FROM Customers WHERE Email = 'sneha.kapoor@example.com')
BEGIN
    DECLARE @SnehaCustomerId UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Customers (Id, FullName, Email, Phone, CreatedAt, IsDeleted)
    VALUES (@SnehaCustomerId, 'Sneha Kapoor', 'sneha.kapoor@example.com', '+919845000002', SYSUTCDATETIME(), 0);
END

IF NOT EXISTS (SELECT 1 FROM Customers WHERE Email = 'arjun.malhotra@example.com')
BEGIN
    DECLARE @ArjunCustomerId UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Customers (Id, FullName, Email, Phone, CreatedAt, IsDeleted)
    VALUES (@ArjunCustomerId, 'Arjun Malhotra', 'arjun.malhotra@example.com', '+919845000003', SYSUTCDATETIME(), 0);
END

DECLARE @VikramId UNIQUEIDENTIFIER = (SELECT Id FROM Customers WHERE Email = 'vikram.nair@example.com');
DECLARE @SnehaId UNIQUEIDENTIFIER = (SELECT Id FROM Customers WHERE Email = 'sneha.kapoor@example.com');
DECLARE @ArjunId UNIQUEIDENTIFIER = (SELECT Id FROM Customers WHERE Email = 'arjun.malhotra@example.com');

-- 3 new leads, one assigned to each existing demo TravelAgent -- this is what
-- sp_Dashboard_GetSalesPerformance's join needs to attribute a staff member's leads/bookings/revenue.
IF NOT EXISTS (SELECT 1 FROM Leads WHERE CustomerName = 'Vikram Nair')
BEGIN
    INSERT INTO Leads (Id, CustomerName, Mobile, Email, DestinationId, TravelDate, Budget, Source, AssignedToUserId, LeadScore, Status, CustomerId, CreatedAt, IsDeleted)
    VALUES (NEWID(), 'Vikram Nair', '+919845000001', 'vikram.nair@example.com', @BaliId, '2026-11-10', 90000, 'Website', @AnanyaId, 80, 'Confirmed', @VikramId, '2026-09-15T09:00:00', 0);
END

IF NOT EXISTS (SELECT 1 FROM Leads WHERE CustomerName = 'Sneha Kapoor')
BEGIN
    INSERT INTO Leads (Id, CustomerName, Mobile, Email, DestinationId, TravelDate, Budget, Source, AssignedToUserId, LeadScore, Status, CustomerId, CreatedAt, IsDeleted)
    VALUES (NEWID(), 'Sneha Kapoor', '+919845000002', 'sneha.kapoor@example.com', @GoaId, '2026-11-20', 65000, 'Referral', @RohanId, 75, 'Confirmed', @SnehaId, '2026-09-16T09:00:00', 0);
END

IF NOT EXISTS (SELECT 1 FROM Leads WHERE CustomerName = 'Arjun Malhotra')
BEGIN
    INSERT INTO Leads (Id, CustomerName, Mobile, Email, DestinationId, TravelDate, Budget, Source, AssignedToUserId, LeadScore, Status, CustomerId, CreatedAt, IsDeleted)
    VALUES (NEWID(), 'Arjun Malhotra', '+919845000003', 'arjun.malhotra@example.com', @PhuketId, '2026-12-05', 110000, 'Instagram', @KavyaId, 85, 'Confirmed', @ArjunId, '2026-09-17T09:00:00', 0);
END

DECLARE @VikramLeadId UNIQUEIDENTIFIER = (SELECT Id FROM Leads WHERE CustomerName = 'Vikram Nair');
DECLARE @SnehaLeadId UNIQUEIDENTIFIER = (SELECT Id FROM Leads WHERE CustomerName = 'Sneha Kapoor');
DECLARE @ArjunLeadId UNIQUEIDENTIFIER = (SELECT Id FROM Leads WHERE CustomerName = 'Arjun Malhotra');

-- 7 new bookings spread across Sep 15-22 (today), each revenue-qualifying (Confirmed/InProgress/
-- Completed) so Revenue Trend gets a real multi-day line, each destination-tagged so Destination
-- Performance gets a 5-destination spread, and each linked to one of the 3 leads above so Team
-- Performance shows 3 distinct staff bars instead of one empty one.
IF NOT EXISTS (SELECT 1 FROM Bookings WHERE BookingNumber = 'BK-20260915-DEMO01')
BEGIN
    INSERT INTO Bookings (Id, BookingNumber, LeadId, CustomerId, DestinationId, TravelDate, NumberOfAdults, NumberOfChildren, TotalAmount, AmountPaid, Status, CreatedAt, IsDeleted)
    VALUES (NEWID(), 'BK-20260915-DEMO01', @VikramLeadId, @VikramId, @BaliId, '2026-11-10', 2, 0, 42000, 42000, 'Confirmed', '2026-09-15T10:00:00', 0);
END

IF NOT EXISTS (SELECT 1 FROM Bookings WHERE BookingNumber = 'BK-20260917-DEMO02')
BEGIN
    INSERT INTO Bookings (Id, BookingNumber, LeadId, CustomerId, DestinationId, TravelDate, NumberOfAdults, NumberOfChildren, TotalAmount, AmountPaid, Status, CreatedAt, IsDeleted)
    VALUES (NEWID(), 'BK-20260917-DEMO02', @SnehaLeadId, @SnehaId, @GoaId, '2026-11-20', 2, 1, 35000, 35000, 'Completed', '2026-09-17T10:00:00', 0);
END

IF NOT EXISTS (SELECT 1 FROM Bookings WHERE BookingNumber = 'BK-20260918-DEMO03')
BEGIN
    INSERT INTO Bookings (Id, BookingNumber, LeadId, CustomerId, DestinationId, TravelDate, NumberOfAdults, NumberOfChildren, TotalAmount, AmountPaid, Status, CreatedAt, IsDeleted)
    VALUES (NEWID(), 'BK-20260918-DEMO03', @ArjunLeadId, @ArjunId, @PhuketId, '2026-12-05', 2, 0, 58000, 20000, 'Confirmed', '2026-09-18T10:00:00', 0);
END

IF NOT EXISTS (SELECT 1 FROM Bookings WHERE BookingNumber = 'BK-20260919-DEMO04')
BEGIN
    INSERT INTO Bookings (Id, BookingNumber, LeadId, CustomerId, DestinationId, TravelDate, NumberOfAdults, NumberOfChildren, TotalAmount, AmountPaid, Status, CreatedAt, IsDeleted)
    VALUES (NEWID(), 'BK-20260919-DEMO04', @VikramLeadId, @VikramId, @DubaiId, '2026-12-15', 4, 2, 75000, 40000, 'InProgress', '2026-09-19T10:00:00', 0);
END

IF NOT EXISTS (SELECT 1 FROM Bookings WHERE BookingNumber = 'BK-20260920-DEMO05')
BEGIN
    INSERT INTO Bookings (Id, BookingNumber, LeadId, CustomerId, DestinationId, TravelDate, NumberOfAdults, NumberOfChildren, TotalAmount, AmountPaid, Status, CreatedAt, IsDeleted)
    VALUES (NEWID(), 'BK-20260920-DEMO05', @SnehaLeadId, @SnehaId, @JaipurId, '2026-10-25', 2, 0, 26000, 26000, 'Confirmed', '2026-09-20T10:00:00', 0);
END

IF NOT EXISTS (SELECT 1 FROM Bookings WHERE BookingNumber = 'BK-20260921-DEMO06')
BEGIN
    INSERT INTO Bookings (Id, BookingNumber, LeadId, CustomerId, DestinationId, TravelDate, NumberOfAdults, NumberOfChildren, TotalAmount, AmountPaid, Status, CreatedAt, IsDeleted)
    VALUES (NEWID(), 'BK-20260921-DEMO06', @ArjunLeadId, @ArjunId, @BaliId, '2026-11-01', 2, 0, 48000, 48000, 'Completed', '2026-09-21T10:00:00', 0);
END

IF NOT EXISTS (SELECT 1 FROM Bookings WHERE BookingNumber = 'BK-20260922-DEMO07')
BEGIN
    INSERT INTO Bookings (Id, BookingNumber, LeadId, CustomerId, DestinationId, TravelDate, NumberOfAdults, NumberOfChildren, TotalAmount, AmountPaid, Status, CreatedAt, IsDeleted)
    VALUES (NEWID(), 'BK-20260922-DEMO07', @VikramLeadId, @VikramId, @GoaId, '2026-11-28', 3, 0, 33000, 15000, 'Confirmed', '2026-09-22T10:00:00', 0);
END
GO
