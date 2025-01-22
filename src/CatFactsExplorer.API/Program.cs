using Microsoft.EntityFrameworkCore;
using CatFactsExplorer.Application.Interfaces;
using CatFactsExplorer.Application.Services;
using CatFactsExplorer.Domain.Repositories;
using CatFactsExplorer.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuração do banco de dados
builder.Services.AddDbContext<CatFactsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MSSQL")));

// Registro de dependências (DI)
builder.Services.AddScoped<ICatFactService, CatFactService>();
builder.Services.AddScoped<ICatFactRepository, CatFactRepository>();

// Configurar CORS
const string CorsPolicyName = "AllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CorsPolicyName, policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configurar Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configuração do Swagger
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Production"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CatFactsExplorer.API v1");
        c.RoutePrefix = string.Empty;  
    });
}

// Middleware do pipeline
app.UseRouting();
app.UseCors(CorsPolicyName);
app.UseAuthorization();
app.MapControllers();

app.Run();