using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;
using OneClickYatra.Api.Services.Storage;

namespace OneClickYatra.Api.AppFunctions;

public sealed class VendorAppFunction : IVendorAppFunction
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDestinationRepository _destinationRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IVendorInvoiceRepository _vendorInvoiceRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ILogger<VendorAppFunction> _logger;

    public VendorAppFunction(
        IVendorRepository __vendorRepository,
        IUserRepository __userRepository,
        IDestinationRepository __destinationRepository,
        IBookingRepository __bookingRepository,
        IVendorInvoiceRepository __vendorInvoiceRepository,
        IFileStorageService __fileStorageService,
        ICurrentUserAccessor __currentUserAccessor,
        IAuditLogWriter __auditLogWriter,
        ILogger<VendorAppFunction> __logger)
    {
        _vendorRepository = __vendorRepository;
        _userRepository = __userRepository;
        _destinationRepository = __destinationRepository;
        _bookingRepository = __bookingRepository;
        _vendorInvoiceRepository = __vendorInvoiceRepository;
        _fileStorageService = __fileStorageService;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
        _logger = __logger;
    }

    public async Task<PaginationResponse<VendorResponse>> SearchAsync(VendorSearchRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _vendorRepository.SearchAsync(__request, __cancellationToken);
        return PaginationResponse<VendorResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<VendorResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var vendor = await GetVendorOrThrowAsync(__id, __cancellationToken);
        return ToResponse(vendor);
    }

    public async Task<VendorResponse> CreateAsync(VendorRequest __request, CancellationToken __cancellationToken)
    {
        var vendor = new VendorModel
        {
            Id = Guid.NewGuid(),
            Name = __request.Name,
            VendorType = __request.VendorType,
            Email = __request.Email,
            Phone = __request.Phone,
            Address = __request.Address,
            City = __request.City,
            Country = __request.Country,
            IsActive = __request.IsActive,
            CreatedBy = _currentUserAccessor.UserId
        };

        await _vendorRepository.CreateAsync(vendor, __cancellationToken);
        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "vendor.created", "Vendor", vendor.Id.ToString(), null, vendor.Name, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "vendor.created", "Vendor", vendor.Id);
        }

        return ToResponse(vendor);
    }

    public async Task<VendorResponse> UpdateAsync(Guid __id, VendorRequest __request, CancellationToken __cancellationToken)
    {
        var vendor = await GetVendorOrThrowAsync(__id, __cancellationToken);

        var oldName = vendor.Name;
        vendor.Name = __request.Name;
        vendor.VendorType = __request.VendorType;
        vendor.Email = __request.Email;
        vendor.Phone = __request.Phone;
        vendor.Address = __request.Address;
        vendor.City = __request.City;
        vendor.Country = __request.Country;
        vendor.IsActive = __request.IsActive;
        vendor.UpdatedBy = _currentUserAccessor.UserId;

        await _vendorRepository.UpdateAsync(vendor, __cancellationToken);
        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "vendor.updated", "Vendor", vendor.Id.ToString(), oldName, vendor.Name, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "vendor.updated", "Vendor", vendor.Id);
        }

        return ToResponse(vendor);
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var vendor = await GetVendorOrThrowAsync(__id, __cancellationToken);

        await _vendorRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);
        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "vendor.deleted", "Vendor", __id.ToString(), vendor.Name, null, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "vendor.deleted", "Vendor", __id);
        }
    }

    public async Task<VendorResponse> LinkUserAsync(Guid __id, VendorLinkUserRequest __request, CancellationToken __cancellationToken)
    {
        var vendor = await GetVendorOrThrowAsync(__id, __cancellationToken);
        _ = await _userRepository.GetByIdAsync(__request.UserId, __cancellationToken) ?? throw new EntityNotFoundException("User", __request.UserId);

        if (await _vendorRepository.GetByUserIdAsync(__request.UserId, __cancellationToken) is { } alreadyLinked && alreadyLinked.Id != __id)
        {
            throw new BusinessException("This user is already linked to another vendor.");
        }

        await _vendorRepository.LinkUserAsync(__id, __request.UserId, _currentUserAccessor.UserId, __cancellationToken);
        _logger.LogInformation(
            "User {UserId} linked to vendor {VendorId} by {ActingUserId}, granting portal login access to the vendor's data",
            __request.UserId, __id, _currentUserAccessor.UserId);
        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "vendor.user_linked", "Vendor", __id.ToString(), null, __request.UserId.ToString(), __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "vendor.user_linked", "Vendor", __id);
        }

        vendor.UserId = __request.UserId;
        return ToResponse(vendor);
    }

    public async Task<List<VendorContactResponse>> GetContactsAsync(Guid __id, CancellationToken __cancellationToken)
    {
        _ = await GetVendorOrThrowAsync(__id, __cancellationToken);
        var contacts = await _vendorRepository.GetContactsAsync(__id, __cancellationToken);
        return contacts.Select(ToContactResponse).ToList();
    }

    public async Task<List<VendorContactResponse>> ReplaceContactsAsync(Guid __id, List<VendorContactRequest> __contacts, CancellationToken __cancellationToken)
    {
        _ = await GetVendorOrThrowAsync(__id, __cancellationToken);

        var models = __contacts.Select(c => new VendorContactModel
        {
            Id = Guid.NewGuid(),
            VendorId = __id,
            ContactName = c.ContactName,
            Designation = c.Designation,
            Phone = c.Phone,
            Email = c.Email,
            IsPrimary = c.IsPrimary
        }).ToList();

        await _vendorRepository.ReplaceContactsAsync(__id, models, __cancellationToken);
        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "vendor.contacts_updated", "Vendor", __id.ToString(), null, $"{models.Count} contact(s)", __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "vendor.contacts_updated", "Vendor", __id);
        }

        return models.Select(ToContactResponse).ToList();
    }

    public async Task<List<VendorRateResponse>> GetRatesAsync(Guid __id, CancellationToken __cancellationToken)
    {
        _ = await GetVendorOrThrowAsync(__id, __cancellationToken);
        var rates = await _vendorRepository.GetRatesAsync(__id, __cancellationToken);
        return rates.Select(ToRateResponse).ToList();
    }

    public async Task<List<VendorRateResponse>> ReplaceRatesAsync(Guid __id, List<VendorRateRequest> __rates, CancellationToken __cancellationToken)
    {
        _ = await GetVendorOrThrowAsync(__id, __cancellationToken);

        // Batched existence check (one round trip, not one per rate) — see
        // IDestinationRepository.GetExistingIdsAsync for the rationale.
        var requestedDestinationIds = __rates.Where(r => r.DestinationId.HasValue).Select(r => r.DestinationId!.Value).Distinct().ToList();
        if (requestedDestinationIds.Count > 0)
        {
            var existingDestinationIds = await _destinationRepository.GetExistingIdsAsync(requestedDestinationIds, __cancellationToken);
            var missingDestinationIds = requestedDestinationIds.Except(existingDestinationIds).ToList();
            if (missingDestinationIds.Count > 0)
            {
                throw new EntityNotFoundException("Destination", missingDestinationIds[0]);
            }
        }

        var models = __rates.Select(r => new VendorRateModel
        {
            Id = Guid.NewGuid(),
            VendorId = __id,
            DestinationId = r.DestinationId,
            ServiceDescription = r.ServiceDescription,
            RateAmount = r.RateAmount,
            Currency = r.Currency,
            ValidFrom = r.ValidFrom,
            ValidTo = r.ValidTo
        }).ToList();

        await _vendorRepository.ReplaceRatesAsync(__id, models, __cancellationToken);
        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "vendor.rates_updated", "Vendor", __id.ToString(), null, $"{models.Count} rate(s)", __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "vendor.rates_updated", "Vendor", __id);
        }

        return models.Select(ToRateResponse).ToList();
    }

    public async Task<PaginationResponse<VendorPaymentResponse>> GetPaymentsAsync(Guid __id, PaginationRequest __request, CancellationToken __cancellationToken)
    {
        _ = await GetVendorOrThrowAsync(__id, __cancellationToken);
        var page = await _vendorRepository.GetPaymentsAsync(__id, __request, __cancellationToken);
        return PaginationResponse<VendorPaymentResponse>.Create(page.Items.Select(ToPaymentResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<VendorPaymentResponse> CreatePaymentAsync(Guid __id, VendorPaymentRequest __request, CancellationToken __cancellationToken)
    {
        _ = await GetVendorOrThrowAsync(__id, __cancellationToken);
        if (__request.BookingId.HasValue && await _bookingRepository.GetByIdAsync(__request.BookingId.Value, __cancellationToken) is null)
        {
            throw new EntityNotFoundException("Booking", __request.BookingId.Value);
        }

        var payment = new VendorPaymentModel
        {
            Id = Guid.NewGuid(),
            VendorId = __id,
            BookingId = __request.BookingId,
            Amount = __request.Amount,
            Status = "Pending",
            Notes = __request.Notes,
            CreatedBy = _currentUserAccessor.UserId
        };

        await _vendorRepository.CreatePaymentAsync(payment, __cancellationToken);
        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "vendor.payment_created", "Vendor", __id.ToString(), null, payment.Amount.ToString("F2"), __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "vendor.payment_created", "Vendor", __id);
        }

        var saved = await _vendorRepository.GetPaymentByIdAsync(payment.Id, __cancellationToken) ?? payment;
        return ToPaymentResponse(saved);
    }

    public async Task<VendorPaymentResponse> UpdatePaymentStatusAsync(Guid __id, Guid __paymentId, VendorPaymentStatusRequest __request, CancellationToken __cancellationToken)
    {
        _ = await GetVendorOrThrowAsync(__id, __cancellationToken);
        var payment = await _vendorRepository.GetPaymentByIdAsync(__paymentId, __cancellationToken) ?? throw new EntityNotFoundException("VendorPayment", __paymentId);
        if (payment.VendorId != __id)
        {
            throw new EntityNotFoundException("VendorPayment", __paymentId);
        }

        var paidAt = __request.Status == "Paid" ? DateTime.UtcNow : (DateTime?)null;
        await _vendorRepository.UpdatePaymentStatusAsync(__paymentId, __request.Status, paidAt, _currentUserAccessor.UserId, __cancellationToken);
        _logger.LogInformation(
            "Vendor payment {PaymentId} for vendor {VendorId} changed status from {OldStatus} to {NewStatus} (amount {Amount}) by {ActingUserId}",
            __paymentId, __id, payment.Status, __request.Status, payment.Amount, _currentUserAccessor.UserId);
        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "vendor.payment_status_changed", "VendorPayment", __paymentId.ToString(), payment.Status, __request.Status, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "vendor.payment_status_changed", "VendorPayment", __paymentId);
        }

        var saved = await _vendorRepository.GetPaymentByIdAsync(__paymentId, __cancellationToken) ?? payment;
        return ToPaymentResponse(saved);
    }

    public async Task<VendorPerformanceResponse> RecordPerformanceAsync(Guid __id, VendorPerformanceRequest __request, CancellationToken __cancellationToken)
    {
        _ = await GetVendorOrThrowAsync(__id, __cancellationToken);
        if (__request.BookingId.HasValue && await _bookingRepository.GetByIdAsync(__request.BookingId.Value, __cancellationToken) is null)
        {
            throw new EntityNotFoundException("Booking", __request.BookingId.Value);
        }

        var performance = new VendorPerformanceModel
        {
            Id = Guid.NewGuid(),
            VendorId = __id,
            BookingId = __request.BookingId,
            Rating = __request.Rating,
            Notes = __request.Notes,
            RecordedBy = _currentUserAccessor.UserId,
            CreatedBy = _currentUserAccessor.UserId
        };

        await _vendorRepository.CreatePerformanceAsync(performance, __cancellationToken);
        // Recompute-on-write: Vendors.Rating is kept as a plain rolling average over every
        // non-deleted VendorPerformance row, recalculated here rather than via a DB trigger.
        await _vendorRepository.RecomputeRatingAsync(__id, __cancellationToken);
        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "vendor.performance_recorded", "Vendor", __id.ToString(), null, performance.Rating.ToString(), __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "vendor.performance_recorded", "Vendor", __id);
        }

        var saved = (await _vendorRepository.GetPerformanceAsync(__id, __cancellationToken)).FirstOrDefault(p => p.Id == performance.Id) ?? performance;
        return ToPerformanceResponse(saved);
    }

    public async Task<List<VendorPerformanceResponse>> GetPerformanceHistoryAsync(Guid __id, CancellationToken __cancellationToken)
    {
        _ = await GetVendorOrThrowAsync(__id, __cancellationToken);
        var history = await _vendorRepository.GetPerformanceAsync(__id, __cancellationToken);
        return history.Select(ToPerformanceResponse).ToList();
    }

    public async Task<PaginationResponse<VendorInvoiceResponse>> GetInvoicesAsync(Guid __id, PaginationRequest __request, CancellationToken __cancellationToken)
    {
        _ = await GetVendorOrThrowAsync(__id, __cancellationToken);
        var page = await _vendorInvoiceRepository.SearchByVendorIdAsync(__id, __request, __cancellationToken);
        return PaginationResponse<VendorInvoiceResponse>.Create(page.Items.Select(ToInvoiceResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<VendorInvoiceResponse> UpdateInvoiceStatusAsync(Guid __id, Guid __invoiceId, VendorInvoiceStatusRequest __request, CancellationToken __cancellationToken)
    {
        _ = await GetVendorOrThrowAsync(__id, __cancellationToken);
        var invoice = await _vendorInvoiceRepository.GetByIdAsync(__invoiceId, __cancellationToken) ?? throw new EntityNotFoundException("VendorInvoice", __invoiceId);
        if (invoice.VendorId != __id)
        {
            throw new EntityNotFoundException("VendorInvoice", __invoiceId);
        }

        await _vendorInvoiceRepository.UpdateStatusAsync(__invoiceId, __request.Status, _currentUserAccessor.UserId, __cancellationToken);
        _logger.LogInformation(
            "Vendor invoice {InvoiceId} for vendor {VendorId} changed status from {OldStatus} to {NewStatus} by {ActingUserId}",
            __invoiceId, __id, invoice.Status, __request.Status, _currentUserAccessor.UserId);
        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "vendor.invoice_status_changed", "VendorInvoice", __invoiceId.ToString(), invoice.Status, __request.Status, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "vendor.invoice_status_changed", "VendorInvoice", __invoiceId);
        }

        var saved = await _vendorInvoiceRepository.GetByIdAsync(__invoiceId, __cancellationToken) ?? invoice;
        return ToInvoiceResponse(saved);
    }

    /// <summary>Staff download — any staff member with vendor.view can download any vendor's
    /// invoice file, since this is an admin review action, not a self-service one (contrast with
    /// VendorPortalAppFunction, which never exposes another vendor's files).</summary>
    public async Task<(Stream Content, string ContentType, string FileName)> DownloadInvoiceAsync(Guid __invoiceId, CancellationToken __cancellationToken)
    {
        var invoice = await _vendorInvoiceRepository.GetByIdAsync(__invoiceId, __cancellationToken) ?? throw new EntityNotFoundException("VendorInvoice", __invoiceId);
        var stored = await _fileStorageService.ReadAsync(invoice.StoragePath, __cancellationToken);
        return (stored.Content, invoice.ContentType, invoice.FileName);
    }

    private async Task<VendorModel> GetVendorOrThrowAsync(Guid __id, CancellationToken __cancellationToken)
        => await _vendorRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Vendor", __id);

    private static VendorResponse ToResponse(VendorModel vendor) => new()
    {
        Id = vendor.Id,
        Name = vendor.Name,
        VendorType = vendor.VendorType,
        Email = vendor.Email,
        Phone = vendor.Phone,
        Address = vendor.Address,
        City = vendor.City,
        Country = vendor.Country,
        UserId = vendor.UserId,
        Rating = vendor.Rating,
        IsActive = vendor.IsActive
    };

    private static VendorContactResponse ToContactResponse(VendorContactModel contact) => new()
    {
        Id = contact.Id,
        ContactName = contact.ContactName,
        Designation = contact.Designation,
        Phone = contact.Phone,
        Email = contact.Email,
        IsPrimary = contact.IsPrimary
    };

    private static VendorRateResponse ToRateResponse(VendorRateModel rate) => new()
    {
        Id = rate.Id,
        DestinationId = rate.DestinationId,
        DestinationName = rate.DestinationName,
        ServiceDescription = rate.ServiceDescription,
        RateAmount = rate.RateAmount,
        Currency = rate.Currency,
        ValidFrom = rate.ValidFrom,
        ValidTo = rate.ValidTo
    };

    private static VendorPaymentResponse ToPaymentResponse(VendorPaymentModel payment) => new()
    {
        Id = payment.Id,
        VendorId = payment.VendorId,
        BookingId = payment.BookingId,
        BookingNumber = payment.BookingNumber,
        Amount = payment.Amount,
        Status = payment.Status,
        Notes = payment.Notes,
        PaidAt = payment.PaidAt,
        CreatedAt = payment.CreatedAt
    };

    private static VendorPerformanceResponse ToPerformanceResponse(VendorPerformanceModel performance) => new()
    {
        Id = performance.Id,
        BookingId = performance.BookingId,
        BookingNumber = performance.BookingNumber,
        Rating = performance.Rating,
        Notes = performance.Notes,
        RecordedByName = performance.RecordedByName,
        CreatedAt = performance.CreatedAt
    };

    private static VendorInvoiceResponse ToInvoiceResponse(VendorInvoiceModel invoice) => new()
    {
        Id = invoice.Id,
        VendorId = invoice.VendorId,
        BookingId = invoice.BookingId,
        BookingNumber = invoice.BookingNumber,
        FileName = invoice.FileName,
        ContentType = invoice.ContentType,
        FileSizeBytes = invoice.FileSizeBytes,
        Amount = invoice.Amount,
        Notes = invoice.Notes,
        Status = invoice.Status,
        CreatedAt = invoice.CreatedAt
    };
}
