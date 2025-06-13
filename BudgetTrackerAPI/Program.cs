using System.Text.Json.Serialization;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models;
using Repositories;
using Repositories.Interfaces;
using Services;
using Services.Interfaces;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

NpgsqlConnection.GlobalTypeMapper.UseJsonNet();

// Add DbContext for PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication Configuration
var jwtKey = builder.Configuration["JwtSettings:Secret"];
var key = Convert.FromBase64String(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key), // Your JWT signing key
        ValidateIssuer = false,
        ValidateAudience = false
    };
})
.AddCookie("ExternalCookies") // Needed to temporarily store external login
.AddGoogle(googleOptions =>
{
    googleOptions.SignInScheme = "ExternalCookies";
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

// Repositories
builder.Services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
builder.Services.AddScoped<IImportSessionRepository, ImportSessionRepository>();
builder.Services.AddScoped<IImportedTransactionRepository, ImportedTransactionRepository>();
builder.Services.AddScoped<ICategorySuggestionRepository, CategorySuggestionRepository>();
builder.Services.AddScoped<IBudgetTemplateRepository, BudgetTemplateRepository>();
builder.Services.AddScoped<IBudgetTemplateItemRepository, BudgetTemplateItemRepository>();
builder.Services.AddScoped<IUserBudgetRepository, UserBudgetRepository>();
builder.Services.AddScoped<IUserBudgetItemRepository, UserBudgetItemRepository>();
builder.Services.AddScoped<IDashboardLayoutRepository, DashboardLayoutRepository>();
builder.Services.AddScoped<ICategoryKeywordMappingRepository, CategoryKeywordMappingRepository>();

// Services
builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IImportParserService, ImportParserService>();
builder.Services.AddScoped<IImportedTransactionService, ImportedTransactionService>();
builder.Services.AddScoped<IImportSessionService, ImportSessionService>();
builder.Services.AddScoped<IBudgetTemplateService, BudgetTemplateService>();
builder.Services.AddScoped<IUserBudgetService, UserBudgetService>();
builder.Services.AddScoped<IUserBudgetItemService, UserBudgetItemService>();
builder.Services.AddScoped<IDashboardLayoutService, DashboardLayoutService>();
builder.Services.AddScoped<ICategoryKeywordMappingService, CategoryKeywordMappingService>();
builder.Services.AddHttpClient<ICurrencyApiService, FrankfurterApiService>();


//App user
builder.Services.AddScoped<UserManager<ApplicationUser>>();
builder.Services.AddScoped<SignInManager<ApplicationUser>>();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer(); // Enables Swagger for minimal APIs
builder.Services.AddSwaggerGen(); // Registers Swagger

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Budget Tracker API v1");
        options.RoutePrefix = "swagger"; 
    });

}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
