using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.AppFunctions;

public sealed class EnquiryAppFunction : IEnquiryAppFunction
{
    private readonly IEnquiryRepository _enquiryRepository;
    private readonly IDestinationRepository _destinationRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAuditLogWriter _auditLogWriter;

    public EnquiryAppFunction(
        IEnquiryRepository __enquiryRepository,
        IDestinationRepository __destinationRepository,
        ICurrentUserAccessor __currentUserAccessor,
        IAuditLogWriter __auditLogWriter)
    {
        _enquiryRepository = __enquiryRepository;
        _destinationRepository = __destinationRepository;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
    }

    public async Task<PaginationResponse<EnquiryResponse>> SearchAsync(EnquirySearchRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _enquiryRepository.SearchAsync(__request, __cancellationToken);
        return PaginationResponse<EnquiryResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<EnquiryResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var enquiry = await _enquiryRepository.GetByIdAsync(__id, __cancellationToken)
            ?? throw new EntityNotFoundException("Enquiry", __id);

        return ToResponse(enquiry);
    }

    public async Task<EnquiryResponse> CreateAsync(EnquiryRequest __request, CancellationToken __cancellationToken)
    {
        if (__request.DestinationId is { } destinationId)
        {
            _ = await _destinationRepository.GetByIdAsync(destinationId, __cancellationToken)
                ?? throw new EntityNotFoundException("Destination", destinationId);
        }

        var enquiry = new EnquiryModel
        {
            Id = Guid.NewGuid(),
            FullName = __request.FullName,
            Email = __request.Email,
            Phone = __request.Phone,
            DestinationId = __request.DestinationId,
            TravelDate = __request.TravelDate,
            Message = __request.Message,
            Status = "New",
            CreatedBy = _currentUserAccessor.UserId
        };

        await _enquiryRepository.CreateAsync(enquiry, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "enquiry.created", "Enquiry", enquiry.Id.ToString(), null, enquiry.FullName, __cancellationToken);

        var created = await _enquiryRepository.GetByIdAsync(enquiry.Id, __cancellationToken) ?? enquiry;
        return ToResponse(created);
    }

    public async Task<EnquiryResponse> UpdateStatusAsync(Guid __id, EnquiryStatusRequest __request, CancellationToken __cancellationToken)
    {
        var enquiry = await _enquiryRepository.GetByIdAsync(__id, __cancellationToken)
            ?? throw new EntityNotFoundException("Enquiry", __id);

        var oldStatus = enquiry.Status;
        await _enquiryRepository.UpdateStatusAsync(__id, __request.Status, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "enquiry.status.changed", "Enquiry", __id.ToString(), oldStatus, __request.Status, __cancellationToken);

        var updated = await _enquiryRepository.GetByIdAsync(__id, __cancellationToken) ?? enquiry;
        return ToResponse(updated);
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var enquiry = await _enquiryRepository.GetByIdAsync(__id, __cancellationToken)
            ?? throw new EntityNotFoundException("Enquiry", __id);

        await _enquiryRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "enquiry.deleted", "Enquiry", __id.ToString(), enquiry.FullName, null, __cancellationToken);
    }

    private static EnquiryResponse ToResponse(EnquiryModel enquiry) => new()
    {
        Id = enquiry.Id,
        FullName = enquiry.FullName,
        Email = enquiry.Email,
        Phone = enquiry.Phone,
        DestinationId = enquiry.DestinationId,
        DestinationName = enquiry.DestinationName,
        TravelDate = enquiry.TravelDate,
        Message = enquiry.Message ?? string.Empty,
        Status = enquiry.Status,
        CreatedAt = enquiry.CreatedAt
    };
}
