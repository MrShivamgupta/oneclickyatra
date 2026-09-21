using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.Repositories;

public interface IVendorRepository
{
    Task<VendorModel?> GetByIdAsync(Guid __id, CancellationToken __cancellationToken);
    Task<VendorModel?> GetByUserIdAsync(Guid __userId, CancellationToken __cancellationToken);
    Task<PaginationResponse<VendorModel>> SearchAsync(VendorSearchRequest __request, CancellationToken __cancellationToken);
    Task CreateAsync(VendorModel __vendor, CancellationToken __cancellationToken);
    Task UpdateAsync(VendorModel __vendor, CancellationToken __cancellationToken);
    Task DeleteAsync(Guid __id, Guid? __deletedBy, CancellationToken __cancellationToken);
    Task LinkUserAsync(Guid __id, Guid __userId, Guid? __updatedBy, CancellationToken __cancellationToken);

    /// <summary>Recomputes Vendors.Rating as the average Rating across that vendor's
    /// non-deleted VendorPerformance rows (simple recompute-on-write, not a trigger).</summary>
    Task RecomputeRatingAsync(Guid __vendorId, CancellationToken __cancellationToken);

    Task<IReadOnlyList<VendorContactModel>> GetContactsAsync(Guid __vendorId, CancellationToken __cancellationToken);
    Task ReplaceContactsAsync(Guid __vendorId, IReadOnlyList<VendorContactModel> __contacts, CancellationToken __cancellationToken);

    Task<IReadOnlyList<VendorRateModel>> GetRatesAsync(Guid __vendorId, CancellationToken __cancellationToken);
    Task ReplaceRatesAsync(Guid __vendorId, IReadOnlyList<VendorRateModel> __rates, CancellationToken __cancellationToken);

    Task<PaginationResponse<VendorPaymentModel>> GetPaymentsAsync(Guid __vendorId, PaginationRequest __request, CancellationToken __cancellationToken);
    Task<VendorPaymentModel?> GetPaymentByIdAsync(Guid __paymentId, CancellationToken __cancellationToken);
    Task CreatePaymentAsync(VendorPaymentModel __payment, CancellationToken __cancellationToken);
    Task UpdatePaymentStatusAsync(Guid __paymentId, string __status, DateTime? __paidAt, Guid? __updatedBy, CancellationToken __cancellationToken);

    Task<IReadOnlyList<VendorPerformanceModel>> GetPerformanceAsync(Guid __vendorId, CancellationToken __cancellationToken);
    Task CreatePerformanceAsync(VendorPerformanceModel __performance, CancellationToken __cancellationToken);

    /// <summary>Best-effort "booking requests" feed for the Vendor Portal — see
    /// VendorBookingRequestModel for the scope note on the DestinationId-based join.</summary>
    Task<PaginationResponse<VendorBookingRequestModel>> GetBookingRequestsAsync(Guid __vendorId, PaginationRequest __request, CancellationToken __cancellationToken);
}
