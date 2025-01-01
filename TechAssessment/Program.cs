using Application.Commons;
using Core.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Persistence.Data;
using Stripe;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Stripe API Key
var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
if (string.IsNullOrEmpty(stripeSecretKey))
{
    throw new Exception("Stripe API key is not configured. Set the 'Stripe:SecretKey' in appsettings.json.");
}
StripeConfiguration.ApiKey = stripeSecretKey;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Startup.cs or Program.cs
builder.Services.AddScoped<JwtHelper>();



//StripeConfiguration.ApiKey = "sk_test_51Qc6ELIxaZxNysZrfGKjEH2PquBBHUcN4L2MFYWcW1yCKDgMRIGPgF2XyL2gMb8LJQHp1tKdRxVFtsLgoA8wChYH00thEuf0RY";


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

//JWT authuntication & authorization

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var secretKey = builder.Configuration["JwtSettings:SecretKey"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});
builder.Services.AddAuthorization();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
