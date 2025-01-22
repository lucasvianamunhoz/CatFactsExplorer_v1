using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CatFactsExplorer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CatFactsExplorer.Domain.Repositories;
using CatFactsExplorer.Infrastructure.Data;
using CatFactsExplorer.Application.Interfaces;
using CatFactsExplorer.Application.Services;
using CatFactsExplorer.Jobs;
using CatFactsExplorer.Worker;
using Microsoft.AspNetCore.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
 
        services.AddDbContext<CatFactsDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("MSSQL")));
         
        services.AddScoped<ICatFactRepository, CatFactRepository>();
        services.AddScoped<ICatFactService, CatFactService>();

 
        services.AddHttpClient<CatFactUpsertJob>();

 
        services.AddHangfire(configuration =>
        {
            configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(
                    context.Configuration.GetConnectionString("MSSQL"),
                    new SqlServerStorageOptions
                    {
                        PrepareSchemaIfNecessary = true,
                        QueuePollInterval = TimeSpan.FromSeconds(15),  
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    });
        });

 
        services.AddHangfireServer();
        services.AddHostedService<WorkerService>();
    })
    .ConfigureWebHostDefaults(webBuilder =>
    {
 
        webBuilder.UseUrls("http://0.0.0.0:5002", "http://localhost:5002"); 
        webBuilder.Configure(app =>
        {
 
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[] { new BasicAuthAuthorizationFilter(
                        new BasicAuthAuthorizationFilterOptions
                        {
                            Login = "admin", 
                            PasswordClear = "admin123"  
                        })}
                });
            });
        });
    });

builder.Build().Run();
 