using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

// ======= Build Injected Services
var webHostBuilder = WebApplication.CreateBuilder(args);
webHostBuilder.Services.AddControllers();
webHostBuilder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Caliber 2022", Version = "v1" });
});


// Add logging
webHostBuilder.Logging.ClearProviders();
webHostBuilder.Logging.AddConsole();

// ======= Build, Configure and Run Application
var hostApi = webHostBuilder.Build();
hostApi.UseSwagger();
hostApi.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Caliber 2022 v1"));
hostApi.UseRouting();
hostApi.UseAuthorization();
hostApi.UseEndpoints(endpoints => endpoints.MapControllers());
await hostApi.RunAsync();
