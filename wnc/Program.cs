using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using wnc.Data;

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
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
