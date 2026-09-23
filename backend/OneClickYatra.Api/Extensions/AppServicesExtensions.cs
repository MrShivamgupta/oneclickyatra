using FluentValidation;
using OneClickYatra.Api.AppFunctions;
using OneClickYatra.Api.Infrastructure;
using OneClickYatra.Api.Repositories;
using OneClickYatra.Api.Security;
using OneClickYatra.Api.Security.Jwt;
using OneClickYatra.Api.Security.Password;
using OneClickYatra.Api.Services;
using OneClickYatra.Api.Services.Email;
using OneClickYatra.Api.Services.Payments;
using OneClickYatra.Api.Services.Storage;
using OneClickYatra.Api.Services.WhatsApp;
using StackExchange.Redis;

namespace OneClickYatra.Api.Extensions;

public static class AppServicesExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection __services, IConfiguration __configuration)
    {
        // Infrastructure
        __services.AddHttpContextAccessor();
        __services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
        __services.AddScoped<ITrackingIdAccessor, HttpContextTrackingIdAccessor>();
        __services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
        __services.AddScoped<ValidationActionFilter>();

        // Redis (cache layer only — SQL Server remains the source of truth). AbortOnConnectFail
        // is disabled so a down/unreachable Redis never crashes the app at DI-resolution time —
        // ICacheService/RedisCacheService catch per-command failures and degrade to a cache miss.
        var redisConfiguration = ConfigurationOptions.Parse(__configuration.GetConnectionString("Redis") ?? "localhost:6379");
        redisConfiguration.AbortOnConnectFail = false;
        redisConfiguration.ConnectTimeout = 1000;
        redisConfiguration.ConnectRetry = 1;
        redisConfiguration.SyncTimeout = 500;
        redisConfiguration.AsyncTimeout = 500;
        redisConfiguration.KeepAlive = 30;
        __services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConfiguration));
        __services.AddScoped<ICacheService, RedisCacheService>();

        // Security
        __services.Configure<SecurityOptions>(__configuration.GetSection(SecurityOptions.SectionName));
        __services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        __services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // Repositories
        __services.AddScoped<IUserRepository, UserRepository>();
        __services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        __services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        __services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        __services.AddScoped<ICountryRepository, CountryRepository>();
        __services.AddScoped<ICityRepository, CityRepository>();
        __services.AddScoped<ICategoryRepository, CategoryRepository>();
        __services.AddScoped<ISeasonRepository, SeasonRepository>();
        __services.AddScoped<IDestinationRepository, DestinationRepository>();
        __services.AddScoped<IPackageRepository, PackageRepository>();
        __services.AddScoped<IPackageContentRepository, PackageContentRepository>();
        __services.AddScoped<IEnquiryRepository, EnquiryRepository>();
        __services.AddScoped<IPageRepository, PageRepository>();
        __services.AddScoped<ICustomerRepository, CustomerRepository>();
        __services.AddScoped<ILeadRepository, LeadRepository>();
        __services.AddScoped<IFollowUpRepository, FollowUpRepository>();
        __services.AddScoped<IQuotationRepository, QuotationRepository>();
        __services.AddScoped<IBookingRepository, BookingRepository>();
        __services.AddScoped<IDashboardRepository, DashboardRepository>();
        __services.AddScoped<IPaymentRepository, PaymentRepository>();
        __services.AddScoped<IRefundRepository, RefundRepository>();
        __services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        __services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        __services.AddScoped<IRoleRepository, RoleRepository>();

        // Vendor Management
        __services.AddScoped<IVendorRepository, VendorRepository>();

        // Reports domain
        __services.AddScoped<IReportRepository, ReportRepository>();

        __services.AddScoped<IWhatsAppTemplateRepository, WhatsAppTemplateRepository>();
        __services.AddScoped<INotificationLogRepository, NotificationLogRepository>();

        __services.AddScoped<ICustomerDocumentRepository, CustomerDocumentRepository>();
        __services.AddScoped<IVendorInvoiceRepository, VendorInvoiceRepository>();

        __services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();

        // Services
        __services.AddScoped<IAuditLogWriter, AuditLogWriter>();
        __services.AddScoped<IQuotationPdfService, QuotationPdfService>();
        __services.AddScoped<IInvoicePdfService, InvoicePdfService>();
        __services.Configure<RazorpayOptions>(__configuration.GetSection(RazorpayOptions.SectionName));
        __services.AddHttpClient<IPaymentGateway, RazorpayPaymentGateway>();

        // Reports domain
        __services.AddScoped<IReportExportService, ReportExportService>();

        __services.Configure<WhatsAppOptions>(__configuration.GetSection(WhatsAppOptions.SectionName));
        __services.AddHttpClient<IWhatsAppGateway, MetaWhatsAppGateway>();

        __services.Configure<FileStorageOptions>(__configuration.GetSection(FileStorageOptions.SectionName));
        __services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        __services.AddScoped<IVoucherPdfService, VoucherPdfService>();

        __services.Configure<EmailOptions>(__configuration.GetSection(EmailOptions.SectionName));
        __services.AddScoped<IEmailGateway, SmtpEmailGateway>();
        __services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        __services.AddScoped<IEmailNotificationSender, EmailNotificationSender>();

        // AppFunctions
        __services.AddScoped<IAuthAppFunction, AuthAppFunction>();
        __services.AddScoped<ICountryAppFunction, CountryAppFunction>();
        __services.AddScoped<ICityAppFunction, CityAppFunction>();
        __services.AddScoped<ICategoryAppFunction, CategoryAppFunction>();
        __services.AddScoped<ISeasonAppFunction, SeasonAppFunction>();
        __services.AddScoped<IDestinationAppFunction, DestinationAppFunction>();
        __services.AddScoped<IPackageAppFunction, PackageAppFunction>();
        __services.AddScoped<IEnquiryAppFunction, EnquiryAppFunction>();
        __services.AddScoped<IPageAppFunction, PageAppFunction>();
        __services.AddScoped<ICustomerAppFunction, CustomerAppFunction>();
        __services.AddScoped<ILeadAppFunction, LeadAppFunction>();
        __services.AddScoped<IFollowUpAppFunction, FollowUpAppFunction>();
        __services.AddScoped<IUserAppFunction, UserAppFunction>();
        __services.AddScoped<IQuotationAppFunction, QuotationAppFunction>();
        __services.AddScoped<IBookingAppFunction, BookingAppFunction>();
        __services.AddScoped<IDashboardAppFunction, DashboardAppFunction>();
        __services.AddScoped<IPaymentAppFunction, PaymentAppFunction>();
        __services.AddScoped<IInvoiceAppFunction, InvoiceAppFunction>();
        __services.AddScoped<ICustomerPortalAppFunction, CustomerPortalAppFunction>();

        // Vendor Management
        __services.AddScoped<IVendorAppFunction, VendorAppFunction>();
        __services.AddScoped<IVendorPortalAppFunction, VendorPortalAppFunction>();

        // Reports domain
        __services.AddScoped<IReportAppFunction, ReportAppFunction>();

        __services.AddScoped<IWhatsAppAppFunction, WhatsAppAppFunction>();

        __services.AddScoped<IFeedbackAppFunction, FeedbackAppFunction>();

        __services.AddScoped<IAuditLogAppFunction, AuditLogAppFunction>();

        __services.AddScoped<IRoleAppFunction, RoleAppFunction>();

        // Validators
        __services.AddValidatorsFromAssemblyContaining<Program>();

        return __services;
    }
}
