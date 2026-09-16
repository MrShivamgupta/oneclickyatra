-- Replaces placeholder CMS copy with production-ready public-site content. Idempotent.

UPDATE Pages
SET Content = N'<p>One Click Yatra helps travelers discover curated destinations and holiday packages with transparent pricing and dependable support at every step.</p>
<p>Whether you are planning a family vacation, a honeymoon, or a group tour, our travel experts work with you to design itineraries that match your dates, budget, and interests.</p>
<p>Explore destinations and packages on our website, or send us an enquiry and we will get back to you within 24 hours with tailored options.</p>',
    UpdatedAt = SYSUTCDATETIME()
WHERE Slug = 'about' AND IsDeleted = 0;
GO

UPDATE Pages
SET Content = N'<p>We would love to hear from you. Reach the One Click Yatra team using any of the channels below:</p>
<ul>
  <li><strong>Email:</strong> support@oneclickyatra.com</li>
  <li><strong>Phone:</strong> +91-98765-43210 (Mon–Sat, 9 AM – 7 PM IST)</li>
  <li><strong>Office:</strong> One Click Yatra, Travel Hub, New Delhi, India</li>
</ul>
<p>For trip planning, the fastest way to get started is our online enquiry form. Share your destination, travel dates, and group size and our experts will respond with options.</p>',
    UpdatedAt = SYSUTCDATETIME()
WHERE Slug = 'contact' AND IsDeleted = 0;
GO

UPDATE Pages
SET Content = N'<p>By using One Click Yatra and booking travel services through our platform, you agree to the following terms:</p>
<ul>
  <li>All bookings are subject to availability and confirmation by our team or third-party suppliers.</li>
  <li>Prices quoted are indicative until a formal quotation or booking confirmation is issued.</li>
  <li>Cancellation and refund rules depend on the package, supplier policy, and payment status at the time of cancellation.</li>
  <li>You are responsible for ensuring passport, visa, and travel-document requirements are met before departure.</li>
</ul>
<p>These terms may be updated from time to time. Continued use of the website constitutes acceptance of the latest version.</p>',
    UpdatedAt = SYSUTCDATETIME()
WHERE Slug = 'terms' AND IsDeleted = 0;
GO

UPDATE Pages
SET Content = N'<p>One Click Yatra respects your privacy. We collect only the information needed to respond to enquiries, process bookings, and improve our services.</p>
<ul>
  <li><strong>Information we collect:</strong> name, email, phone number, travel preferences, and booking details you provide voluntarily.</li>
  <li><strong>How we use it:</strong> to communicate about your trip, issue quotations, confirm bookings, and send service-related updates.</li>
  <li><strong>Sharing:</strong> we do not sell your personal data. We may share necessary details with hotels, airlines, and other suppliers to fulfil your booking.</li>
  <li><strong>Security:</strong> we apply industry-standard safeguards to protect your data.</li>
</ul>
<p>For privacy-related questions, contact us at privacy@oneclickyatra.com.</p>',
    UpdatedAt = SYSUTCDATETIME()
WHERE Slug = 'privacy' AND IsDeleted = 0;
GO
