using Amazon.CloudWatchLogs;
using ZODs.Api.Common.Configuration;
using ZODs.Api.Configuration;
using ZODs.Api.Identity.Configuration;
using Serilog;
using Serilog.Sinks.AwsCloudWatch;
using ZODs.Payment.Configuration;
using Amazon.Runtime;
using Amazon;
using ZODs.AI.OpenAI.Configuration;
using Resend;
using ZODs.Api.Common.Constants;
using System.Threading.RateLimiting;
using ZODs.Api.Extensions;
using ZODs.Api.ExceptionHandling;
using ZODs.Api.Service;
using ZODs.Api.Logging;
using ZODs.AI.Together;
using ZODs.AI.Google.Configuration;

namespace ZODs.Api.Helpers;

public static class StartupHelpers
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ZodsApiConfiguration>(configuration.GetSection(nameof(ZodsApiConfiguration)));
        services.Configure<IdentityConfiguration>(configuration.GetSection(nameof(IdentityConfiguration)));
        services.Configure<JwtAuthConfig>(configuration.GetSection(nameof(JwtAuthConfig)));
        services.Configure<GoogleAuthOptions>(configuration.GetSection(nameof(GoogleAuthOptions)));
        services.Configure<EmailOptions>(configuration.GetSection(nameof(EmailOptions)));
        services.Configure<PaymentConfiguration>(configuration.GetSection(nameof(PaymentConfiguration)));
        services.Configure<MailJetOptions>(configuration.GetSection(nameof(MailJetOptions)));
        services.Configure<OpenAIConfiguration>(configuration.GetSection(nameof(OpenAIConfiguration)));
        services.Configure<ResendClientOptions>(configuration.GetSection(nameof(ResendClientOptions)));
        services.Configure<TogetherAIOptions>(configuration.GetSection(nameof(TogetherAIOptions)));
        services.Configure<GoogleAIOptions>(configuration.GetSection(nameof(GoogleAIOptions)));
        services.Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)));

        return services;
    }

    public static void ConfigureLogging(this ConfigureHostBuilder host, AWSConfiguration awsConfiguration)
    {
        host.UseSerilog((hostContext, loggerConfig) =>
         {
             // Read common settings from JSON file
             loggerConfig.ReadFrom.Configuration(hostContext.Configuration);

             // Additional enrichers, filters, etc.
             loggerConfig.Enrich
             .WithProperty("ApplicationName", hostContext.HostingEnvironment.ApplicationName)
             .Enrich.With<UserDataEnricher>();

             // Only use AWS CloudWatch in Production
             if (hostContext.HostingEnvironment.IsProduction())
             {
                 ArgumentNullException.ThrowIfNull(awsConfiguration, nameof(awsConfiguration));

                 // AWS client
                 var credentials = new BasicAWSCredentials(awsConfiguration.AccessKey, awsConfiguration.SecretKey);
                 var region = RegionEndpoint.GetBySystemName(awsConfiguration.Region);
                 var client = new AmazonCloudWatchLogsClient(credentials, region);

                 loggerConfig.WriteTo.AmazonCloudWatch(
                                 logGroup: awsConfiguration.LogGroup,
                                 logStreamPrefix: DateTime.UtcNow.ToString("yyyy_MM_dd_HH_mm_ss"),
                                 restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                                 textFormatter: new AWSLogFormatter(),
                                 cloudWatchClient: client
                 );
             }
         });
    }

    public static void AddRateLimiters(this IServiceCollection services)
    {
        services.AddCustomApiKeyUserIdFixedWindowRateLimiter(
            RateLimiters.OpenAICustomApiKeyChatRateLimiter,
            10,
            10,
            "You've hit the 10-minutes cap of {0} AI requests in the last {1} minutes. Take this moment to plan your next big move. If you're looking to raise the cap and achieve more, our upgraded plans are [just a click away](https://app.zods.pro/plan).")
                .AddCustomApiKeyUserIdFixedWindowRateLimiter(
            RateLimiters.OpenAICustomApiKeyAILensRateLimiter,
            5,
            10,
            "You've hit the 10-minutes cap of {0} AI requests in the last {1} minutes. Take this moment to plan your next big move. If you're looking to raise the cap and achieve more, our upgraded plans are [just a click away](https://app.zods.pro/plan).");
    }

    public static IServiceCollection AddCustomApiKeyUserIdFixedWindowRateLimiter(
        this IServiceCollection services,
        string rateLimiterName,
        int permitLimit,
        int windowMinutes,
        string errorMessageTemplate)
        => services.AddRateLimiter(options =>
        {
            options.AddPolicy(rateLimiterName,
                    httpContext =>
                      {
                          var task = Task.Run(async () =>
                          {
                              var isPaidPricingPlan = httpContext.User.IsPaidPricingPlan();
                              var userId = httpContext.User.GetUserId();

                              if (!isPaidPricingPlan)
                              {
                                  var userInfoService = httpContext.RequestServices.GetRequiredService<IUserInfoService>();
                                  var userInfo = await userInfoService.GetUserInfoCachedAsync(userId, httpContext.RequestAborted);
                                  isPaidPricingPlan = userInfo.IsPaidPlan;
                              }

                              if (isPaidPricingPlan)
                              {
                                  return RateLimitPartition.GetNoLimiter(userId);
                              }

                              return RateLimitPartition.GetFixedWindowLimiter(
                                  partitionKey: userId,
                                  factory: _ => new FixedWindowRateLimiterOptions
                                  {
                                      PermitLimit = permitLimit,
                                      Window = TimeSpan.FromMinutes(windowMinutes),
                                  });
                          });

                          return task.GetAwaiter().GetResult();
                      });

            options.OnRejected = async (context, ct) =>
                    {
                        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        var errorDetails = new ErrorDetails
                        {
                            StatusCode = StatusCodes.Status429TooManyRequests,
                            Message = string.Format(errorMessageTemplate, permitLimit, windowMinutes),
                        };

                        await context.HttpContext.Response.WriteAsync(
                    errorDetails.ToString(),
                    cancellationToken: ct);
                    };
        });
}
