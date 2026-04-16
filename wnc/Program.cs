using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Globalization;
using wnc.Data;
using wnc.Infrastructure.Identity;
using wnc.Infrastructure.Security;
using wnc.Models;

var builder = WebApplication.CreateBuilder(args);

var supportedCultures = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("vi-VN")
};

// Add services to the container.
builder.Services.AddLocalization();
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserStore<AppUser>, AppUserStore>();
builder.Services.AddScoped<IRoleStore<Role>, AppRoleStore>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, BcryptPasswordHasher>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();
builder.Services.AddScoped<PortalSessionService>();
builder.Services.AddSingleton<PersistentUserStateCookieService>();
builder.Services.AddIdentityCore<AppUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<Role>()
    .AddSignInManager<SignInManager<AppUser>>()
    .AddRoleManager<RoleManager<Role>>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = PortalSessionService.SessionCookieName;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services
    .AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.Cookie.Name = "wnc.auth";
        options.LoginPath = "/auth/student/login";
        options.AccessDeniedPath = "/auth/student/login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                var loginPath = context.Request.Path.StartsWithSegments("/admin", StringComparison.OrdinalIgnoreCase)
                    ? "/auth/admin/login"
                    : "/auth/student/login";

                var returnUrl = Uri.EscapeDataString(
                    $"{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}");

                context.Response.Redirect($"{loginPath}?returnUrl={returnUrl}");
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                var loginPath = context.Request.Path.StartsWithSegments("/admin", StringComparison.OrdinalIgnoreCase)
                    ? "/auth/admin/login"
                    : "/auth/student/login";

                context.Response.Redirect(loginPath);
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

var webRootPath = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
var uploadsPath = Path.Combine(webRootPath, "uploads");

Directory.CreateDirectory(uploadsPath);

var requestLocalizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

await DbInitializer.InitializeAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization(requestLocalizationOptions);
app.UseRouting();
app.UseSession();
app.UseMiddleware<UserStateTrackingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
