-- Populates HeroImageUrl for the seeded Destinations/Packages -- both public.html templates
-- already render this field (Home's card-grid and the destination/package detail pages fall back
-- to a plain emoji placeholder only when it's null), so this is a pure data fix, no frontend code
-- change needed. Images are real, freely-licensed (Unsplash License -- free for commercial use, no
-- attribution required) photos, verified to load (HTTP 200) before being written here. Only rows
-- still missing an image are touched, so this is safe to re-run and won't clobber a real photo an
-- admin uploads later.

UPDATE Destinations SET HeroImageUrl = 'https://images.unsplash.com/photo-1557093793-d149a38a1be8?w=1200&q=80&auto=format&fit=crop'
WHERE Slug = 'bali-indonesia' AND HeroImageUrl IS NULL;

UPDATE Destinations SET HeroImageUrl = 'https://images.unsplash.com/photo-1745750434535-5943ef2fd31a?w=1200&q=80&auto=format&fit=crop'
WHERE Slug = 'dubai-uae' AND HeroImageUrl IS NULL;

UPDATE Destinations SET HeroImageUrl = 'https://images.unsplash.com/photo-1614082242765-7c98ca0f3df3?w=1200&q=80&auto=format&fit=crop'
WHERE Slug = 'goa-india' AND HeroImageUrl IS NULL;

UPDATE Destinations SET HeroImageUrl = 'https://images.unsplash.com/photo-1578155173088-710a9aef3849?w=1200&q=80&auto=format&fit=crop'
WHERE Slug = 'jaipur-india' AND HeroImageUrl IS NULL;

UPDATE Destinations SET HeroImageUrl = 'https://images.unsplash.com/photo-1724405504642-39518cce855a?w=1200&q=80&auto=format&fit=crop'
WHERE Slug = 'manali-india' AND HeroImageUrl IS NULL;

UPDATE Destinations SET HeroImageUrl = 'https://images.unsplash.com/photo-1585977411904-049f086f89bb?w=1200&q=80&auto=format&fit=crop'
WHERE Slug = 'phuket-thailand' AND HeroImageUrl IS NULL;

UPDATE Destinations SET HeroImageUrl = 'https://images.unsplash.com/photo-1638951239897-89cea3d899de?w=1200&q=80&auto=format&fit=crop'
WHERE Slug = 'shimla-india' AND HeroImageUrl IS NULL;

UPDATE Destinations SET HeroImageUrl = 'https://images.unsplash.com/photo-1774075884764-be7319c06e08?w=1200&q=80&auto=format&fit=crop'
WHERE Slug = 'singapore' AND HeroImageUrl IS NULL;

UPDATE Destinations SET HeroImageUrl = 'https://images.unsplash.com/photo-1622462282200-98e80f729c63?w=1200&q=80&auto=format&fit=crop'
WHERE Slug = 'udaipur-india' AND HeroImageUrl IS NULL;

UPDATE Packages SET HeroImageUrl = 'https://images.unsplash.com/photo-1614082242765-7c98ca0f3df3?w=1200&q=80&auto=format&fit=crop'
WHERE Slug = 'goa-family-getaway' AND HeroImageUrl IS NULL;

UPDATE Packages SET HeroImageUrl = 'https://images.unsplash.com/photo-1557093793-d149a38a1be8?w=1200&q=80&auto=format&fit=crop'
WHERE Slug = 'smoke-test-package-1789563738' AND HeroImageUrl IS NULL;
GO
