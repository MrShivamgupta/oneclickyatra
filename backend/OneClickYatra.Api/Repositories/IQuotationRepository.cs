using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface IQuotationRepository
{
    Task<QuotationModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<QuotationModel?> GetByPublicTokenHashAsync(string __publicTokenHash, CancellationToken __cancellationToken);
    Task<PaginationResponse<QuotationModel>> SearchAsync(QuotationSearchRequest __request, CancellationToken __cancellationToken);
    Task CreateAsync(QuotationModel __quotation, CancellationToken __cancellationToken);
    Task UpdateAsync(QuotationModel __quotation, CancellationToken __cancellationToken);
    Task UpdateStatusAsync(Guid __id, string __status, Guid? __updatedBy, CancellationToken __cancellationToken);
    Task ApproveAsync(Guid __id, Guid __selectedOptionId, string __approvedByName, CancellationToken __cancellationToken);
    Task RejectAsync(Guid __id, string? __reason, CancellationToken __cancellationToken);
    Task UpdatePublicTokenHashAsync(Guid __id, string __publicTokenHash, Guid? __updatedBy, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);

    Task<IReadOnlyList<QuotationOptionModel>> GetOptionsAsync(Guid __quotationId, CancellationToken __cancellationToken);
    Task<QuotationOptionModel?> GetOptionByIdAsync(Guid __optionId, CancellationToken __cancellationToken);
    Task<IReadOnlyList<QuotationItemModel>> GetItemsForOptionsAsync(IReadOnlyList<Guid> __optionIds, CancellationToken __cancellationToken);
    Task ReplaceOptionsAsync(Guid __quotationId, IReadOnlyList<(QuotationOptionModel Option, List<QuotationItemModel> Items)> __options, CancellationToken __cancellationToken);

    Task CreateApprovalAsync(QuotationApprovalModel __approval, CancellationToken __cancellationToken);
}
