using Abjjad.Microblog.Data;
using Abjjad.Microblog.Services;
using Abjjad.Microblog.Background;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services
builder.Services.AddControllersWithViews();

// DbContext - SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=abjjad_microblog.db"));

// JWT settings
var jwtKey = configuration["Jwt:Key"] ?? "ThisIsADevKey-ReplaceInProd!";
var jwtIssuer = configuration["Jwt:Issuer"] ?? "abjjad.local";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

// Authentication: Cookies for MVC, JWT for API
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();

// DI for services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPostService, PostService>();
// For local storage
builder.Services.AddSingleton<IImageStorage, LocalImageStorage>();

// For Azure Blob storage
//builder.Services.AddSingleton<IImageStorage,AzureBlobImageStorage>();
builder.Services.AddSingleton<ImageProcessingQueue>();
builder.Services.AddHostedService<ImageProcessingHostedService>();

var app = builder.Build();

// Ensure database created and seeded
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    ctx.Database.Migrate();
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Timeline}/{action=Index}/{id?}");

app.MapControllers(); // map API controllers too

app.Run();
