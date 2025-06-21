using ZODs.Api.Service.Extensions;
using ZODs.Api.Repository.Extensions;
using ZODs.Api.ExceptionHandling;
using ZODs.Api.Helpers;
using ZODs.Api.Repository;
using Microsoft.IdentityModel.Logging;
using ZODs.Api.Common.Configuration;
using ZODs.Api.Common.Extensions;
using Amazon.SimpleEmail;
using ZODs.Api.Common.Interfaces;
using ZODs.Api.Common.Queues;
using ZODs.Api.HostedServices;
using ZODs.Api.Identity.Extensions;
using ZODs.Api.Identity.Configuration;
using ZODs.Api.Service;
using ZODs.Api.Authorization;
using ZODs.Payment.Configuration;
using ZODs.Payment.Extensions;
using Amazon.Runtime;
using Amazon;
using ZODs.AI.OpenAI.Extensions;
using Serilog;
using ZODs.AI.Together;
using ZODs.Api.Middlewares;
using ZODs.AI.Google.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// Get the environment setting directly from configuration
string environmentName = configuration["ASPNETCORE_ENVIRONMENT"];
bool isProduction = string.Equals(environmentName, "Production", StringComparison.OrdinalIgnoreCase);

configuration.AddJsonFile("serilog.json", optional: false, reloadOnChange: true);

services.AddApiConfiguration(configuration);

var identityConfiguration = configuration.GetSection(nameof(IdentityConfiguration)).Get<IdentityConfiguration>();
var googleAuthOptions = configuration.GetSection(nameof(GoogleAuthOptions)).Get<GoogleAuthOptions>();
var awsConfiguration = configuration.GetSection("AWS").Get<AWSConfiguration>();
var paymentConfiguration = configuration.GetSection(nameof(PaymentConfiguration)).Get<PaymentConfiguration>();

paymentConfiguration.ValidateConfiguration();
awsConfiguration.ValidateConfiguration(isProduction);

var dbConnectionString = builder.Configuration.GetConnectionString("ZODsCS");
if (string.IsNullOrEmpty(dbConnectionString))
    throw new Exception("Database connection string is not set.");

// Serilog configuration
builder.Host.ConfigureLogging(awsConfiguration);

services.AddCmDbContext(dbConnectionString);
services.AddZODsIdentity<ZodsContext>();

services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
services.AddHostedService<QueuedHostedService>();

services.AddHttpClient();
services.AddPaymentProcessorClient(paymentConfiguration);

services.AddRepositories();
services.AddUnitOfWork<ZodsContext>();
services.AddCmServices()
        .AddFeatureLimitationSync()
        .AddFeatureLimitationSyncManager()
        .AddSubscriptionTransitionServices();

// AI services
services.AddOpenAIChatCompletionService()
        .AddOpenAITokenizerService()
        .AddTogetherAIClient()
        .AddGeminiAIClient()
        .AddAIContextService();

services.AddHttpContextAccessor();

var credentials = new BasicAWSCredentials(awsConfiguration.AccessKey, awsConfiguration.SecretKey);
var config = new AmazonSimpleEmailServiceConfig
{
    RegionEndpoint = RegionEndpoint.USEast1 // Set your desired region
};
var client = new AmazonSimpleEmailServiceClient(credentials, config);

services.AddDefaultAWSOptions(configuration.GetAWSOptions());
if (isProduction)
{
    services.AddSingleton<IAmazonSimpleEmailService>(sp => client);
}
else
{
    services.AddAWSService<IAmazonSimpleEmailService>();
}

// Common services
services.AddGoogleAuthService()
        .AddResendEmailService()
        .AddCacheService(configuration.GetConnectionString("RedisCS"));

services.AddCors(c => c.AddPolicy("AllowAll", options =>
{
    options.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));

services.Configure<RouteOptions>(opt => opt.LowercaseUrls = true);

services.AddRateLimiters();
services.AddControllers(opt =>
{
    opt.Filters.Add<ValidSubscriptionAttribute>();
    opt.Filters.Add<FeatureAuthorizationFilter>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddZODsAuthentication(identityConfiguration, googleAuthOptions);
services.AddAuthorization();

var app = builder.Build();

app.ConfigureExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    IdentityModelEventSource.ShowPII = true;
}

app.UseCors("AllowAll");

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseSerilogRequestLogging(opt =>
{
    opt.MessageTemplate = "{UserId} requested: {RequestPath}";
});

app.UseMiddleware<RateLimitingMiddleware>();
app.UseRateLimiter();

app.MapControllers();

// Executes migrations if needed and seeds data if param is set to true
await DbMigrationsHelpers.EnsureDatabasesMigrated(app);

using (var serviceScope = app.Services.CreateScope())
{
    var dataSeedService = serviceScope.ServiceProvider.GetRequiredService<IDataSeedService>();
    await dataSeedService.SeedAsync();
}

app.Run();
