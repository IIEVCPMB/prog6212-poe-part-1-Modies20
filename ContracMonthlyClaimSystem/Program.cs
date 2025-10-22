using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Data;
using Microsoft.EntityFrameworkCore.InMemory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Use In-Memory Database for development (easier setup)
builder.Services.AddDbContext<ApplicationDbContext>(static options =>
    options.UseInMemoryDatabase("ContractMonthlyClaimSystem"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Custom error handling for different HTTP status codes
app.UseStatusCodePagesWithReExecute("/Home/Error{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Global error handling
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An unhandled exception occurred.");
        context.Response.Redirect("/Home/Error");
    }
});

app.Run();