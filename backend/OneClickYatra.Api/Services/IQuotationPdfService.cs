using OneClickYatra.Api.Models;

namespace OneClickYatra.Api.Services;

public interface IQuotationPdfService
{
    byte[] Generate(QuotationModel __quotation, IReadOnlyList<QuotationOptionModel> __options, IReadOnlyDictionary<Guid, List<QuotationItemModel>> __itemsByOptionId);
}
