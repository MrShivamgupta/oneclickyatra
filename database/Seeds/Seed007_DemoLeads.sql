-- Development/demo data only. Never run against Production.

DECLARE @GoaDestId UNIQUEIDENTIFIER = (SELECT Id FROM Destinations WHERE Slug = 'goa-india');
DECLARE @BaliDestId UNIQUEIDENTIFIER = (SELECT Id FROM Destinations WHERE Slug = 'bali-indonesia');
DECLARE @DubaiDestId UNIQUEIDENTIFIER = (SELECT Id FROM Destinations WHERE Slug = 'dubai-uae');
DECLARE @SuperAdminId UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Email = 'superadmin@oneclickyatra.dev');

INSERT INTO Leads (Id, CustomerName, Mobile, Email, DestinationId, TravelDate, Budget, Source, AssignedToUserId, LeadScore, Status)
VALUES
    (NEWID(), 'Rahul Verma', '+919812300001', 'rahul.verma@example.com', @GoaDestId, '2026-12-15', 80000, 'Website', @SuperAdminId, 72, 'Contacted'),
    (NEWID(), 'Priya Mehta', '+919812300002', 'priya.mehta@example.com', @BaliDestId, '2027-01-10', 120000, 'Instagram', @SuperAdminId, 65, 'QuotationSent'),
    (NEWID(), 'Aman Gupta', '+919812300003', 'aman.gupta@example.com', @DubaiDestId, '2026-11-20', 90000, 'Referral', @SuperAdminId, 40, 'New'),
    (NEWID(), 'Neha Singh', '+919812300004', 'neha.singh@example.com', @GoaDestId, '2027-02-05', 60000, 'Facebook', NULL, 25, 'New');
GO

DECLARE @RahulLeadId UNIQUEIDENTIFIER = (SELECT Id FROM Leads WHERE CustomerName = 'Rahul Verma');
DECLARE @PriyaLeadId UNIQUEIDENTIFIER = (SELECT Id FROM Leads WHERE CustomerName = 'Priya Mehta');

INSERT INTO FollowUps (Id, LeadId, ScheduledAt, Type, Notes, Status)
VALUES
    (NEWID(), @RahulLeadId, SYSUTCDATETIME(), 'Call', 'Confirm Goa trip dates.', 'Pending'),
    (NEWID(), @PriyaLeadId, SYSUTCDATETIME(), 'WhatsApp', 'Send Thailand package as an alternative.', 'Pending');
GO
