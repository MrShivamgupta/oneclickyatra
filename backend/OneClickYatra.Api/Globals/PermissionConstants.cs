namespace OneClickYatra.Api.Globals;

/// <summary>
/// Canonical permission keys ("resource.action"). Extend as new modules are added;
/// never remove an entry without checking RolePermissions usages first.
/// </summary>
public static class PermissionConstants
{
    public const string DashboardView = "dashboard.view";

    public const string LeadView = "lead.view";
    public const string LeadCreate = "lead.create";
    public const string LeadUpdate = "lead.update";
    public const string LeadDelete = "lead.delete";

    public const string CustomerView = "customer.view";
    public const string CustomerCreate = "customer.create";
    public const string CustomerUpdate = "customer.update";
    public const string CustomerDelete = "customer.delete";

    public const string FollowUpView = "followup.view";
    public const string FollowUpCreate = "followup.create";
    public const string FollowUpUpdate = "followup.update";
    public const string FollowUpDelete = "followup.delete";

    public const string QuotationView = "quotation.view";
    public const string QuotationCreate = "quotation.create";
    public const string QuotationUpdate = "quotation.update";
    public const string QuotationDelete = "quotation.delete";

    public const string PackageView = "package.view";
    public const string PackageCreate = "package.create";
    public const string PackageUpdate = "package.update";
    public const string PackageDelete = "package.delete";

    public const string DestinationView = "destination.view";
    public const string DestinationCreate = "destination.create";
    public const string DestinationUpdate = "destination.update";
    public const string DestinationDelete = "destination.delete";

    public const string MasterDataView = "masterdata.view";
    public const string MasterDataManage = "masterdata.manage";

    public const string BookingView = "booking.view";
    public const string BookingCreate = "booking.create";
    public const string BookingUpdate = "booking.update";
    public const string BookingCancel = "booking.cancel";
    public const string BookingRefund = "booking.refund";
    public const string BookingDelete = "booking.delete";

    public const string PaymentView = "payment.view";
    public const string PaymentCreate = "payment.create";
    public const string PaymentRefund = "payment.refund";

    public const string ReportView = "report.view";

    public const string UserManage = "user.manage";
    public const string RoleManage = "role.manage";
    public const string SettingsManage = "settings.manage";
    public const string AuditView = "audit.view";

    public const string EnquiryView = "enquiry.view";
    public const string EnquiryUpdate = "enquiry.update";
    public const string EnquiryDelete = "enquiry.delete";

    public const string CmsView = "cms.view";
    public const string CmsManage = "cms.manage";
}

/// <summary>Seed-time role names. Architecture supports adding further roles without code changes.</summary>
public static class RoleConstants
{
    public const string Guest = "Guest";
    public const string Customer = "Customer";
    public const string TravelAgent = "TravelAgent";
    public const string OperationsStaff = "OperationsStaff";
    public const string Finance = "Finance";
    public const string SuperAdmin = "SuperAdmin";
}
