using BusinessObject;
using CakeCurious_API.Utilities;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository.Configuration;
using System.Text.Json.Serialization;
using Nest;
using Elasticsearch.Net;
using CakeCurious_API.Services;
using Google.Apis.Services;
using Google.Apis.FirebaseDynamicLinks.v1;
using Google.Cloud.Storage.V1;

var builder = WebApplication.CreateBuilder(args);

var appConfiguration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

// Set web app info
var appInfo = appConfiguration.GetSection("CakeCuriousInfo");
EnvironmentHelper.AddEnvironmentVariables(appInfo);

// Add services to the container.
builder.Services.AddRouting(o =>
{
    o.LowercaseUrls = true;
});

builder.Services.AddControllers(o =>
{
    o.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
    o.InputFormatters.Insert(o.InputFormatters.Count, new TextPlainInputFormatter());
}
).AddJsonOptions(x =>
{
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<CouponExpireCheckService>();

builder.Services.AddCors();

ScopedRepositoryRegister.AddScopedRepositories(builder.Services);

// Configure Elastisearch Client
var elasticUri = new Uri(Environment.GetEnvironmentVariable("ES_SECRET")!);
var elasticPool = new SingleNodeConnectionPool(elasticUri);
var elasticSettings = new ConnectionSettings(elasticPool)
    .DefaultIndex("recipes")
    //.EnableDebugMode()
    .EnableApiVersioningHeader();
var elasticClient = new ElasticClient(elasticSettings);
builder.Services.AddSingleton<IElasticClient>(elasticClient);

builder.Services.RegisterMapsterConfiguration();

builder.Services.AddDbContext<CakeCuriousDbContext>();

// Configure Google Services
var googleCredential = GoogleCredential.GetApplicationDefault();

var firebaseDynamicLinksService = new FirebaseDynamicLinksService(new BaseClientService.Initializer
{
    HttpClientInitializer = googleCredential,
    ApplicationName = "CakeCuriousServer",
});

builder.Services.AddSingleton(firebaseDynamicLinksService);

FirebaseApp.Create(new AppOptions()
{
    Credential = googleCredential,
});

var storageClient = StorageClient.Create();
builder.Services.AddSingleton(storageClient);

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

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
using (var context = scope.ServiceProvider.GetService<CakeCuriousDbContext>())
    context!.Database.Migrate();

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
