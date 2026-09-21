-- Seeds the 8 default email templates the Communication domain's outbound email flows render
-- from (BookingConfirmation / BookingCancellation / PaymentReceipt / RefundConfirmation /
-- EnquiryAcknowledgment / QuotationSent / PaymentReminder / DepartureReminder). Each BodyHtml uses
-- simple {{PlaceholderName}} tokens substituted by EmailTemplateService.RenderAsync -- no
-- Handlebars/Razor logic, just literal string replacement -- so the exact token names below are the
-- contract the trigger-wiring AppFunctions must pass in their placeholders dictionary.

INSERT INTO EmailTemplates (Id, Name, Subject, BodyHtml, IsActive, CreatedAt, IsDeleted)
SELECT NEWID(), v.Name, v.Subject, v.BodyHtml, 1, SYSUTCDATETIME(), 0
FROM (VALUES
    (
        'BookingConfirmation',
        'Your booking {{BookingReference}} is confirmed!',
        '<div style="font-family:Arial,sans-serif;color:#1f2933;line-height:1.6;max-width:600px;">' +
        '<p>Dear {{CustomerName}},</p>' +
        '<p>Great news -- your booking with One Click Yatra is confirmed! Here are your trip details:</p>' +
        '<table style="border-collapse:collapse;width:100%;margin:16px 0;">' +
        '<tr><td style="padding:6px 0;color:#52606d;">Booking Reference</td><td style="padding:6px 0;font-weight:bold;">{{BookingReference}}</td></tr>' +
        '<tr><td style="padding:6px 0;color:#52606d;">Package</td><td style="padding:6px 0;">{{PackageName}}</td></tr>' +
        '<tr><td style="padding:6px 0;color:#52606d;">Travel Dates</td><td style="padding:6px 0;">{{TravelStartDate}} to {{TravelEndDate}}</td></tr>' +
        '<tr><td style="padding:6px 0;color:#52606d;">Number of Travelers</td><td style="padding:6px 0;">{{TravelerCount}}</td></tr>' +
        '<tr><td style="padding:6px 0;color:#52606d;">Total Amount</td><td style="padding:6px 0;">{{TotalAmount}}</td></tr>' +
        '</table>' +
        '<p>We will be in touch with your itinerary and travel documents closer to your departure date. If you have any questions in the meantime, just reply to this email or call us at {{SupportPhone}}.</p>' +
        '<p>Thank you for choosing us -- we cannot wait to help you make wonderful memories!</p>' +
        '<p>Warm regards,<br/>Team One Click Yatra</p>' +
        '</div>'
    ),
    (
        'BookingCancellation',
        'Your booking {{BookingReference}} has been cancelled',
        '<div style="font-family:Arial,sans-serif;color:#1f2933;line-height:1.6;max-width:600px;">' +
        '<p>Dear {{CustomerName}},</p>' +
        '<p>This email confirms that your booking <strong>{{BookingReference}}</strong> for {{PackageName}} has been cancelled as requested.</p>' +
        '<p><strong>Reason:</strong> {{CancellationReason}}</p>' +
        '<p>{{RefundAmount}} will be refunded to your original payment method within 7-10 business days. You will receive a separate confirmation once the refund has been processed.</p>' +
        '<p>We are sorry to see this trip not go ahead, and we would love to help you plan a future one whenever you are ready. Reach us anytime at {{SupportPhone}}.</p>' +
        '<p>Warm regards,<br/>Team One Click Yatra</p>' +
        '</div>'
    ),
    (
        'PaymentReceipt',
        'Payment receipt for booking {{BookingReference}}',
        '<div style="font-family:Arial,sans-serif;color:#1f2933;line-height:1.6;max-width:600px;">' +
        '<p>Dear {{CustomerName}},</p>' +
        '<p>Thank you for your payment. Here is your receipt for booking {{BookingReference}}:</p>' +
        '<table style="border-collapse:collapse;width:100%;margin:16px 0;">' +
        '<tr><td style="padding:6px 0;color:#52606d;">Receipt Number</td><td style="padding:6px 0;font-weight:bold;">{{ReceiptNumber}}</td></tr>' +
        '<tr><td style="padding:6px 0;color:#52606d;">Amount Paid</td><td style="padding:6px 0;">{{AmountPaid}}</td></tr>' +
        '<tr><td style="padding:6px 0;color:#52606d;">Payment Date</td><td style="padding:6px 0;">{{PaymentDate}}</td></tr>' +
        '<tr><td style="padding:6px 0;color:#52606d;">Payment Method</td><td style="padding:6px 0;">{{PaymentMethod}}</td></tr>' +
        '<tr><td style="padding:6px 0;color:#52606d;">Balance Due</td><td style="padding:6px 0;">{{BalanceDue}}</td></tr>' +
        '</table>' +
        '<p>Please keep this receipt for your records. If any of these details look incorrect, let us know right away by replying to this email.</p>' +
        '<p>Warm regards,<br/>Team One Click Yatra</p>' +
        '</div>'
    ),
    (
        'RefundConfirmation',
        'Your refund for booking {{BookingReference}} has been processed',
        '<div style="font-family:Arial,sans-serif;color:#1f2933;line-height:1.6;max-width:600px;">' +
        '<p>Dear {{CustomerName}},</p>' +
        '<p>We have processed a refund for your booking {{BookingReference}}. Details are below:</p>' +
        '<table style="border-collapse:collapse;width:100%;margin:16px 0;">' +
        '<tr><td style="padding:6px 0;color:#52606d;">Refund Amount</td><td style="padding:6px 0;font-weight:bold;">{{RefundAmount}}</td></tr>' +
        '<tr><td style="padding:6px 0;color:#52606d;">Refund Date</td><td style="padding:6px 0;">{{RefundDate}}</td></tr>' +
        '<tr><td style="padding:6px 0;color:#52606d;">Refunded To</td><td style="padding:6px 0;">{{RefundMode}}</td></tr>' +
        '</table>' +
        '<p>Depending on your bank, it can take a few extra business days for the amount to reflect in your account. If it has not arrived within 10 business days, please contact us at {{SupportPhone}} and we will follow up right away.</p>' +
        '<p>Warm regards,<br/>Team One Click Yatra</p>' +
        '</div>'
    ),
    (
        'EnquiryAcknowledgment',
        'We have received your enquiry, {{CustomerName}}!',
        '<div style="font-family:Arial,sans-serif;color:#1f2933;line-height:1.6;max-width:600px;">' +
        '<p>Dear {{CustomerName}},</p>' +
        '<p>Thank you for reaching out to One Click Yatra about {{DestinationName}}. Your enquiry ({{EnquiryReference}}) has been received, and one of our travel experts will get back to you within 24 hours with options tailored to your plans.</p>' +
        '<p>In the meantime, if you would like to share any preferences -- travel dates, budget, or number of travelers -- just reply to this email so we can put together the best possible plan for you.</p>' +
        '<p>Talk soon,<br/>Team One Click Yatra<br/>{{SupportPhone}}</p>' +
        '</div>'
    ),
    (
        'QuotationSent',
        'Your travel quotation for {{DestinationName}} is ready',
        '<div style="font-family:Arial,sans-serif;color:#1f2933;line-height:1.6;max-width:600px;">' +
        '<p>Dear {{CustomerName}},</p>' +
        '<p>As promised, we have put together a quotation for your trip to {{DestinationName}}. Here is a quick summary:</p>' +
        '<table style="border-collapse:collapse;width:100%;margin:16px 0;">' +
        '<tr><td style="padding:6px 0;color:#52606d;">Quotation Reference</td><td style="padding:6px 0;font-weight:bold;">{{QuotationReference}}</td></tr>' +
        '<tr><td style="padding:6px 0;color:#52606d;">Total Amount</td><td style="padding:6px 0;">{{TotalAmount}}</td></tr>' +
        '<tr><td style="padding:6px 0;color:#52606d;">Valid Until</td><td style="padding:6px 0;">{{ValidTillDate}}</td></tr>' +
        '</table>' +
        '<p><a href="{{QuotationLink}}" style="color:#2563eb;">View your full quotation and itinerary here</a>.</p>' +
        '<p>Have questions or want to tweak anything -- the hotel, the activities, the dates? Just reply to this email and we will adjust it for you.</p>' +
        '<p>Warm regards,<br/>Team One Click Yatra</p>' +
        '</div>'
    ),
    (
        'PaymentReminder',
        'Reminder: payment due for booking {{BookingReference}}',
        '<div style="font-family:Arial,sans-serif;color:#1f2933;line-height:1.6;max-width:600px;">' +
        '<p>Dear {{CustomerName}},</p>' +
        '<p>This is a friendly reminder that a payment of {{DueAmount}} for your booking {{BookingReference}} ({{PackageName}}) is due by {{DueDate}}.</p>' +
        '<p><a href="{{PaymentLink}}" style="color:#2563eb;">Click here to complete your payment securely</a>.</p>' +
        '<p>Keeping this payment on schedule helps us confirm your hotel and travel arrangements without any last-minute hiccups. If you have already paid or need help, just let us know by replying to this email or calling {{SupportPhone}}.</p>' +
        '<p>Warm regards,<br/>Team One Click Yatra</p>' +
        '</div>'
    ),
    (
        'DepartureReminder',
        'Your trip to {{DestinationName}} starts in {{DaysToDeparture}} days!',
        '<div style="font-family:Arial,sans-serif;color:#1f2933;line-height:1.6;max-width:600px;">' +
        '<p>Dear {{CustomerName}},</p>' +
        '<p>Your adventure to {{DestinationName}} is almost here -- {{PackageName}} departs on {{DepartureDate}}, just {{DaysToDeparture}} days from now!</p>' +
        '<p>A quick checklist before you go:</p>' +
        '<ul>' +
        '<li>Valid ID / passport and any required visas</li>' +
        '<li>Printed or downloaded copies of your booking voucher and tickets</li>' +
        '<li>Travel insurance details, if applicable</li>' +
        '<li>Weather-appropriate packing for your destination</li>' +
        '</ul>' +
        '<p>If you need anything at all before departure, we are just a call or email away at {{SupportPhone}}.</p>' +
        '<p>Have a wonderful trip!<br/>Team One Click Yatra</p>' +
        '</div>'
    )
) AS v(Name, Subject, BodyHtml)
WHERE NOT EXISTS (SELECT 1 FROM EmailTemplates t WHERE t.Name = v.Name AND t.IsDeleted = 0);
GO
