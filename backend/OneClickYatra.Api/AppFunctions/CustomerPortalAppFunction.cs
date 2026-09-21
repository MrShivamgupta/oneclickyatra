using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;
using OneClickYatra.Api.Services.Storage;

namespace OneClickYatra.Api.AppFunctions;

public sealed class CustomerPortalAppFunction : ICustomerPortalAppFunction
{
    private static readonly string[] FeedbackEligibleStatuses = ["Completed"];
    private static readonly string[] VoucherEligibleStatuses = ["Confirmed", "InProgress", "Completed"];

    private readonly ICustomerRepository _customerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBookingAppFunction _bookingAppFunction;
    private readonly IPaymentAppFunction _paymentAppFunction;
    private readonly IInvoiceAppFunction _invoiceAppFunction;
    private readonly IQuotationAppFunction _quotationAppFunction;
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly ICustomerDocumentRepository _customerDocumentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IVoucherPdfService _voucherPdfService;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ILogger<CustomerPortalAppFunction> _logger;

    public CustomerPortalAppFunction(
        ICustomerRepository __customerRepository,
        IUserRepository __userRepository,
        IBookingAppFunction __bookingAppFunction,
        IPaymentAppFunction __paymentAppFunction,
        IInvoiceAppFunction __invoiceAppFunction,
        IQuotationAppFunction __quotationAppFunction,
        IFeedbackRepository __feedbackRepository,
        ICustomerDocumentRepository __customerDocumentRepository,
        IFileStorageService __fileStorageService,
        IVoucherPdfService __voucherPdfService,
        IAuditLogWriter __auditLogWriter,
        ILogger<CustomerPortalAppFunction> __logger)
    {
        _customerRepository = __customerRepository;
        _userRepository = __userRepository;
        _bookingAppFunction = __bookingAppFunction;
        _paymentAppFunction = __paymentAppFunction;
        _invoiceAppFunction = __invoiceAppFunction;
        _quotationAppFunction = __quotationAppFunction;
        _feedbackRepository = __feedbackRepository;
        _customerDocumentRepository = __customerDocumentRepository;
        _fileStorageService = __fileStorageService;
        _voucherPdfService = __voucherPdfService;
        _auditLogWriter = __auditLogWriter;
        _logger = __logger;
    }

    public async Task<CustomerResponse> GetMyProfileAsync(Guid __userId, CancellationToken __cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(__userId, __cancellationToken);
        return ToResponse(customer);
    }

    public async Task<CustomerResponse> UpdateMyProfileAsync(Guid __userId, CustomerRequest __request, CancellationToken __cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(__userId, __cancellationToken);
        customer.FullName = __request.FullName;
        customer.Email = __request.Email;
        customer.Phone = __request.Phone;
        customer.UpdatedBy = __userId;

        await _customerRepository.UpdateAsync(customer, __cancellationToken);
        _logger.LogInformation("Customer {CustomerId} profile updated via portal by {UserId}", customer.Id, __userId);
        try
        {
            await _auditLogWriter.LogAsync(__userId, "customer.profile.updated_via_portal", "Customer", customer.Id.ToString(), null, customer.FullName, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "customer.profile.updated_via_portal", "Customer", customer.Id);
        }

        return ToResponse(customer);
    }

    public async Task<PaginationResponse<BookingResponse>> GetMyBookingsAsync(Guid __userId, PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(__userId, __cancellationToken);
        return await _bookingAppFunction.SearchAsync(new BookingSearchRequest
        {
            PageNumber = __request.PageNumber,
            PageSize = __request.PageSize,
            CustomerId = customer.Id
        }, __cancellationToken);
    }

    public async Task<BookingDetailResponse> GetMyBookingByIdAsync(Guid __userId, Guid __bookingId, CancellationToken __cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(__userId, __cancellationToken);
        var detail = await _bookingAppFunction.GetByIdAsync(__bookingId, __cancellationToken);
        EnsureOwnedByCustomer(detail.Booking.CustomerId, customer.Id, "Booking", __bookingId);
        return detail;
    }

    public async Task<PaginationResponse<PaymentResponse>> GetMyPaymentsAsync(Guid __userId, PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(__userId, __cancellationToken);
        return await _paymentAppFunction.SearchAsync(new PaymentSearchRequest
        {
            PageNumber = __request.PageNumber,
            PageSize = __request.PageSize,
            CustomerId = customer.Id
        }, __cancellationToken);
    }

    public async Task<PaginationResponse<InvoiceResponse>> GetMyInvoicesAsync(Guid __userId, PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(__userId, __cancellationToken);
        return await _invoiceAppFunction.SearchAsync(new InvoiceSearchRequest
        {
            PageNumber = __request.PageNumber,
            PageSize = __request.PageSize,
            CustomerId = customer.Id
        }, __cancellationToken);
    }

    public async Task<byte[]> GetMyInvoicePdfAsync(Guid __userId, Guid __invoiceId, CancellationToken __cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(__userId, __cancellationToken);
        var invoice = await _invoiceAppFunction.GetByIdAsync(__invoiceId, __cancellationToken);
        var booking = await _bookingAppFunction.GetByIdAsync(invoice.BookingId, __cancellationToken);
        EnsureOwnedByCustomer(booking.Booking.CustomerId, customer.Id, "Invoice", __invoiceId);

        return await _invoiceAppFunction.GeneratePdfAsync(__invoiceId, __cancellationToken);
    }

    public async Task<PaginationResponse<QuotationResponse>> GetMyQuotationsAsync(Guid __userId, PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(__userId, __cancellationToken);
        return await _quotationAppFunction.SearchAsync(new QuotationSearchRequest
        {
            PageNumber = __request.PageNumber,
            PageSize = __request.PageSize,
            CustomerId = customer.Id
        }, __cancellationToken);
    }

    public async Task<FeedbackResponse> SubmitFeedbackAsync(Guid __userId, Guid __bookingId, FeedbackRequest __request, CancellationToken __cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(__userId, __cancellationToken);
        var detail = await _bookingAppFunction.GetByIdAsync(__bookingId, __cancellationToken);
        EnsureOwnedByCustomer(detail.Booking.CustomerId, customer.Id, "Booking", __bookingId);

        if (!FeedbackEligibleStatuses.Contains(detail.Booking.Status))
        {
            throw new BusinessException("Feedback can only be submitted once the trip is completed.");
        }

        if (await _feedbackRepository.GetByBookingIdAsync(__bookingId, __cancellationToken) is not null)
        {
            throw new BusinessException("Feedback has already been submitted for this booking.");
        }

        var feedback = new FeedbackModel
        {
            Id = Guid.NewGuid(),
            BookingId = __bookingId,
            CustomerId = customer.Id,
            Rating = __request.Rating,
            Comment = __request.Comment,
            CreatedBy = __userId
        };

        await _feedbackRepository.CreateAsync(feedback, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(__userId, "feedback.submitted", "Booking", __bookingId.ToString(), null, __request.Rating.ToString(), __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "feedback.submitted", "Booking", __bookingId);
        }

        return new FeedbackResponse
        {
            Id = feedback.Id,
            BookingId = feedback.BookingId,
            BookingNumber = detail.Booking.BookingNumber,
            CustomerName = customer.FullName,
            Rating = feedback.Rating,
            Comment = feedback.Comment,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<CustomerDocumentResponse> UploadDocumentAsync(Guid __userId, Guid __bookingId, Stream __content, string __fileName, string __contentType, CancellationToken __cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(__userId, __cancellationToken);
        var detail = await _bookingAppFunction.GetByIdAsync(__bookingId, __cancellationToken);
        EnsureOwnedByCustomer(detail.Booking.CustomerId, customer.Id, "Booking", __bookingId);

        var storagePath = await _fileStorageService.SaveAsync(__content, __fileName, __contentType, __cancellationToken);

        var document = new CustomerDocumentModel
        {
            Id = Guid.NewGuid(),
            BookingId = __bookingId,
            UploadedByUserId = __userId,
            FileName = __fileName,
            ContentType = __contentType,
            FileSizeBytes = __content.Length,
            StoragePath = storagePath,
            CreatedBy = __userId
        };
        await _customerDocumentRepository.CreateAsync(document, __cancellationToken);
        // CreateAsync's INSERT stamps CreatedAt via SYSUTCDATETIME() rather than the (unset) model
        // property — set it here purely so the response returned to the caller reflects it, the
        // same approach WhatsAppAppFunction.SendMessageAsync already uses for its log response.
        document.CreatedAt = DateTime.UtcNow;
        _logger.LogInformation("Customer document {DocumentId} uploaded for booking {BookingId} by {UserId}", document.Id, __bookingId, __userId);

        try
        {
            await _auditLogWriter.LogAsync(__userId, "customer_document.uploaded", "Booking", __bookingId.ToString(), null, __fileName, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "customer_document.uploaded", "Booking", __bookingId);
        }

        return ToDocumentResponse(document);
    }

    public async Task<List<CustomerDocumentResponse>> GetDocumentsAsync(Guid __userId, Guid __bookingId, CancellationToken __cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(__userId, __cancellationToken);
        var detail = await _bookingAppFunction.GetByIdAsync(__bookingId, __cancellationToken);
        EnsureOwnedByCustomer(detail.Booking.CustomerId, customer.Id, "Booking", __bookingId);

        var documents = await _customerDocumentRepository.GetByBookingIdAsync(__bookingId, __cancellationToken);
        return documents.Select(ToDocumentResponse).ToList();
    }

    public async Task<(Stream Content, string ContentType, string FileName)> DownloadDocumentAsync(Guid __userId, Guid __documentId, CancellationToken __cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(__userId, __cancellationToken);
        var document = await _customerDocumentRepository.GetByIdAsync(__documentId, __cancellationToken) ?? throw new EntityNotFoundException("Document", __documentId);
        var detail = await _bookingAppFunction.GetByIdAsync(document.BookingId, __cancellationToken);
        EnsureOwnedByCustomer(detail.Booking.CustomerId, customer.Id, "Document", __documentId);

        var stored = await _fileStorageService.ReadAsync(document.StoragePath, __cancellationToken);
        return (stored.Content, document.ContentType, document.FileName);
    }

    public async Task<byte[]> GetVoucherPdfAsync(Guid __userId, Guid __bookingId, CancellationToken __cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(__userId, __cancellationToken);
        var detail = await _bookingAppFunction.GetByIdAsync(__bookingId, __cancellationToken);
        EnsureOwnedByCustomer(detail.Booking.CustomerId, customer.Id, "Booking", __bookingId);

        if (!VoucherEligibleStatuses.Contains(detail.Booking.Status))
        {
            throw new BusinessException("A voucher is only available once the booking is confirmed.");
        }

        return _voucherPdfService.Generate(detail);
    }

    /// <summary>Resolves the Customer linked to this user, creating one if this account predates
    /// registration linking Customers automatically (or was provisioned by an admin directly).</summary>
    private async Task<CustomerModel> GetOrCreateCustomerAsync(Guid __userId, CancellationToken __cancellationToken)
    {
        var existing = await _customerRepository.GetByUserIdAsync(__userId, __cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var user = await _userRepository.GetByIdAsync(__userId, __cancellationToken) ?? throw new EntityNotFoundException("User", __userId);
        var customer = new CustomerModel
        {
            Id = Guid.NewGuid(),
            FullName = user.FullName,
            Email = user.Email,
            Phone = string.Empty,
            UserId = user.Id,
            CreatedBy = user.Id
        };

        await _customerRepository.CreateAsync(customer, __cancellationToken);
        return customer;
    }

    /// <summary>Uses EntityNotFoundException (never a 403) for ownership failures, so a customer
    /// probing another customer's id learns nothing about whether it exists.</summary>
    private static void EnsureOwnedByCustomer(Guid? __actualOwnerId, Guid __expectedOwnerId, string __entityName, Guid __entityId)
    {
        if (__actualOwnerId != __expectedOwnerId)
        {
            throw new EntityNotFoundException(__entityName, __entityId);
        }
    }

    private static CustomerDocumentResponse ToDocumentResponse(CustomerDocumentModel document) => new()
    {
        Id = document.Id,
        BookingId = document.BookingId,
        FileName = document.FileName,
        ContentType = document.ContentType,
        FileSizeBytes = document.FileSizeBytes,
        CreatedAt = document.CreatedAt
    };

    private static CustomerResponse ToResponse(CustomerModel customer) => new()
    {
        Id = customer.Id,
        FullName = customer.FullName,
        Email = customer.Email,
        Phone = customer.Phone,
        UserId = customer.UserId
    };
}
