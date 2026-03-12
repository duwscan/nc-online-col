using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.IdentityAccess.AuthLogs;
using wnc.Features.IdentityAccess.Common;
using wnc.Features.IdentityAccess.Roles;
using wnc.Features.IdentityAccess.UserRoles;
using wnc.Features.IdentityAccess.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IListQueryService<UsersListQuery, UserListItemViewModel>, UsersListQueryService>();
builder.Services.AddScoped<IListQueryService<RolesListQuery, RoleListItemViewModel>, RolesListQueryService>();
builder.Services.AddScoped<IListQueryService<UserRolesListQuery, UserRoleListItemViewModel>, UserRolesListQueryService>();
builder.Services.AddScoped<IListQueryService<AuthLogsListQuery, AuthLogListItemViewModel>, AuthLogsListQueryService>();

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
