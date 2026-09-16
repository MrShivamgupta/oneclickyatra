using System.Security.Cryptography;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;
using OneClickYatra.Api.Models.Responses;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Security;
using OneClickYatra.Api.Services;

namespace OneClickYatra.Api.AppFunctions;

public sealed class QuotationAppFunction : IQuotationAppFunction
{
    private static readonly string[] EditableStatuses = ["Draft", "Sent", "Expired"];
    private static readonly string[] DeletableStatuses = ["Draft", "Rejected", "Expired"];

    private readonly IQuotationRepository _quotationRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IDestinationRepository _destinationRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IQuotationPdfService _pdfService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IAuditLogWriter _auditLogWriter;

    public QuotationAppFunction(
        IQuotationRepository __quotationRepository,
        ILeadRepository __leadRepository,
        IDestinationRepository __destinationRepository,
        IPackageRepository __packageRepository,
        IQuotationPdfService __pdfService,
        ICurrentUserAccessor __currentUserAccessor,
        IAuditLogWriter __auditLogWriter)
    {
        _quotationRepository = __quotationRepository;
        _leadRepository = __leadRepository;
        _destinationRepository = __destinationRepository;
        _packageRepository = __packageRepository;
        _pdfService = __pdfService;
        _currentUserAccessor = __currentUserAccessor;
        _auditLogWriter = __auditLogWriter;
    }

    public async Task<PaginationResponse<QuotationResponse>> SearchAsync(QuotationSearchRequest __request, CancellationToken __cancellationToken)
    {
        var page = await _quotationRepository.SearchAsync(__request, __cancellationToken);
        return PaginationResponse<QuotationResponse>.Create(page.Items.Select(ToResponse).ToList(), page.PageNumber, page.PageSize, page.TotalCount);
    }

    public async Task<QuotationDetailResponse> GetByIdAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Quotation", __id);
        var options = await LoadOptionResponsesAsync(quotation.Id, __cancellationToken);
        return new QuotationDetailResponse { Quotation = ToResponse(quotation), Options = options };
    }

    public async Task<QuotationResponse> CreateAsync(QuotationRequest __request, CancellationToken __cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(__request.LeadId, __cancellationToken) ?? throw new EntityNotFoundException("Lead", __request.LeadId);

        var (rawToken, tokenHash) = GeneratePublicToken();
        var quotation = new QuotationModel
        {
            Id = Guid.NewGuid(),
            QuotationNumber = GenerateQuotationNumber(),
            LeadId = lead.Id,
            CustomerId = lead.CustomerId,
            Title = __request.Title,
            Status = "Draft",
            ValidUntil = __request.ValidUntil,
            Notes = __request.Notes,
            PublicTokenHash = tokenHash,
            CreatedBy = _currentUserAccessor.UserId
        };

        await _quotationRepository.CreateAsync(quotation, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "quotation.created", "Quotation", quotation.Id.ToString(), null, quotation.QuotationNumber, __cancellationToken);

        var saved = await _quotationRepository.GetByIdAsync(quotation.Id, __cancellationToken) ?? quotation;
        var response = ToResponse(saved);
        response.PublicToken = rawToken;
        return response;
    }

    public async Task<QuotationResponse> UpdateAsync(Guid __id, QuotationRequest __request, CancellationToken __cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Quotation", __id);
        EnsureEditable(quotation);

        quotation.Title = __request.Title;
        quotation.ValidUntil = __request.ValidUntil;
        quotation.Notes = __request.Notes;
        quotation.UpdatedBy = _currentUserAccessor.UserId;

        await _quotationRepository.UpdateAsync(quotation, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "quotation.updated", "Quotation", quotation.Id.ToString(), null, quotation.Title, __cancellationToken);

        return ToResponse(quotation);
    }

    public async Task<QuotationResponse> RegenerateLinkAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Quotation", __id);

        var (rawToken, tokenHash) = GeneratePublicToken();
        await _quotationRepository.UpdatePublicTokenHashAsync(__id, tokenHash, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "quotation.link_regenerated", "Quotation", __id.ToString(), null, null, __cancellationToken);

        quotation.PublicTokenHash = tokenHash;
        var response = ToResponse(quotation);
        response.PublicToken = rawToken;
        return response;
    }

    public async Task<QuotationResponse> SendAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Quotation", __id);

        if (quotation.Status != "Draft")
        {
            throw new BusinessException("Only a Draft quotation can be sent.");
        }

        var options = await _quotationRepository.GetOptionsAsync(__id, __cancellationToken);
        if (options.Count == 0)
        {
            throw new BusinessException("Add at least one option before sending this quotation.");
        }

        await _quotationRepository.UpdateStatusAsync(__id, "Sent", _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "quotation.sent", "Quotation", __id.ToString(), "Draft", "Sent", __cancellationToken);

        quotation.Status = "Sent";
        return ToResponse(quotation);
    }

    public async Task<List<QuotationOptionResponse>> ReplaceOptionsAsync(Guid __id, List<QuotationOptionRequest> __options, CancellationToken __cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Quotation", __id);
        EnsureEditable(quotation);

        foreach (var option in __options)
        {
            if (option.DestinationId.HasValue && await _destinationRepository.GetByIdAsync(option.DestinationId.Value, __cancellationToken) is null)
            {
                throw new EntityNotFoundException("Destination", option.DestinationId.Value);
            }
            if (option.PackageId.HasValue && await _packageRepository.GetByIdAsync(option.PackageId.Value, __cancellationToken) is null)
            {
                throw new EntityNotFoundException("Package", option.PackageId.Value);
            }
        }

        var toSave = __options.Select(option =>
        {
            var optionId = Guid.NewGuid();
            var totalPrice = option.PricePerPerson * option.NumberOfPeople;
            var optionModel = new QuotationOptionModel
            {
                Id = optionId,
                QuotationId = __id,
                PackageId = option.PackageId,
                OptionName = option.OptionName,
                DestinationId = option.DestinationId,
                DurationDays = option.DurationDays,
                DurationNights = option.DurationNights,
                HotelCategory = option.HotelCategory,
                NumberOfPeople = option.NumberOfPeople,
                PricePerPerson = option.PricePerPerson,
                TotalPrice = totalPrice,
                IsRecommended = option.IsRecommended,
                SortOrder = option.SortOrder
            };

            var items = option.Items
                .Select(item => new QuotationItemModel
                {
                    Id = Guid.NewGuid(),
                    QuotationOptionId = optionId,
                    Description = item.Description,
                    Category = item.Category,
                    Amount = item.Amount,
                    SortOrder = item.SortOrder
                })
                .ToList();

            return (Option: optionModel, Items: items);
        }).ToList();

        await _quotationRepository.ReplaceOptionsAsync(__id, toSave, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "quotation.options_updated", "Quotation", __id.ToString(), null, $"{toSave.Count} option(s)", __cancellationToken);

        return toSave.Select(saved => ToOptionResponse(saved.Option, saved.Items)).ToList();
    }

    public async Task DeleteAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Quotation", __id);
        if (!DeletableStatuses.Contains(quotation.Status))
        {
            throw new BusinessException("Only a Draft, Rejected or Expired quotation can be deleted.");
        }

        await _quotationRepository.DeleteAsync(__id, _currentUserAccessor.UserId, __cancellationToken);
        await _auditLogWriter.LogAsync(_currentUserAccessor.UserId, "quotation.deleted", "Quotation", __id.ToString(), quotation.QuotationNumber, null, __cancellationToken);
    }

    public async Task<QuotationPublicResponse> GetPublicAsync(string __token, CancellationToken __cancellationToken)
    {
        var quotation = await LoadPublicQuotationAsync(__token, __cancellationToken);
        var options = await LoadOptionResponsesAsync(quotation.Id, __cancellationToken);

        return new QuotationPublicResponse
        {
            QuotationNumber = quotation.QuotationNumber,
            Title = quotation.Title,
            Status = quotation.Status,
            ValidUntil = quotation.ValidUntil,
            Notes = quotation.Notes,
            LeadCustomerName = quotation.LeadCustomerName,
            SelectedOptionId = quotation.SelectedOptionId,
            Options = options
        };
    }

    public async Task ApprovePublicAsync(string __token, QuotationPublicApproveRequest __request, string? __ipAddress, CancellationToken __cancellationToken)
    {
        var quotation = await LoadPublicQuotationAsync(__token, __cancellationToken);
        if (quotation.Status != "Sent")
        {
            throw new BusinessException("This quotation has already been decided or is no longer available.");
        }

        var option = await _quotationRepository.GetOptionByIdAsync(__request.SelectedOptionId, __cancellationToken);
        if (option is null || option.QuotationId != quotation.Id)
        {
            throw new BusinessException("The selected option does not belong to this quotation.");
        }

        await _quotationRepository.ApproveAsync(quotation.Id, option.Id, __request.ApprovedByName, __cancellationToken);
        await _quotationRepository.CreateApprovalAsync(new QuotationApprovalModel
        {
            Id = Guid.NewGuid(),
            QuotationId = quotation.Id,
            SelectedOptionId = option.Id,
            Decision = "Approved",
            ApprovedByName = __request.ApprovedByName,
            Comments = __request.Comments,
            IpAddress = __ipAddress
        }, __cancellationToken);

        await _auditLogWriter.LogAsync(null, "quotation.approved", "Quotation", quotation.Id.ToString(), "Sent", "Approved", __cancellationToken);
    }

    public async Task RejectPublicAsync(string __token, QuotationPublicRejectRequest __request, string? __ipAddress, CancellationToken __cancellationToken)
    {
        var quotation = await LoadPublicQuotationAsync(__token, __cancellationToken);
        if (quotation.Status != "Sent")
        {
            throw new BusinessException("This quotation has already been decided or is no longer available.");
        }

        await _quotationRepository.RejectAsync(quotation.Id, __request.Reason, __cancellationToken);
        await _quotationRepository.CreateApprovalAsync(new QuotationApprovalModel
        {
            Id = Guid.NewGuid(),
            QuotationId = quotation.Id,
            SelectedOptionId = null,
            Decision = "Rejected",
            ApprovedByName = __request.RejectedByName,
            Comments = __request.Reason,
            IpAddress = __ipAddress
        }, __cancellationToken);

        await _auditLogWriter.LogAsync(null, "quotation.rejected", "Quotation", quotation.Id.ToString(), "Sent", "Rejected", __cancellationToken);
    }

    public async Task<byte[]> GeneratePdfAsync(Guid __id, CancellationToken __cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdAsync(__id, __cancellationToken) ?? throw new EntityNotFoundException("Quotation", __id);
        return await BuildPdfAsync(quotation, __cancellationToken);
    }

    public async Task<byte[]> GeneratePublicPdfAsync(string __token, CancellationToken __cancellationToken)
    {
        var quotation = await LoadPublicQuotationAsync(__token, __cancellationToken);
        return await BuildPdfAsync(quotation, __cancellationToken);
    }

    private async Task<byte[]> BuildPdfAsync(QuotationModel __quotation, CancellationToken __cancellationToken)
    {
        var options = await _quotationRepository.GetOptionsAsync(__quotation.Id, __cancellationToken);
        var items = await _quotationRepository.GetItemsForOptionsAsync(options.Select(o => o.Id).ToList(), __cancellationToken);
        var itemsByOptionId = items.GroupBy(i => i.QuotationOptionId).ToDictionary(g => g.Key, g => g.ToList());
        return _pdfService.Generate(__quotation, options, itemsByOptionId);
    }

    private async Task<QuotationModel> LoadPublicQuotationAsync(string __token, CancellationToken __cancellationToken)
    {
        var quotation = await _quotationRepository.GetByPublicTokenHashAsync(TokenHasher.Hash(__token), __cancellationToken);
        if (quotation is null || quotation.Status == "Draft")
        {
            throw new EntityNotFoundException("Quotation", "link");
        }

        if (quotation.Status == "Sent" && quotation.ValidUntil.HasValue && quotation.ValidUntil.Value < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            await _quotationRepository.UpdateStatusAsync(quotation.Id, "Expired", null, __cancellationToken);
            quotation.Status = "Expired";
        }

        return quotation;
    }

    private async Task<List<QuotationOptionResponse>> LoadOptionResponsesAsync(Guid __quotationId, CancellationToken __cancellationToken)
    {
        var options = await _quotationRepository.GetOptionsAsync(__quotationId, __cancellationToken);
        var items = await _quotationRepository.GetItemsForOptionsAsync(options.Select(o => o.Id).ToList(), __cancellationToken);
        var itemsByOptionId = items.GroupBy(i => i.QuotationOptionId).ToDictionary(g => g.Key, g => g.ToList());

        return options
            .Select(option => ToOptionResponse(option, itemsByOptionId.TryGetValue(option.Id, out var found) ? found : []))
            .ToList();
    }

    private void EnsureEditable(QuotationModel __quotation)
    {
        if (!EditableStatuses.Contains(__quotation.Status))
        {
            throw new BusinessException("This quotation has already been decided and can no longer be edited.");
        }
    }

    private static (string RawToken, string TokenHash) GeneratePublicToken()
    {
        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        return (rawToken, TokenHasher.Hash(rawToken));
    }

    private static string GenerateQuotationNumber()
        => $"QT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

    private static QuotationResponse ToResponse(QuotationModel quotation) => new()
    {
        Id = quotation.Id,
        QuotationNumber = quotation.QuotationNumber,
        LeadId = quotation.LeadId,
        LeadCustomerName = quotation.LeadCustomerName,
        CustomerId = quotation.CustomerId,
        CustomerName = quotation.CustomerName,
        Title = quotation.Title,
        Status = quotation.Status,
        ValidUntil = quotation.ValidUntil,
        Notes = quotation.Notes,
        SelectedOptionId = quotation.SelectedOptionId,
        ApprovedAt = quotation.ApprovedAt,
        ApprovedByName = quotation.ApprovedByName,
        RejectionReason = quotation.RejectionReason
    };

    private static QuotationOptionResponse ToOptionResponse(QuotationOptionModel option, List<QuotationItemModel> items) => new()
    {
        Id = option.Id,
        PackageId = option.PackageId,
        OptionName = option.OptionName,
        DestinationId = option.DestinationId,
        DestinationName = option.DestinationName,
        DurationDays = option.DurationDays,
        DurationNights = option.DurationNights,
        HotelCategory = option.HotelCategory,
        NumberOfPeople = option.NumberOfPeople,
        PricePerPerson = option.PricePerPerson,
        TotalPrice = option.TotalPrice,
        IsRecommended = option.IsRecommended,
        SortOrder = option.SortOrder,
        Items = items
            .Select(item => new QuotationItemResponse
            {
                Id = item.Id,
                Description = item.Description,
                Category = item.Category,
                Amount = item.Amount,
                SortOrder = item.SortOrder
            })
            .ToList()
    };
}
