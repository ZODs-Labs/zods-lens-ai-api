using ZODs.Payment.Clients;
using ZODs.Payment.Configuration;
using ZODs.Payment.Constants;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace ZODs.Payment.Extensions
{
    public static class PaymentServicesExtensions
    {
        public static IServiceCollection AddPaymentProcessorClient(this IServiceCollection services, PaymentConfiguration configuration)
        {
            services.AddHttpClient<LemonSqueezyClient>(HttpClients.PaymentProcessorClient, httpClient =>
            {
                httpClient.BaseAddress = new Uri(configuration.ApiUrl);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(LemonSqueezyConstants.ContentType));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration.ApiKey);

                httpClient.DefaultRequestHeaders.Add("Origin", "https://zods.pro/api");
            });

            return services.AddTransient<IPaymentProcessorClient, LemonSqueezyClient>();
        }
    }
}