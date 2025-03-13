using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using TicketModule.Data;
using TicketModule.Models;
using TicketModule.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавляем поддержку контроллеров и Razor Views
builder.Services.AddControllersWithViews();

// Регистрируем репозитории как синглтоны
builder.Services.AddSingleton<TicketRepository>();
builder.Services.AddSingleton<FlightRepository>();

// Регистрируем настройки из конфигурации
builder.Services.Configure<FlightSettings>(builder.Configuration.GetSection("FlightSettings"));

// Переключение между заглушками и реальными сервисами
bool useStubs = builder.Configuration.GetValue<bool>("UseStubs");

if (useStubs)
{
    // Используем заглушки для Табло и Кейтеринга
    builder.Services.AddScoped<ITableService, TableServiceStub>();
    builder.Services.AddScoped<ICateringService, CateringServiceStub>();
}
else
{
    // Регистрируем HttpClient для реальных сервисов. 
    // URL-адреса берутся из конфигурации.
   builder.Services.AddHttpClient<ITableService, TableService>(client =>
{
    var tableBaseUrl = builder.Configuration["TableSettings:BaseUrl"] 
                       ?? throw new InvalidOperationException("Missing TableSettings:BaseUrl configuration");
    client.BaseAddress = new Uri(tableBaseUrl);
});
builder.Services.AddHttpClient<ICateringService, CateringService>(client =>
{
    var cateringBaseUrl = builder.Configuration["CateringSettings:BaseUrl"]
                          ?? throw new InvalidOperationException("Missing CateringSettings:BaseUrl configuration");
    client.BaseAddress = new Uri(cateringBaseUrl);
});
}

// Регистрируем основной сервис билетов
builder.Services.AddScoped<ITicketService, TicketService>();

// Регистрируем остальные сервисы, контроллеры и CORS
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Регистрируем Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ticket Purchase/Return API",
        Version = "v1",
        Description = "API для покупки и возврата билетов"
    });
});

// Настройка Kestrel для HTTP (8080) и HTTPS (8081)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ticket API v1");
    });
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");

// Маршруты для API и MVC
app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();
