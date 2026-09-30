using APIBankingDiplom.TokenWorkings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using APIBankingDiplom.Filters;
using APIBankingDiplom.DBClasses.DBContext;
using APIBankingDiplom.BackgroundServices;
using Microsoft.OpenApi.Models;

// Quick bolt-on
string[] missingVariables = new[]
{
    "API_DATABASE_CONNECTION_STRING",
    "API_AES_SECRETKEY",
    "API_SHA384_SECRETKEY",
    "API_EXCHANGERATE_KEY",
    "API_EMAIL_ADDRESS",
    "API_EMAIL_PASSWORD"
}.Where(variable => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(variable))).ToArray();

if (missingVariables.Length > 0)
    throw new InvalidOperationException($"Missing environment variables: {string.Join(", ", missingVariables)}");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateIdentityFilter>();
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Adding swagger with Bearer Authentication set up
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme 
    {
        In = ParameterLocation.Header,
        Description = "Bearer JWT Token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Accept requests only from specific origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

builder.Services.AddDbContext<BankingContext>();
builder.Services.AddHostedService<InvalidTokensCleanupService>();
builder.Services.AddHostedService<CardOperationsService>();
builder.Services.AddHostedService<CommissionCollectingService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = TokenOptions.AccessTokenValidationParams;
    });

// Exclude NULL variables to keep response clean and payload smaller if possible
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
