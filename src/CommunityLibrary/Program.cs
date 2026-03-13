using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CommunityLibrary.Data;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "DataSource=app.db;Cache=Shared";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(connectionString);
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

await SeedData.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseRouting();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";
    if (path.StartsWith("//", StringComparison.Ordinal))
    {
        context.Request.Path = "/" + path.TrimStart('/');
    }
    await next();
});

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/Identity/Account/Register", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Redirect("/Account/Login");
        return;
    }
    if (context.Request.Path.StartsWithSegments("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase))
    {
        var returnUrl = context.Request.Query["ReturnUrl"].FirstOrDefault();
        var redirect = string.IsNullOrEmpty(returnUrl) ? "/Account/Login" : "/Account/Login?returnUrl=" + Uri.EscapeDataString(returnUrl);
        context.Response.Redirect(redirect);
        return;
    }
    await next();
});

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
