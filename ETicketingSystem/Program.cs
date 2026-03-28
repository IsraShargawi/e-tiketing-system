using ETicketingSystem.Accounting.Services;
using ETicketingSystem.Data;
using ETicketingSystem.Payment.Handlers;
using ETicketingSystem.Payment.Interfaces;
using ETicketingSystem.Payment.Services;
using ETicketingSystem.Ticket.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// CORS Configuration for React Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000", 
                "http://localhost:5173",
                "http://localhost:80",
                "http://frontend:80")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Payment Handlers (Strategy Pattern)
builder.Services.AddScoped<IPaymentHandler, CreditCardHandler>();
builder.Services.AddScoped<IPaymentHandler, QRScanHandler>();

// Services
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<LedgerService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "E-Ticketing System API", 
        Version = "v1",
        Description = @"Backend API for E-Ticketing & Payment Simulation Platform with Double-Entry Ledger"
    });
    
    // Add XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Ticketing System API V1");
        c.DocumentTitle = "E-Ticketing System API";
    });
}
else
{
    // Enable Swagger in production for demo purposes
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Ticketing System API V1");
    });
}

app.UseCors("AllowReactApp");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
