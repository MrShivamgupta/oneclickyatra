using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.AppFunctions;

public sealed class LeadAppFunction : ILeadAppFunction
{
    private readonly ILeadRepository _leadRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IDestinationRepository _destinationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAuditLogWriter _auditLogWriter;

    public LeadAppFunction(
        ILeadRepository __leadRepository,
        ICustomerRepository __customerRepository,
        IDestinationRepository __destinationRepository,
        IUserRepository __userRepository,
        ICurrentUserAccessor __currentUserAccessor,
        IAuditLogWriter __auditLogWriter)
    {
        _leadRepository = __leadRepository;
        _customerRepository = __customerRepository;
        _destinationRepository = __destinationRepository;
        _userRepository = __userRepository;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
    }

    public async Task<PaginationResponse<LeadResponse>> SearchAsync(LeadSearchRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _leadRepository.SearchAsync(__request, __cancellationToken);
        return PaginationResponse<LeadResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<LeadResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Lead", __id);
        return ToResponse(lead);
    }

    public async Task<LeadResponse> CreateAsync(LeadRequest __request, CancellationToken __cancellationToken)
    {
        await ValidateReferencesAsync(__request, __cancellationToken);

        var lead = new LeadModel
        {
            Id = Guid.NewGuid(),
            CustomerName = __request.CustomerName,
            Mobile = __request.Mobile,
            Email = __request.Email,
            DestinationId = __request.DestinationId,
            TravelDate = __request.TravelDate,
            Budget = __request.Budget,
            Source = __request.Source,
            AssignedToUserId = __request.AssignedToUserId,
            LeadScore = 0,
            Status = "New",
            CreatedBy = _currentUserAccessor.UserId
        };

        await _leadRepository.CreateAsync(lead, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "lead.created", "Lead", lead.Id.ToString(), null, lead.CustomerName, __cancellationToken);

        return await GetByIdAsync(lead.Id, __cancellationToken);
    }

    public async Task<LeadResponse> UpdateAsync(Guid __id, LeadRequest __request, CancellationToken __cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Lead", __id);
        await ValidateReferencesAsync(__request, __cancellationToken);

        var oldName = lead.CustomerName;
        lead.CustomerName = __request.CustomerName;
        lead.Mobile = __request.Mobile;
        lead.Email = __request.Email;
        lead.DestinationId = __request.DestinationId;
        lead.TravelDate = __request.TravelDate;
        lead.Budget = __request.Budget;
        lead.Source = __request.Source;
        lead.AssignedToUserId = __request.AssignedToUserId;
        lead.UpdatedBy = _currentUserAccessor.UserId;

        await _leadRepository.UpdateAsync(lead, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "lead.updated", "Lead", lead.Id.ToString(), oldName, lead.CustomerName, __cancellationToken);

        return await GetByIdAsync(__id, __cancellationToken);
    }

    public async Task<LeadResponse> UpdateStatusAsync(Guid __id, LeadStatusRequest __request, CancellationToken __cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Lead", __id);

        var oldStatus = lead.Status;
        await _leadRepository.UpdateStatusAsync(__id, __request.Status, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "lead.status.changed", "Lead", __id.ToString(), oldStatus, __request.Status, __cancellationToken);

        return await GetByIdAsync(__id, __cancellationToken);
    }

    public async Task<LeadResponse> AssignAsync(Guid __id, LeadAssignRequest __request, CancellationToken __cancellationToken)
    {
        _ = await _leadRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Lead", __id);
        _ = await _userRepository.GetByIdAsync(__request.AssignedToUserId, __cancellationToken)
            ?? throw new EntityNotFoundException("User", __request.AssignedToUserId);

        await _leadRepository.AssignAsync(__id, __request.AssignedToUserId, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "lead.assigned", "Lead", __id.ToString(), null, __request.AssignedToUserId.ToString(), __cancellationToken);

        return await GetByIdAsync(__id, __cancellationToken);
    }

    public async Task<LeadResponse> UpdateScoreAsync(Guid __id, LeadScoreRequest __request, CancellationToken __cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Lead", __id);

        await _leadRepository.UpdateScoreAsync(__id, __request.LeadScore, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "lead.score.changed", "Lead", __id.ToString(), lead.LeadScore.ToString(), __request.LeadScore.ToString(), __cancellationToken);

        return await GetByIdAsync(__id, __cancellationToken);
    }

    public async Task<CustomerResponse> ConvertToCustomerAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Lead", __id);

        if (lead.CustomerId is { } existingCustomerId)
        {
            var existingCustomer = await _customerRepository.GetByIdAsync(existingCustomerId, __cancellationToken)
                ?? throw new EntityNotFoundException("Customer", existingCustomerId);
            return new CustomerResponse
            {
                Id = existingCustomer.Id,
                FullName = existingCustomer.FullName,
                Email = existingCustomer.Email,
                Phone = existingCustomer.Phone,
                UserId = existingCustomer.UserId
            };
        }

        var customer = new CustomerModel
        {
            Id = Guid.NewGuid(),
            FullName = lead.CustomerName,
            Email = lead.Email,
            Phone = lead.Mobile,
            CreatedBy = _currentUserAccessor.UserId
        };

        await _customerRepository.CreateAsync(customer, __cancellationToken);
        await _leadRepository.LinkCustomerAsync(__id, customer.Id, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "lead.converted", "Lead", __id.ToString(), null, customer.Id.ToString(), __cancellationToken);

        return new CustomerResponse
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            UserId = customer.UserId
        };
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Lead", __id);

        await _leadRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "lead.deleted", "Lead", __id.ToString(), lead.CustomerName, null, __cancellationToken);
    }

    private async Task ValidateReferencesAsync(LeadRequest __request, CancellationToken __cancellationToken)
    {
        if (__request.DestinationId is { } destinationId)
        {
            _ = await _destinationRepository.GetByIdAsync(destinationId, __cancellationToken)
                ?? throw new EntityNotFoundException("Destination", destinationId);
        }

        if (__request.AssignedToUserId is { } assignedToUserId)
        {
            _ = await _userRepository.GetByIdAsync(assignedToUserId, __cancellationToken)
                ?? throw new EntityNotFoundException("User", assignedToUserId);
        }
    }

    private static LeadResponse ToResponse(LeadModel lead) => new()
    {
        Id = lead.Id,
        CustomerName = lead.CustomerName,
        Mobile = lead.Mobile,
        Email = lead.Email,
        DestinationId = lead.DestinationId,
        DestinationName = lead.DestinationName,
        TravelDate = lead.TravelDate,
        Budget = lead.Budget,
        Source = lead.Source,
        AssignedToUserId = lead.AssignedToUserId,
        AssignedToName = lead.AssignedToName,
        LeadScore = lead.LeadScore,
        Status = lead.Status,
        CustomerId = lead.CustomerId
    };
}
