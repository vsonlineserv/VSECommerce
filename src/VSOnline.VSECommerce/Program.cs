using AspNetCoreRateLimit;
using AuthPermissions;
using AutoMapper;
using AutoMapper.Internal.Mappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using VSOnline.VSECommerce;
using VSOnline.VSECommerce.Controllers;
using VSOnline.VSECommerce.Domain;
using VSOnline.VSECommerce.Domain.Caching;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.Helper;
using VSOnline.VSECommerce.Domain.Loader;
using VSOnline.VSECommerce.Domain.Mapping;
using VSOnline.VSECommerce.Domain.Notifications;
using VSOnline.VSECommerce.Domain.Order;
using VSOnline.VSECommerce.Domain.Settings;
using VSOnline.VSECommerce.Permission;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Utilities;
using static VSOnline.VSECommerce.Domain.UserService;

var builder = WebApplication.CreateBuilder(args);

var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile(new ObjectMapper());
});

var mapper = config.CreateMapper();

builder.Services.AddControllers();

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext"));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ClockSkew = TimeSpan.Zero,
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["AppSettings:JwtIssuer"],
        ValidAudience = builder.Configuration["AppSettings:JwtIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Jwtkey"]))
    };
});
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllHeaders",
    builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();

    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyTypes.Orders_Read, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Orders_Read); });
    options.AddPolicy(PolicyTypes.Orders_Write, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Orders_Write); });
    options.AddPolicy(PolicyTypes.Orders_Edit, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Orders_Edit); });
    options.AddPolicy(PolicyTypes.Orders_Delete, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Orders_Delete); });

    options.AddPolicy(PolicyTypes.Category_Read, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Category_Read); });
    options.AddPolicy(PolicyTypes.Category_Write, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Category_Write); });
    options.AddPolicy(PolicyTypes.Category_Edit, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Category_Edit); });
    options.AddPolicy(PolicyTypes.Category_Delete, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Category_Delete); });

    options.AddPolicy(PolicyTypes.Product_Read, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Product_Read); });
    options.AddPolicy(PolicyTypes.Product_Write, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Product_Write); });
    options.AddPolicy(PolicyTypes.Product_Edit, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Product_Edit); });
    options.AddPolicy(PolicyTypes.Product_Delete, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Product_Delete); });

    options.AddPolicy(PolicyTypes.Inventory_Read, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Inventory_Read); });
    options.AddPolicy(PolicyTypes.Inventory_Write, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Inventory_Write); });
    options.AddPolicy(PolicyTypes.Inventory_Edit, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Inventory_Edit); });
    options.AddPolicy(PolicyTypes.Inventory_Delete, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Inventory_Delete); });

    options.AddPolicy(PolicyTypes.Discount_Read, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Discount_Read); });
    options.AddPolicy(PolicyTypes.Discount_Write, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Discount_Write); });
    options.AddPolicy(PolicyTypes.Discount_Edit, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Discount_Edit); });
    options.AddPolicy(PolicyTypes.Discount_Delete, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Discount_Delete); });

    options.AddPolicy(PolicyTypes.Enquiries_Read, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Enquiries_Read); });
    options.AddPolicy(PolicyTypes.Enquiries_Write, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Enquiries_Write); });
    options.AddPolicy(PolicyTypes.Enquiries_Edit, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Enquiries_Edit); });
    options.AddPolicy(PolicyTypes.Enquiries_Delete, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Enquiries_Delete); });

    options.AddPolicy(PolicyTypes.Payment_Read, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Payment_Read); });
    options.AddPolicy(PolicyTypes.Payment_Write, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Payment_Write); });
    options.AddPolicy(PolicyTypes.Payment_Edit, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Payment_Edit); });
    options.AddPolicy(PolicyTypes.Payment_Delete, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Payment_Delete); });

    options.AddPolicy(PolicyTypes.Shipping_Read, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Shipping_Read); });
    options.AddPolicy(PolicyTypes.Shipping_Write, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Shipping_Write); });
    options.AddPolicy(PolicyTypes.Shipping_Edit, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Shipping_Edit); });
    options.AddPolicy(PolicyTypes.Shipping_Delete, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.Shipping_Delete); });

    options.AddPolicy(PolicyTypes.General, policy => { policy.RequireClaim(CustomClaimTypes.Permission, UserPermissions.General); });


});

builder.Services.AddSingleton(mapper);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<PricingRepository>();
builder.Services.AddScoped<SellerContactHelper>();
builder.Services.AddScoped<MailHelper>();
builder.Services.AddScoped<MailClient>();
builder.Services.AddScoped<MessageClient>();
builder.Services.AddScoped<MessageHelper>();
builder.Services.AddScoped<EfContext>();
builder.Services.AddScoped<OrderHelper>();
builder.Services.AddScoped<SellerRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<SellerBranchRepository>();
builder.Services.AddScoped<ProductFilterRepository>();
builder.Services.AddScoped<HelperQuery>();
builder.Services.AddScoped<ProductHelper>();
builder.Services.AddScoped<ProductFeaturesHelper>();
builder.Services.AddScoped<RatingHelper>();
builder.Services.AddScoped<CartRepository>();
builder.Services.AddScoped<TrackingOrderHelper>();
builder.Services.AddScoped<SearchClient>();
builder.Services.AddScoped<SearchHelper>();
builder.Services.AddScoped<SearchConfiguration>();
builder.Services.AddScoped<ManufacturerRepository>();
builder.Services.AddScoped<SiteSettingsService>();
builder.Services.AddScoped<StaticCache>();
builder.Services.AddScoped<NullCache>();
builder.Services.AddScoped<DefaultCache>();
builder.Services.AddScoped<ImportProductData>();
builder.Services.AddScoped<LoadDataFromIndependentExcel>();
builder.Services.AddScoped<LoaderHelper>();
builder.Services.AddScoped<LoadProductData>();
builder.Services.AddScoped<DTOMapper>();
builder.Services.AddScoped<ShoppingCartRepository>();
builder.Services.AddScoped<UserActionHelper>();
builder.Services.AddScoped<UserWishlistRepository>();
builder.Services.AddScoped<FileUploadController>();
builder.Services.AddScoped<NotificationServices>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();


builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "200s",
            Limit = 500
        }
    };
});

builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var app = builder.Build();
app.UseCors("AllowAllHeaders");
app.UseAuthentication();

app.UseHttpsRedirection();

app.UseIpRateLimiting();

app.UseAuthorization();
app.MapControllers();

app.Run();
