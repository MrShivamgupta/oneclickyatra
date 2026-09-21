using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.AppFunctions;

public sealed class CustomerAppFunction : ICustomerAppFunction
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ILogger<CustomerAppFunction> _logger;

    public CustomerAppFunction(
        ICustomerRepository __customerRepository,
        ICurrentUserAccessor __currentUserAccessor,
        IAuditLogWriter __auditLogWriter,
        ILogger<CustomerAppFunction> __logger)
    {
        _customerRepository = __customerRepository;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
        _logger = __logger;
    }

    public async Task<PaginationResponse<CustomerResponse>> ListAsync(PaginationRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _customerRepository.ListAsync(__request, __cancellationToken);
        return PaginationResponse<CustomerResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<CustomerResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Customer", __id);
        return ToResponse(customer);
    }

    public async Task<CustomerResponse> CreateAsync(CustomerRequest __request, CancellationToken __cancellationToken)
    {
        var customer = new CustomerModel
        {
            Id = Guid.NewGuid(),
            FullName = __request.FullName,
            Email = __request.Email,
            Phone = __request.Phone,
            CreatedBy = _currentUserAccessor.UserId
        };

        await _customerRepository.CreateAsync(customer, __cancellationToken);
        _logger.LogInformation("Customer {CustomerId} created by {UserId}", customer.Id, _currentUserAccessor.UserId);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "customer.created", "Customer", customer.Id.ToString(), null, customer.FullName, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "customer.created", "Customer", customer.Id);
        }

        return ToResponse(customer);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid __id, CustomerRequest __request, CancellationToken __cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Customer", __id);

        var oldName = customer.FullName;
        customer.FullName = __request.FullName;
        customer.Email = __request.Email;
        customer.Phone = __request.Phone;
        customer.UpdatedBy = _currentUserAccessor.UserId;

        await _customerRepository.UpdateAsync(customer, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "customer.updated", "Customer", customer.Id.ToString(), oldName, customer.FullName, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "customer.updated", "Customer", customer.Id);
        }

        return ToResponse(customer);
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Customer", __id);

        await _customerRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);

        try
        {
            await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "customer.deleted", "Customer", __id.ToString(), customer.FullName, null, __cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write audit log for {Action} on {EntityType} {EntityId}", "customer.deleted", "Customer", __id);
        }
    }

    private static CustomerResponse ToResponse(CustomerModel customer) => new()
    {
        Id = customer.Id,
        FullName = customer.FullName,
        Email = customer.Email,
        Phone = customer.Phone,
        UserId = customer.UserId
    };
}
