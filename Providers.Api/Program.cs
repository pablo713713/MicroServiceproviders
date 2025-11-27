using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using Providers.Application.Interfaces;
using Providers.Application.Services;
using Providers.Application.Validators;
using Providers.Domain.Entities;
using Providers.Domain.Interfaces;
using Providers.Infrastructure.Persistences;
using Providers.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

// =======================
//  DB (singleton propio)
// =======================
DatabaseConnection.Initialize(builder.Configuration);

// =======================
//  MVC + Swagger
// =======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =======================
//  DI Providers
// =======================
builder.Services.AddScoped<IRepository<Provider>, ProviderRepository>();
builder.Services.AddScoped<IValidator<Provider>, ProviderValidator>();
builder.Services.AddScoped<IProviderService, ProviderService>();

// =======================
//  JWT Auth
//  (mismos valores que en User.Api → appsettings.json: Jwt:Key, Issuer, Audience)
// =======================
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key no configurado");
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// =======================
//  Pipeline
// =======================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();   // 👈 primero autenticación
app.UseAuthorization();    // 👈 luego autorización

app.MapControllers();

app.Run();
