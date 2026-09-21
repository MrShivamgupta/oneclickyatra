using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;
using OneClickYatra.Api.Services.Storage;

namespace OneClickYatra.Api.AppFunctions;

/// <summary>
/// The vendor-facing self-service portal. Gated by [Authorize] only, deliberately not
/// [HasPermission] — Vendor accounts hold no RBAC permissions at all (see Seed011), so every
/// method here authorizes by ownership ("is this my own Vendor record"), never by permission
/// claim. Mirrors CustomerPortalAppFunction's shape one level over.
/// </summary>
public sealed class VendorPortalAppFunction : IVendorPortalAppFunction
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IUserRepository _userRepository;
    private readonly IVendorInvoiceRepository _vendorInvoiceRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ILogger<VendorPortalAppFunction> _logger;

    public VendorPortalAppFunction(
        IVendorRepository __vendorRepository,
        IUserRepository __userRepository,
        IVendorInvoiceRepository __vendorInvoiceRepository,
        IFileStorageService __fileStorageService,
        IAuditLogWriter __auditLogWriter,
        ILogger<VendorPortalAppFunction> __logger)
    {
        _vendorRepository = __vendorRepository;
        _userRepository = __userRepository;
        _vendorInvoiceRepository = __vendorInvoiceRepository;
        _fileStorageService = __fileStorageService;
        _auditLogWriter = __auditLogWriter;
        _logger = __logger;
    }

    public async Task<VendorResponse> GetMyProfileAsync(Guid __userId, CancellationToken __cancellationToken)
    {
        var vendor = await GetOrCreateVendorAsync(__userId, __cancellationToken);
        return ToResponse(vendor);
    }

    public async Task<VendorResponse> UpdateMyProfileAsync(Guid __userId, VendorRequest __request, CancellationToken __cancellationToken)
    {
        var vendor = await GetOrCreateVendorAsync(__userId, __cancellationToken);

        // Self-service is deliberately narrower than the admin PUT /vendors/{id}: a vendor may
        // correct its own contact details, but VendorType/IsActive stay admin-controlled since
        // they drive classification and activation decisions elsewhere in the system.
        vendor.Name = __request.Name;
        vendor.Email = __request.Email;
        vendor.Phone = __request.Phone;
        vendor.Address = __request.Address;
        vendor.City = __request.City;
        vendor.Country = __request.Country;
        vendor.UpdatedBy = __userId;

        await _vendorRepository.UpdateAsync(vendor, __cancellationToken);
        _logger.LogInformation("Vendor {VendorId} profile updated by its own user {UserId}", vendor.Id, __userId);
        try
        {
            await _auditLogWriter.LogAsync(__userId, "vendor.profile.updated_via_portal", "Vendor", vendor.Id.ToString(), null, vendor.Name, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "vendor.profile.updated_via_portal", "Vendor", vendor.Id);
        }
        return ToResponse(vendor);
    }

    public async Task<List<VendorRateResponse>> GetMyRatesAsync(Guid __userId, CancellationToken __cancellationToken)
    {
        var vendor = await GetOrCreateVendorAsync(__userId, __cancellationToken);
        var rates = await _vendorRepository.GetRatesAsync(vendor.Id, __cancellationToken);
        return rates.Select(ToRateResponse).ToList();
    }

    public async Task<PaginationResponse<VendorPaymentResponse>> GetMyPaymentsAsync(Guid __userId, PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var vendor = await GetOrCreateVendorAsync(__userId, __cancellationToken);
        var page = await _vendorRepository.GetPaymentsAsync(vendor.Id, __request, __cancellationToken);
        return PaginationResponse<VendorPaymentResponse>.Create(page.Items.Select(ToPaymentResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<PaginationResponse<VendorBookingRequestResponse>> GetMyBookingRequestsAsync(Guid __userId, PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var vendor = await GetOrCreateVendorAsync(__userId, __cancellationToken);
        var page = await _vendorRepository.GetBookingRequestsAsync(vendor.Id, __request, __cancellationToken);
        return PaginationResponse<VendorBookingRequestResponse>.Create(page.Items.Select(ToBookingRequestResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<VendorInvoiceResponse> SubmitInvoiceAsync(Guid __userId, Stream __content, string __fileName, string __contentType, decimal __amount, string? __notes, Guid? __bookingId, CancellationToken __cancellationToken)
    {
        var vendor = await GetOrCreateVendorAsync(__userId, __cancellationToken);
        var storagePath = await _fileStorageService.SaveAsync(__content, __fileName, __contentType, __cancellationToken);

        var invoice = new VendorInvoiceModel
        {
            Id = Guid.NewGuid(),
            VendorId = vendor.Id,
            BookingId = __bookingId,
            UploadedByUserId = __userId,
            FileName = __fileName,
            ContentType = __contentType,
            FileSizeBytes = __content.Length,
            StoragePath = storagePath,
            Amount = __amount,
            Notes = __notes,
            Status = "Pending",
            CreatedBy = __userId
        };
        await _vendorInvoiceRepository.CreateAsync(invoice, __cancellationToken);
        invoice.CreatedAt = DateTime.UtcNow;
        _logger.LogInformation("Vendor invoice {InvoiceId} submitted by vendor {VendorId} for {Amount}", invoice.Id, vendor.Id, __amount);
        try
        {
            await _auditLogWriter.LogAsync(__userId, "vendor.invoice.submitted", "VendorInvoice", invoice.Id.ToString(), null, __amount.ToString(), __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "vendor.invoice.submitted", "VendorInvoice", invoice.Id);
        }

        return ToInvoiceResponse(invoice);
    }

    public async Task<PaginationResponse<VendorInvoiceResponse>> GetMyInvoicesAsync(Guid __userId, PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var vendor = await GetOrCreateVendorAsync(__userId, __cancellationToken);
        var page = await _vendorInvoiceRepository.SearchByVendorIdAsync(vendor.Id, __request, __cancellationToken);
        return PaginationResponse<VendorInvoiceResponse>.Create(page.Items.Select(ToInvoiceResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    /// <summary>Resolves the Vendor linked to this user, auto-creating a minimal placeholder
    /// record from the User row if none exists yet (mirrors CustomerPortalAppFunction's
    /// GetOrCreateCustomerAsync). Vendors have no self-registration flow, so in practice this
    /// only fires for an account whose Vendor row an admin has not yet linked via
    /// POST /vendors/{id}/link-user — the placeholder defaults VendorType to "Hotel" (the CHECK
    /// constraint requires a concrete value and there is no neutral "Unknown" option) and leaves
    /// IsActive false so it will not be mistaken for a fully onboarded vendor until an admin
    /// reviews and corrects it.</summary>
    private async Task<VendorModel> GetOrCreateVendorAsync(Guid __userId, CancellationToken __cancellationToken)
    {
        var existing = await _vendorRepository.GetByUserIdAsync(__userId, __cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var user = await _userRepository.GetByIdAsync(__userId, __cancellationToken) ?? throw new EntityNotFoundException("User", __userId);
        var vendor = new VendorModel
        {
            Id = Guid.NewGuid(),
            Name = user.FullName,
            VendorType = "Hotel",
            Email = user.Email,
            Phone = string.Empty,
            UserId = user.Id,
            IsActive = false,
            CreatedBy = user.Id
        };

        await _vendorRepository.CreateAsync(vendor, __cancellationToken);
        return vendor;
    }

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

    private static VendorBookingRequestResponse ToBookingRequestResponse(VendorBookingRequestModel request) => new()
    {
        BookingId = request.BookingId,
        BookingNumber = request.BookingNumber,
        Status = request.Status,
        TravelDate = request.TravelDate,
        ReturnDate = request.ReturnDate,
        NumberOfAdults = request.NumberOfAdults,
        NumberOfChildren = request.NumberOfChildren,
        DestinationId = request.DestinationId,
        DestinationName = request.DestinationName
    };
}
