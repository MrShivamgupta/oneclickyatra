using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Services;

public interface IQuotationPdfService
{
    Task<byte[]> GenerateAsync(
        QuotationModel __quotation,
        IReadOnlyList<QuotationOptionModel> __options,
        IReadOnlyDictionary<Guid, List<QuotationItemModel>> __itemsByOptionId,
        CancellationToken __cancellationToken);
}
