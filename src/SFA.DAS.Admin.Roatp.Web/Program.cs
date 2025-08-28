using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.ApplicationInsights;
using SFA.DAS.Admin.Roatp.Web.AppStart;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;
using SFA.DAS.Api.Common.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration.AddApplicationConfiguration(builder.Services);

builder.Services
    .AddLogging(builder =>
    {
        builder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
        builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
    })
    .AddApplicationInsightsTelemetry();

builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => false; // Default is true, make it false
        options.MinimumSameSitePolicy = SameSiteMode.None;
    });

builder.Services
    .AddHealthChecks()
    .AddCheck<OuterApiHealthCheck>("Outer API Health Check");

builder.Services
    .AddDataProtection(configuration)
    .AddHttpContextAccessor()
    .AddAuthentication(configuration)
    .AddApplicationRegistrations(configuration)
    .AddSession(configuration)
    .AddControllersWithViews()
    .AddSessionStateTempDataProvider();


var app = builder.Build();


app
    .UseHsts()
    .UseStatusCodePagesWithReExecute("/error/{0}")
    .UseExceptionHandler("/error");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseDeveloperExceptionPage();
}

app
    .UseHealthChecks("/health",
        new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
        })
    .UseHealthChecks("/ping",
        new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
        });

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
