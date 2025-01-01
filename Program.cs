using gestion_budget.DAL;
using gestion_budget.Services;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

//DI
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<DefaultSettingService>();
builder.Services.AddScoped<BudgetAlertService>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<TransactionService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:26789/api/transactions");
});
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Back-Office");
    options.Conventions.AllowAnonymousToPage("/Back-Office/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Back-Office/Account/Register");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.Use(async (context, next) =>
{
    var session = context.Session;
    var path = context.Request.Path.Value.ToLowerInvariant();

    var publicPages = new[] { "/back-office/account/login", "/back-office/account/register", "/back-office/account/logout" };

    if (!session.TryGetValue("UserId", out _) && !publicPages.Contains(path))
    {
        context.Response.Redirect("/Back-Office/Account/Login");
        return;
    }

    await next();
});


app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
