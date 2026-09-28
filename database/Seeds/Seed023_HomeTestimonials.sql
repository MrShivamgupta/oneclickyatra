-- A couple more realistic, well-rated testimonials (in addition to the one already seeded via
-- earlier dashboard demo data) so the new public home-page "What our customers say" section has
-- more than a single card to show. Attached to real Completed demo bookings, matched by booking
-- number (stable) rather than hardcoded GUIDs.
IF NOT EXISTS (SELECT 1 FROM Feedbacks f INNER JOIN Bookings b ON b.Id = f.BookingId WHERE b.BookingNumber = 'BK-20260921-DEMO06')
BEGIN
    INSERT INTO Feedbacks (Id, BookingId, CustomerId, Rating, Comment, CreatedAt)
    SELECT NEWID(), b.Id, b.CustomerId, 5,
           'Everything was handled so smoothly, from the itinerary to the hotel bookings. Would definitely book with One Click Yatra again!',
           '2026-09-22T09:00:00'
    FROM Bookings b WHERE b.BookingNumber = 'BK-20260921-DEMO06';
END
GO

IF NOT EXISTS (SELECT 1 FROM Feedbacks f INNER JOIN Bookings b ON b.Id = f.BookingId WHERE b.BookingNumber = 'BK-20260917-DEMO02')
BEGIN
    INSERT INTO Feedbacks (Id, BookingId, CustomerId, Rating, Comment, CreatedAt)
    SELECT NEWID(), b.Id, b.CustomerId, 4,
           'Great value for money and the support team responded quickly whenever we had questions during the trip.',
           '2026-09-18T14:30:00'
    FROM Bookings b WHERE b.BookingNumber = 'BK-20260917-DEMO02';
END
GO
