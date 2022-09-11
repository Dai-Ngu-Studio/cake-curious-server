using BusinessObject;
using CakeCurious_API.Utilities;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddControllers(o =>
{
    o.InputFormatters.Insert(o.InputFormatters.Count, new TextPlainInputFormatter());
}).AddJsonOptions(x =>
{
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.AddPooledDbContextFactory<CakeCuriousDbContext>(
    opt => opt
    .EnableDetailedErrors()
    .UseSqlServer(configuration.GetConnectionString("CakeCuriousDb"), sqlServerOptions => sqlServerOptions.CommandTimeout(60))
    );

FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.GetApplicationDefault(),
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Authority = "https://securetoken.google.com/cake-curious";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = "https://securetoken.google.com/cake-curious",
        ValidateAudience = true,
        ValidAudience = "cake-curious",
        ValidateLifetime = true,
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetService<IDbContextFactory<CakeCuriousDbContext>>();
    var context = await factory!.CreateDbContextAsync();
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) { }

app.UseSwagger();
app.UseSwaggerUI();

app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});

app.UseCors(options =>
{
    options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
