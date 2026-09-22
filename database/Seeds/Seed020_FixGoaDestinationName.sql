-- Data-quality fix, found while verifying the new "Revenue by Destination" donut chart: the live
-- dev DB's Goa row (slug 'goa-india', seeded by Seed003) has Name = 'Goa1', not 'Goa'. No seed file
-- ever wrote that value -- Seed007/Seed018 both look it up by Slug, never by Name -- so it must have
-- been edited via the admin Destinations UI during this session's earlier click-through testing and
-- never reverted. It rendered fine as a bar-chart label before; the new donut's legend makes the
-- stray "1" obvious, so fixing it here rather than leaving a test artifact in demo data.
SET QUOTED_IDENTIFIER ON;
UPDATE Destinations SET Name = 'Goa' WHERE Slug = 'goa-india' AND Name = 'Goa1';
