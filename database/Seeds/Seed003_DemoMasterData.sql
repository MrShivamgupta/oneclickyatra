-- Development/demo data only. Never run against Production.

DECLARE @India UNIQUEIDENTIFIER = NEWID();
DECLARE @Indonesia UNIQUEIDENTIFIER = NEWID();
DECLARE @Thailand UNIQUEIDENTIFIER = NEWID();
DECLARE @UAE UNIQUEIDENTIFIER = NEWID();
DECLARE @Singapore UNIQUEIDENTIFIER = NEWID();

INSERT INTO Countries (Id, Name, IsoCode)
VALUES
    (@India, 'India', 'IN'),
    (@Indonesia, 'Indonesia', 'ID'),
    (@Thailand, 'Thailand', 'TH'),
    (@UAE, 'United Arab Emirates', 'AE'),
    (@Singapore, 'Singapore', 'SG');
GO

DECLARE @IndiaId UNIQUEIDENTIFIER = (SELECT Id FROM Countries WHERE IsoCode = 'IN');
DECLARE @IndonesiaId UNIQUEIDENTIFIER = (SELECT Id FROM Countries WHERE IsoCode = 'ID');
DECLARE @ThailandId UNIQUEIDENTIFIER = (SELECT Id FROM Countries WHERE IsoCode = 'TH');
DECLARE @UAEId UNIQUEIDENTIFIER = (SELECT Id FROM Countries WHERE IsoCode = 'AE');

INSERT INTO Cities (Id, CountryId, Name)
VALUES
    (NEWID(), @IndiaId, 'Goa'),
    (NEWID(), @IndiaId, 'Srinagar'),
    (NEWID(), @IndonesiaId, 'Denpasar'),
    (NEWID(), @ThailandId, 'Phuket'),
    (NEWID(), @UAEId, 'Dubai');
GO

INSERT INTO Categories (Id, Name, Slug, Description)
VALUES
    (NEWID(), 'Honeymoon', 'honeymoon', 'Romantic getaways for couples.'),
    (NEWID(), 'Adventure', 'adventure', 'Trekking, water sports and outdoor activities.'),
    (NEWID(), 'Family', 'family', 'Kid-friendly resorts and activities.'),
    (NEWID(), 'Pilgrimage', 'pilgrimage', 'Religious and spiritual tours.'),
    (NEWID(), 'Luxury', 'luxury', 'Premium stays and experiences.');
GO

INSERT INTO Seasons (Id, Name, StartMonth, EndMonth)
VALUES
    (NEWID(), 'Winter', 11, 2),
    (NEWID(), 'Summer', 3, 6),
    (NEWID(), 'Monsoon', 7, 10);
GO

DECLARE @GoaCityId UNIQUEIDENTIFIER = (SELECT Id FROM Cities WHERE Name = 'Goa');
DECLARE @DenpasarCityId UNIQUEIDENTIFIER = (SELECT Id FROM Cities WHERE Name = 'Denpasar');
DECLARE @PhuketCityId UNIQUEIDENTIFIER = (SELECT Id FROM Cities WHERE Name = 'Phuket');
DECLARE @DubaiCityId UNIQUEIDENTIFIER = (SELECT Id FROM Cities WHERE Name = 'Dubai');
DECLARE @IndiaCountryId UNIQUEIDENTIFIER = (SELECT Id FROM Countries WHERE IsoCode = 'IN');
DECLARE @IndonesiaCountryId UNIQUEIDENTIFIER = (SELECT Id FROM Countries WHERE IsoCode = 'ID');
DECLARE @ThailandCountryId UNIQUEIDENTIFIER = (SELECT Id FROM Countries WHERE IsoCode = 'TH');
DECLARE @UAECountryId UNIQUEIDENTIFIER = (SELECT Id FROM Countries WHERE IsoCode = 'AE');
DECLARE @SingaporeCountryId UNIQUEIDENTIFIER = (SELECT Id FROM Countries WHERE IsoCode = 'SG');

INSERT INTO Destinations (Id, CountryId, CityId, Name, Slug, ShortDescription, IsFeatured, IsPublished)
VALUES
    (NEWID(), @IndiaCountryId, @GoaCityId, 'Goa', 'goa-india', 'Beaches, nightlife and Portuguese heritage.', 1, 1),
    (NEWID(), @IndonesiaCountryId, @DenpasarCityId, 'Bali', 'bali-indonesia', 'Temples, rice terraces and beach resorts.', 1, 1),
    (NEWID(), @ThailandCountryId, @PhuketCityId, 'Phuket', 'phuket-thailand', 'Island hopping and water sports.', 1, 1),
    (NEWID(), @UAECountryId, @DubaiCityId, 'Dubai', 'dubai-uae', 'Shopping, desert safaris and skyscrapers.', 0, 1),
    (NEWID(), @SingaporeCountryId, NULL, 'Singapore', 'singapore', 'Family-friendly city break.', 0, 1);
GO
