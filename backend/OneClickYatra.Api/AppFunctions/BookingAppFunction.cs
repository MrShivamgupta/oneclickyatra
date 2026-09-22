using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;
using OneClickYatra.Api.Services.Email;

namespace OneClickYatra.Api.AppFunctions;

public sealed class BookingAppFunction : IBookingAppFunction
{
    /// <summary>
    /// The booking state machine (SRS §7): Draft → Quoted → PendingPayment → Confirmed → InProgress → Completed,
    /// with Cancelled reachable from any pre-InProgress state (a booking that hasn't traveled yet can always be
    /// called off) and a RefundPending → Refunded branch off Confirmed or Cancelled (money already collected).
    /// This is the one and only place transitions are decided — every other method goes through
    /// <see cref="TransitionAsync"/> rather than writing Status directly.
    /// </summary>
    private static readonly Dictionary<string, string[]> AllowedTransitions = new()
    {
        ["Draft"] = ["Quoted", "Cancelled"],
        ["Quoted"] = ["PendingPayment", "Cancelled"],
        ["PendingPayment"] = ["Confirmed", "Cancelled"],
        ["Confirmed"] = ["InProgress", "Cancelled", "RefundPending"],
        ["InProgress"] = ["Completed"],
        ["Completed"] = [],
        ["Cancelled"] = ["RefundPending"],
        ["RefundPending"] = ["Refunded"],
        ["Refunded"] = []
    };

    private static readonly string[] EditableStatuses = ["Draft", "Quoted", "PendingPayment", "Confirmed", "InProgress"];

    /// <summary>No agency-settings domain exists yet in this codebase (Settings is still a
    /// deferred Phase 7 placeholder) to source a real support contact number from, so every email
    /// trigger uses this one shared, clearly-a-placeholder value for the {{SupportPhone}} token.</summary>
    private const string SupportPhonePlaceholder = "+91-11-4567-8900";

    private readonly IBookingRepository _bookingRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IDestinationRepository _destinationRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IQuotationRepository _quotationRepository;
    private readonly ILeadAppFunction _leadAppFunction;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ITrackingIdAccessor _trackingIdAccessor;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly IEmailNotificationSender _emailNotificationSender;
    private readonly ILogger<BookingAppFunction> _logger;

    public BookingAppFunction(
        IBookingRepository __bookingRepository,
        ICustomerRepository __customerRepository,
        ILeadRepository __leadRepository,
        IDestinationRepository __destinationRepository,
        IPackageRepository __packageRepository,
        IQuotationRepository __quotationRepository,
        ILeadAppFunction __leadAppFunction,
        ICurrentUserAccessor __currentUserAccessor,
        ITrackingIdAccessor __trackingIdAccessor,
        IAuditLogWriter __auditLogWriter,
        IEmailNotificationSender __emailNotificationSender,
        ILogger<BookingAppFunction> __logger)
    {
        _bookingRepository = __bookingRepository;
        _customerRepository = __customerRepository;
        _leadRepository = __leadRepository;
        _destinationRepository = __destinationRepository;
        _packageRepository = __packageRepository;
        _quotationRepository = __quotationRepository;
        _leadAppFunction = __leadAppFunction;
        _currentUserAccessor = __currentUserAccessor;
        _trackingIdAccessor = __trackingIdAccessor;
        _auditLogWriter = __auditLogWriter;
        _emailNotificationSender = __emailNotificationSender;
        _logger = __logger;
    }

    public async Task<PaginationResponse<BookingResponse>> SearchAsync(BookingSearchRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _bookingRepository.SearchAsync(__request, __cancellationToken);
        return PaginationResponse<BookingResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<List<BookingResponse>> GetUpcomingDeparturesAsync(int __withinDays, int __top, CancellationToken __cancellationToken)
    {
        var bookings = await _bookingRepository.GetUpcomingDeparturesAsync(__withinDays, __top, __cancellationToken);
        return bookings.Select(ToResponse).ToList();
    }

    public async Task<BookingDetailResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Booking", __id);
        var passengers = await _bookingRepository.GetPassengersAsync(__id, __cancellationToken);
        var addOns = await _bookingRepository.GetAddOnsAsync(__id, __cancellationToken);

        return new BookingDetailResponse
        {
            Booking = ToResponse(booking),
            Passengers = passengers.Select(ToPassengerResponse).ToList(),
            AddOns = addOns.Select(ToAddOnResponse).ToList()
        };
    }

    public async Task<BookingResponse> CreateAsync(BookingRequest __request, CancellationToken __cancellationToken)
    {
        _ = await _customerRepository.GetByIdAsync(__request.CustomerId, __cancellationToken)
            ?? throw new EntityNotFoundException("Customer", __request.CustomerId);
        await ValidateOptionalReferencesAsync(__request.LeadId, __request.PackageId, __request.DestinationId, __cancellationToken);

        var booking = new BookingModel
        {
            Id = Guid.NewGuid(),
            BookingNumber = GenerateBookingNumber(),
            LeadId = __request.LeadId,
            CustomerId = __request.CustomerId,
            PackageId = __request.PackageId,
            DestinationId = __request.DestinationId,
            TravelDate = __request.TravelDate,
            ReturnDate = __request.ReturnDate,
            NumberOfAdults = __request.NumberOfAdults,
            NumberOfChildren = __request.NumberOfChildren,
            TotalAmount = __request.TotalAmount,
            Notes = __request.Notes,
            Status = "Draft",
            CreatedBy = _currentUserAccessor.UserId
        };

        await _bookingRepository.CreateAsync(booking, __cancellationToken);
        await WriteHistoryAsync(booking.Id, null, "Draft", "Booking created.", __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "booking.created", "Booking", booking.Id.ToString(), null, booking.BookingNumber, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "booking.created", "Booking", booking.Id);
        }

        _logger.LogInformation("Booking {BookingId} {BookingNumber} created by {UserId}", booking.Id, booking.BookingNumber, _currentUserAccessor.UserId);

        var saved = await _bookingRepository.GetByIdAsync(booking.Id, __cancellationToken) ?? booking;
        return ToResponse(saved);
    }

    public async Task<BookingResponse> UpdateAsync(Guid __id, BookingRequest __request, CancellationToken __cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Booking", __id);
        EnsureEditable(booking);
        await ValidateOptionalReferencesAsync(__request.LeadId, __request.PackageId, __request.DestinationId, __cancellationToken);

        booking.PackageId = __request.PackageId;
        booking.DestinationId = __request.DestinationId;
        booking.TravelDate = __request.TravelDate;
        booking.ReturnDate = __request.ReturnDate;
        booking.NumberOfAdults = __request.NumberOfAdults;
        booking.NumberOfChildren = __request.NumberOfChildren;
        booking.TotalAmount = __request.TotalAmount;
        booking.Notes = __request.Notes;
        booking.UpdatedBy = _currentUserAccessor.UserId;

        await _bookingRepository.UpdateAsync(booking, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "booking.updated", "Booking", booking.Id.ToString(), null, booking.BookingNumber, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "booking.updated", "Booking", booking.Id);
        }

        return ToResponse(booking);
    }

    public async Task<BookingResponse> UpdateStatusAsync(Guid __id, BookingStatusRequest __request, CancellationToken __cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Booking", __id);
        await TransitionAsync(booking, __request.Status, __request.Reason, __cancellationToken);
        return ToResponse(booking);
    }

    public async Task<BookingResponse> CancelAsync(Guid __id, BookingCancelRequest __request, CancellationToken __cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Booking", __id);
        await TransitionAsync(booking, "Cancelled", __request.Reason, __cancellationToken);
        return ToResponse(booking);
    }

    public async Task<BookingResponse> InitiateRefundAsync(Guid __id, BookingRefundRequest __request, CancellationToken __cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Booking", __id);
        await TransitionAsync(booking, "RefundPending", __request.Reason, __cancellationToken);
        return ToResponse(booking);
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Booking", __id);
        if (booking.Status != "Draft")
        {
            throw new BusinessException("Only a Draft booking can be deleted. Cancel it instead.");
        }

        await _bookingRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "booking.deleted", "Booking", __id.ToString(), booking.BookingNumber, null, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "booking.deleted", "Booking", __id);
        }

        _logger.LogInformation("Booking {BookingId} {BookingNumber} deleted by {UserId}", __id, booking.BookingNumber, _currentUserAccessor.UserId);
    }

    public async Task<List<BookingPassengerResponse>> ReplacePassengersAsync(Guid __id, List<BookingPassengerRequest> __passengers, CancellationToken __cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Booking", __id);
        EnsureEditable(booking);

        var models = __passengers.Select(p => new BookingPassengerModel
        {
            Id = Guid.NewGuid(),
            BookingId = __id,
            FullName = p.FullName,
            Age = p.Age,
            Gender = p.Gender,
            IdProofType = p.IdProofType,
            IdProofNumber = p.IdProofNumber,
            IsLeadPassenger = p.IsLeadPassenger
        }).ToList();

        await _bookingRepository.ReplacePassengersAsync(__id, models, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "booking.passengers_updated", "Booking", __id.ToString(), null, $"{models.Count} passenger(s)", __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "booking.passengers_updated", "Booking", __id);
        }

        return models.Select(ToPassengerResponse).ToList();
    }

    public async Task<List<BookingAddOnResponse>> ReplaceAddOnsAsync(Guid __id, List<BookingAddOnRequest> __addOns, CancellationToken __cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Booking", __id);
        EnsureEditable(booking);

        var models = __addOns.Select(a => new BookingAddOnModel
        {
            Id = Guid.NewGuid(),
            BookingId = __id,
            Name = a.Name,
            Description = a.Description,
            Price = a.Price,
            Quantity = a.Quantity
        }).ToList();

        await _bookingRepository.ReplaceAddOnsAsync(__id, models, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "booking.addons_updated", "Booking", __id.ToString(), null, $"{models.Count} add-on(s)", __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "booking.addons_updated", "Booking", __id);
        }

        return models.Select(ToAddOnResponse).ToList();
    }

    public async Task<List<BookingStatusHistoryResponse>> GetStatusHistoryAsync(Guid __id, CancellationToken __cancellationToken)
    {
        _ = await _bookingRepository.GetByIdAsync(__id, __cancellationToken)
            ?? throw new EntityNotFoundException("Booking", __id);

        var history = await _bookingRepository.GetStatusHistoryAsync(__id, __cancellationToken);
        return history.Select(h => new BookingStatusHistoryResponse
        {
            Id = h.Id,
            OldStatus = h.OldStatus,
            NewStatus = h.NewStatus,
            ChangedByName = h.ChangedByName,
            ChangedAt = h.ChangedAt,
            Reason = h.Reason
        }).ToList();
    }

    public async Task<BookingResponse> ConvertFromQuotationAsync(Guid __quotationId, CancellationToken __cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdAsync(__quotationId, __cancellationToken) ?? throw new EntityNotFoundException("Quotation", __quotationId);
        if (quotation.Status != "Approved")
        {
            throw new BusinessException("Only an Approved quotation can be converted to a booking.");
        }
        if (quotation.SelectedOptionId is null)
        {
            throw new BusinessException("This quotation has no approved option to convert.");
        }

        var option = await _quotationRepository.GetOptionByIdAsync(quotation.SelectedOptionId.Value, __cancellationToken)
            ?? throw new EntityNotFoundException("QuotationOption", quotation.SelectedOptionId.Value);

        var customerId = quotation.CustomerId;
        if (customerId is null)
        {
            var customer = await _leadAppFunction.ConvertToCustomerAsync(quotation.LeadId, __cancellationToken);
            customerId = customer.Id;
        }

        var booking = new BookingModel
        {
            Id = Guid.NewGuid(),
            BookingNumber = GenerateBookingNumber(),
            LeadId = quotation.LeadId,
            CustomerId = customerId.Value,
            QuotationId = quotation.Id,
            QuotationOptionId = option.Id,
            PackageId = option.PackageId,
            DestinationId = option.DestinationId,
            NumberOfAdults = option.NumberOfPeople,
            NumberOfChildren = 0,
            TotalAmount = option.TotalPrice,
            Notes = $"Created from quotation {quotation.QuotationNumber} ({option.OptionName}).",
            Status = "Quoted",
            CreatedBy = _currentUserAccessor.UserId
        };

        await _bookingRepository.CreateAsync(booking, __cancellationToken);
        await WriteHistoryAsync(booking.Id, null, "Quoted", $"Converted from quotation {quotation.QuotationNumber}.", __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "booking.created_from_quotation", "Booking", booking.Id.ToString(), quotation.QuotationNumber, booking.BookingNumber, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "booking.created_from_quotation", "Booking", booking.Id);
        }

        _logger.LogInformation("Booking {BookingId} {BookingNumber} created from quotation {QuotationId} by {UserId}", booking.Id, booking.BookingNumber, quotation.Id, _currentUserAccessor.UserId);

        await _quotationRepository.UpdateStatusAsync(quotation.Id, "Converted", _currentUserAccessor.UserId, __cancellationToken);

        var saved = await _bookingRepository.GetByIdAsync(booking.Id, __cancellationToken) ?? booking;
        return ToResponse(saved);
    }

    private async Task TransitionAsync(BookingModel __booking, string __newStatus, string? __reason, CancellationToken __cancellationToken)
    {
        if (!AllowedTransitions.TryGetValue(__booking.Status, out var allowed) || !allowed.Contains(__newStatus))
        {
            throw new BusinessException($"Cannot move a booking from '{__booking.Status}' to '{__newStatus}'.");
        }

        var oldStatus = __booking.Status;
        await _bookingRepository.UpdateStatusAsync(__booking.Id, __newStatus, _currentUserAccessor.UserId, __cancellationToken);

        _logger.LogInformation("Booking {BookingId} transitioned {OldStatus} -> {NewStatus} by {UserId}", __booking.Id, oldStatus, __newStatus, _currentUserAccessor.UserId);

        if (__newStatus == "Cancelled")
        {
            await _bookingRepository.SetCancellationReasonAsync(__booking.Id, __reason, __cancellationToken);
            __booking.CancellationReason = __reason;
        }

        await WriteHistoryAsync(__booking.Id, oldStatus, __newStatus, __reason, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "booking.status.changed", "Booking", __booking.Id.ToString(), oldStatus, __newStatus, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "booking.status.changed", "Booking", __booking.Id);
        }

        if (__newStatus is "Confirmed" or "Cancelled")
        {
            await SendTransitionEmailAsync(__booking, __newStatus, __reason, __cancellationToken);
        }

        __booking.Status = __newStatus;
    }

    /// <summary>Best-effort — the status transition above has already committed, so a failed email
    /// (no customer email on file, SMTP not configured, gateway error) must never mask that.</summary>
    private async Task SendTransitionEmailAsync(BookingModel __booking, string __newStatus, string? __reason, CancellationToken __cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(__booking.CustomerId, __cancellationToken);
            if (string.IsNullOrWhiteSpace(customer?.Email))
            {
                _logger.LogWarning("Skipped booking transition email for Booking {BookingId}: no email address on file for Customer {CustomerId}", __booking.Id, __booking.CustomerId);
                return;
            }

            var packageName = __booking.PackageId.HasValue
                ? (await _packageRepository.GetByIdAsync(__booking.PackageId.Value, __cancellationToken))?.Title
                : null;

            if (__newStatus == "Confirmed")
            {
                var placeholders = new Dictionary<string, string>
                {
                    ["BookingReference"] = __booking.BookingNumber,
                    ["CustomerName"] = customer.FullName,
                    ["PackageName"] = packageName ?? "your travel package",
                    ["TravelStartDate"] = __booking.TravelDate?.ToString("dd MMM yyyy") ?? "to be confirmed",
                    ["TravelEndDate"] = __booking.ReturnDate?.ToString("dd MMM yyyy") ?? "to be confirmed",
                    ["TravelerCount"] = (__booking.NumberOfAdults + __booking.NumberOfChildren).ToString(),
                    ["TotalAmount"] = "Rs. " + __booking.TotalAmount.ToString("N2"),
                    ["SupportPhone"] = SupportPhonePlaceholder
                };
                await _emailNotificationSender.SendAsync("BookingConfirmation", customer.Email, placeholders, __cancellationToken);
            }
            else
            {
                var placeholders = new Dictionary<string, string>
                {
                    ["BookingReference"] = __booking.BookingNumber,
                    ["CustomerName"] = customer.FullName,
                    ["PackageName"] = packageName ?? "your travel package",
                    ["CancellationReason"] = __reason ?? "Not specified",
                    ["RefundAmount"] = "Rs. " + __booking.AmountPaid.ToString("N2"),
                    ["SupportPhone"] = SupportPhonePlaceholder
                };
                await _emailNotificationSender.SendAsync("BookingCancellation", customer.Email, placeholders, __cancellationToken);
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to send {NewStatus} email for Booking {BookingId}", __newStatus, __booking.Id);
        }
    }

    private async Task WriteHistoryAsync(Guid __bookingId, string? __oldStatus, string __newStatus, string? __reason, CancellationToken __cancellationToken)
    {
        await _bookingRepository.AddStatusHistoryAsync(new BookingStatusHistoryModel
        {
            Id = Guid.NewGuid(),
            BookingId = __bookingId,
            OldStatus = __oldStatus,
            NewStatus = __newStatus,
            ChangedBy = _currentUserAccessor.UserId,
            Reason = __reason,
            TrackingId = _trackingIdAccessor.TrackingId
        }, __cancellationToken);
    }

    private async Task ValidateOptionalReferencesAsync(Guid? __leadId, Guid? __packageId, Guid? __destinationId, CancellationToken __cancellationToken)
    {
        if (__leadId.HasValue && await _leadRepository.GetByIdAsync(__leadId.Value, __cancellationToken) is null)
        {
            throw new EntityNotFoundException("Lead", __leadId.Value);
        }
        if (__packageId.HasValue && await _packageRepository.GetByIdAsync(__packageId.Value, __cancellationToken) is null)
        {
            throw new EntityNotFoundException("Package", __packageId.Value);
        }
        if (__destinationId.HasValue && await _destinationRepository.GetByIdAsync(__destinationId.Value, __cancellationToken) is null)
        {
            throw new EntityNotFoundException("Destination", __destinationId.Value);
        }
    }

    private static void EnsureEditable(BookingModel __booking)
    {
        if (!EditableStatuses.Contains(__booking.Status))
        {
            throw new BusinessException($"A booking in '{__booking.Status}' status can no longer be edited.");
        }
    }

    private static string GenerateBookingNumber()
        => $"BK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

    private static BookingResponse ToResponse(BookingModel booking) => new()
    {
        Id = booking.Id,
        BookingNumber = booking.BookingNumber,
        LeadId = booking.LeadId,
        CustomerId = booking.CustomerId,
        CustomerName = booking.CustomerName,
        QuotationId = booking.QuotationId,
        QuotationOptionId = booking.QuotationOptionId,
        PackageId = booking.PackageId,
        PackageTitle = booking.PackageTitle,
        DestinationId = booking.DestinationId,
        DestinationName = booking.DestinationName,
        DestinationImageUrl = booking.DestinationImageUrl,
        TravelDate = booking.TravelDate,
        ReturnDate = booking.ReturnDate,
        NumberOfAdults = booking.NumberOfAdults,
        NumberOfChildren = booking.NumberOfChildren,
        TotalAmount = booking.TotalAmount,
        AmountPaid = booking.AmountPaid,
        Notes = booking.Notes,
        Status = booking.Status,
        CancellationReason = booking.CancellationReason
    };

    private static BookingPassengerResponse ToPassengerResponse(BookingPassengerModel passenger) => new()
    {
        Id = passenger.Id,
        FullName = passenger.FullName,
        Age = passenger.Age,
        Gender = passenger.Gender,
        IdProofType = passenger.IdProofType,
        IdProofNumber = passenger.IdProofNumber,
        IsLeadPassenger = passenger.IsLeadPassenger
    };

    private static BookingAddOnResponse ToAddOnResponse(BookingAddOnModel addOn) => new()
    {
        Id = addOn.Id,
        Name = addOn.Name,
        Description = addOn.Description,
        Price = addOn.Price,
        Quantity = addOn.Quantity
    };
}
