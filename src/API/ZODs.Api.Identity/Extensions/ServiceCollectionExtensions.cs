using ZODs.Api.Common.Configuration;
using ZODs.Api.Identity.Configuration;
using ZODs.Api.Identity.Services;
using ZODs.Api.Identity.Services.Interfaces;
using ZODs.Api.Repository.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ZODs.Api.Identity.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddZODsAuthentication(
            this IServiceCollection services,
            IdentityConfiguration configuration,
            GoogleAuthOptions googleAuthOptions)
        {
            services.AddScoped<ITokenService, TokenService>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddGoogle(opt =>
            {
                opt.ClientId = googleAuthOptions.ClientId;
                opt.ClientSecret = googleAuthOptions.ClientSecret;

                opt.Scope.Add("profile");
                opt.SignInScheme = Microsoft.AspNetCore.Identity.IdentityConstants.ExternalScheme;
            })
            .AddJwtBearer(options =>
            {
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.JwtOptions.Key));
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration.JwtOptions.Issuer,
                    ValidAudience = configuration.JwtOptions.Audience,
                    IssuerSigningKey = signingKey,
                };
            });

            //Cookie Policy needed for External Auth
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            });
        }

        public static void AddZODsIdentity<TDbContext>(
            this IServiceCollection services)
            where TDbContext : IdentityDbContext<
                                User,
                                Role,
                                Guid,
                                IdentityUserClaim<Guid>,
                                IdentityUserRole<Guid>,
                                IdentityUserLogin<Guid>,
                                IdentityRoleClaim<Guid>,
                                IdentityUserToken<Guid>>
        {
            services.AddIdentity<User, Role>(opt =>
                    {
                        // Password settings
                        opt.Password.RequireDigit = true;
                        opt.Password.RequiredLength = 8;
                        opt.Password.RequireNonAlphanumeric = true;
                        opt.Password.RequireUppercase = true;
                        opt.Password.RequireLowercase = true;

                        // Lockout settings
                        opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                        opt.Lockout.MaxFailedAccessAttempts = 5;
                        opt.Lockout.AllowedForNewUsers = true;

                        // User settings
                        opt.User.RequireUniqueEmail = true;

                        // Sign In
                        opt.SignIn.RequireConfirmedEmail = true;
                    })
                    .AddEntityFrameworkStores<TDbContext>()
                    .AddDefaultTokenProviders();
        }
    }
}