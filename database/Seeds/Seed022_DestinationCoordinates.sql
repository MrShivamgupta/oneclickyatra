-- Backfills Latitude/Longitude (added by V026) for every currently-seeded destination, so the new
-- public destinations map has real pins from day one instead of an empty map. Matched by Slug
-- (stable, unique) rather than Name, same idiom as Seed020's data-quality fix.
UPDATE Destinations SET Latitude = 15.2993,  Longitude = 74.1240  WHERE Slug = 'goa-india';
UPDATE Destinations SET Latitude = -8.3405,  Longitude = 115.0920 WHERE Slug = 'bali-indonesia';
UPDATE Destinations SET Latitude = 7.8804,   Longitude = 98.3923  WHERE Slug = 'phuket-thailand';
UPDATE Destinations SET Latitude = 25.2048,  Longitude = 55.2708  WHERE Slug = 'dubai-uae';
UPDATE Destinations SET Latitude = 26.9124,  Longitude = 75.7873  WHERE Slug = 'jaipur-india';
UPDATE Destinations SET Latitude = 32.2432,  Longitude = 77.1892  WHERE Slug = 'manali-india';
UPDATE Destinations SET Latitude = 31.1048,  Longitude = 77.1734  WHERE Slug = 'shimla-india';
UPDATE Destinations SET Latitude = 1.3521,   Longitude = 103.8198 WHERE Slug = 'singapore';
UPDATE Destinations SET Latitude = 24.5854,  Longitude = 73.7125  WHERE Slug = 'udaipur-india';
