using Day11Identity.IdentityData;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<EventIdentityDbContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("con")
        )
    );


builder.Services.AddIdentity<IdentityUser, IdentityRole>(
    options => options.SignIn.RequireConfirmedAccount = false
    ).AddEntityFrameworkStores<EventIdentityDbContext>();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "auth_cookie";
    options.Cookie.SameSite = SameSiteMode.None;
    options.LoginPath = new PathString("/users/login");
    options.AccessDeniedPath = new PathString("/users/AccessDenied");

}
    );

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
