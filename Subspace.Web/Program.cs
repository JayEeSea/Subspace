using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Data;
using Subspace.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("SubspaceApiDb"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("SubspaceApiDb"))
    ));

builder.Services.AddDbContext<SubspaceDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("SubspaceApiDb"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("SubspaceApiDb"))
    ));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    
}

app.UseRouting();

app.UseAuthorization();

app.MapGet("/Identity/Account/Register", context =>
{
    context.Response.StatusCode = 404;
    return Task.CompletedTask;
});

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

var seedSettings = builder.Configuration.GetSection("SeedAdmin");
var userEmail = seedSettings["Email"];
var roleName = seedSettings["Role"];

if (app.Environment.IsDevelopment() && !string.IsNullOrWhiteSpace(userEmail))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync(roleName))
        await roleManager.CreateAsync(new IdentityRole(roleName));

    var user = await userManager.FindByEmailAsync(userEmail);
    if (user != null && !(await userManager.IsInRoleAsync(user, roleName)))
        await userManager.AddToRoleAsync(user, roleName);
}

app.Run();